﻿namespace Money.Shared.Models;

public class PaymentsDay
{
    public DateTime Date { get; set; }

    public List<Payment> Payments { get; set; } = [];

    public decimal CalculateSum(PaymentTypes.Value paymentType)
    {
        return Payments.Where(x => x.IsDeleted == false && x.Category.PaymentType == paymentType)
            .Sum(x => x.Sum);
    }
}
