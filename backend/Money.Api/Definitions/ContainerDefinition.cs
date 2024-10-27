﻿using Money.Business;

namespace Money.Api.Definitions;

public class ContainerDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<RequestEnvironment>();

        builder.Services.AddScoped<AccountService>();
        builder.Services.AddScoped<AuthService>();

        builder.Services.AddScoped<CategoryService>();
        builder.Services.AddScoped<PaymentService>();
        builder.Services.AddScoped<FileService>();
    }
}
