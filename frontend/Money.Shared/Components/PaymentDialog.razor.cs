﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Money.ApiClient;
using NCalc;
using NCalc.Factories;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Money.Shared.Components;

public partial class PaymentDialog
{
    private static readonly HashSet<char> ValidKeys = ['(', ')', '+', '-', '*', '/'];
    private bool _isNumericSumVisible = true;

    public bool IsOpen { get; private set; }

    [Parameter]
    public Payment Payment { get; set; } = default!;

    [Parameter]
    public EventCallback<Payment> OnSubmit { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = InputModel.Empty;

    [Inject]
    private MoneyClient MoneyClient { get; set; } = default!;

    [Inject]
    private CategoryService CategoryService { get; set; } = default!;

    [Inject]
    private ISnackbar SnackbarService { get; set; } = default!;

    [Inject]
    private IAsyncExpressionFactory Factory { get; set; } = default!;

    public async Task ToggleOpen(PaymentTypes.Value? type = null)
    {
        IsOpen = !IsOpen;

        if (IsOpen)
        {
            List<Category> categories = await CategoryService.GetCategories() ?? [];

            Input = new InputModel
            {
                Category = Payment.Category,
                Comment = Payment.Comment,
                Date = Payment.Date,
                Place = Payment.Place,
                Sum = Payment.Sum,
                CalculationSum = Payment.Sum.ToString(CultureInfo.CurrentCulture),
            };

            // todo обработать, если текущая категория удалена.
            if (type == null)
            {
                Input.CategoryList = [..categories.Where(x => x.PaymentType == Payment.Category.PaymentType)];
                return;
            }

            Input.CategoryList = [..categories.Where(x => x.PaymentType == type)];

            Category? first = Input.CategoryList.FirstOrDefault();

            if (first != null)
            {
                Input.Category = first;
            }
        }
    }

    private async Task ToggleSumFieldAsync()
    {
        if (_isNumericSumVisible)
        {
            Input.CalculationSum = Input.Sum.ToString(CultureInfo.CurrentCulture);
        }
        else if (await ValidateSumAsync() == false)
        {
            return;
        }

        _isNumericSumVisible = !_isNumericSumVisible;
    }

    private async Task<bool> ValidateSumAsync()
    {
        if (_isNumericSumVisible)
        {
            return true;
        }

        decimal? sum = await CalculateAsync();

        if (sum == null)
        {
            return false;
        }

        Input.Sum = sum.Value;
        Input.CalculationSum = Input.Sum.ToString(CultureInfo.CurrentCulture);

        return true;
    }

    private async Task<decimal?> CalculateAsync()
    {
        decimal? sum = null;

        if (string.IsNullOrWhiteSpace(Input.CalculationSum))
        {
            return sum;
        }

        try
        {
            string rawSum = Input.CalculationSum.Replace(',', '.');
            AsyncExpression expression = Factory.Create(rawSum, ExpressionOptions.DecimalAsDefault);

            object? rawResult = await expression.EvaluateAsync();
            sum = Convert.ToDecimal(rawResult);
        }
        catch (Exception)
        {
            SnackbarService.Add("Нераспознано значение в поле 'сумма'.", Severity.Warning);
        }

        return sum;
    }

    private async Task OnSumKeyDown(KeyboardEventArgs args)
    {
        char key = args.Key.Length == 1 ? args.Key[0] : '\0';

        if (key == '\0' || !ValidKeys.Contains(key))
        {
            return;
        }

        await ToggleSumFieldAsync();

        // Костыль, но ‘-’ валидный символ для NumericField, поэтому происходит его повторное добавление
        // InputMode.@decimal / https://developer.mozilla.org/ru/docs/Web/HTML/Global_attributes/inputmode#decimal
        if (key != '-')
        {
            Input.CalculationSum += key;
        }
    }

    private async Task SubmitAsync()
    {
        try
        {
            if (await ValidateSumAsync() == false)
            {
                return;
            }

            await SaveAsync();
            SnackbarService.Add("Операция успешно сохранена!", Severity.Success);

            Payment.Category = Input.Category;
            Payment.Comment = Input.Comment;
            Payment.Date = Input.Date!.Value;
            Payment.Place = Input.Place;
            Payment.Sum = Input.Sum;

            await OnSubmit.InvokeAsync(Payment);
            await ToggleOpen();
        }
        catch (Exception)
        {
            // TODO: добавить логирование ошибки
            SnackbarService.Add("Не удалось сохранить платеж. Пожалуйста, попробуйте еще раз.", Severity.Error);
        }
    }

    private async Task SaveAsync()
    {
        PaymentClient.SaveRequest clientCategory = CreateSaveRequest();

        if (Payment.Id == null)
        {
            ApiClientResponse<int> result = await MoneyClient.Payment.Create(clientCategory);
            Payment.Id = result.Content;
        }
        else
        {
            await MoneyClient.Payment.Update(Payment.Id.Value, clientCategory);
        }
    }

    private PaymentClient.SaveRequest CreateSaveRequest()
    {
        return new PaymentClient.SaveRequest
        {
            CategoryId = Input.Category.Id ?? throw new MoneyException("Идентификатор категории отсутствует при сохранении платежа"),
            Comment = Input.Comment,
            Date = Input.Date!.Value,
            Sum = Input.Sum,
            Place = Input.Place,
        };
    }

    private sealed class InputModel
    {
        public static readonly InputModel Empty = new()
        {
            Category = Category.Empty,
        };

        [Required(ErrorMessage = "Категория обязательна")]
        public required Category Category { get; set; }

        public List<Category>? CategoryList { get; set; }

        [Required(ErrorMessage = "Требуется сумма")]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "Сумма вне допустимого диапазона")]
        public decimal Sum { get; set; }

        public string? CalculationSum { get; set; }

        public string? Comment { get; set; }

        public string? Place { get; set; }

        [Required(ErrorMessage = "Укажите дату")]
        public DateTime? Date { get; set; }
    }
}
