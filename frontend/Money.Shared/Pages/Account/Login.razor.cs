﻿using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
//using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Money.Shared.Pages.Account;

public partial class Login
{
    //[CascadingParameter]
    //private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; } = null;

    [Inject]
    private AuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    public async Task LoginUser(EditContext context)
    {
        if (context.Validate() == false)
        {
            return;
        }

        UserDto user = new(Input.Email, Input.Password);

        await AuthenticationService.LoginAsync(user)
            .TapError(message => Snackbar.Add($"Ошибка во время входа {message}", Severity.Error))
            .Tap(() => NavigationManager.ReturnTo(ReturnUrl));
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
