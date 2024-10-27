﻿using Blazored.LocalStorage;
using CSharpFunctionalExtensions;
using Money.ApiClient;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AuthData = Money.Shared.Models.AuthData;

namespace Money.Shared.Services;

public class AuthenticationService(
    IHttpClientFactory clientFactory,
    AuthenticationStateProvider authStateProvider,
    ILocalStorage localStorage)
{
    public const string AccessTokenKey = "authToken";
    public const string RefreshTokenKey = "refreshToken";

    private readonly HttpClient _client = clientFactory.CreateClient("api_base");

    public async Task<Result> RegisterAsync(UserDto user)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("Account/register", new { user.Email, user.Password });

        if (response.IsSuccessStatusCode == false)
        {
            ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync());
            string error = problemDetails?.Title ?? "Ошибка регистрации аккаунта на сервере.";
            return Result.Failure(error);
        }

        return Result.Success();
    }

    public async Task<Result> LoginAsync(UserDto user)
    {
        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", user.Email),
            new KeyValuePair<string, string>("password", user.Password),
        ]);

        Result<AuthData> result = null;

        try
        {
            result = await AuthenticateAsync(requestContent);
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex);
        }

        return await result.Tap(async () => await ((AuthStateProvider)authStateProvider).NotifyUserAuthentication());
    }

    public async Task<Result> LogoutAsync()
    {
        await localStorage.RemoveItemsAsync([AccessTokenKey, RefreshTokenKey]);
        await ((AuthStateProvider)authStateProvider).NotifyUserAuthentication();
        return Result.Success();
    }

    public async Task<Result<string>> RefreshTokenAsync()
    {
        string? token = await localStorage.GetItemAsync<string>(AccessTokenKey);
        string? refreshToken = await localStorage.GetItemAsync<string>(RefreshTokenKey);

        if (token == null || refreshToken == null)
        {
            return Result.Failure<string>("Не удалось загрузить токен доступа или токен обновления.");
        }

        FormUrlEncodedContent requestContent = new([
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
        ]);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Result<AuthData> result = await AuthenticateAsync(requestContent);
        return result.Map(data => data.AccessToken);
    }

    private async Task<Result<AuthData>> AuthenticateAsync(FormUrlEncodedContent requestContent)
    {
        HttpResponseMessage response = await _client.PostAsync("connect/token", requestContent);
        _client.DefaultRequestHeaders.Clear();

        if (response.IsSuccessStatusCode == false)
        {
            ProblemDetails? problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync());
            string error = problemDetails?.Title ?? "Не удалось получить данные авторизации.";
            return Result.Failure<AuthData>(error);
        }

        AuthData? result = await response.Content.ReadFromJsonAsync<AuthData>();

        if (result == null)
        {
            return Result.Failure<AuthData>("Некорректный ответ от сервера.");
        }

        await localStorage.SetItemAsync(AccessTokenKey, result.AccessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, result.RefreshToken);
        return result;
    }
}
