using Money.ApiClient;
using System.Diagnostics;

namespace Money.Shared.Services;

public class CategoryService(MoneyClient moneyClient)
{
    public async Task<List<Category>?> GetCategories()
    {
        ApiClientResponse<CategoryClient.Category[]> apiCategories = null;

        try
        {
            apiCategories = await moneyClient.Category.Get();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        return apiCategories.Content?.Select(apiCategory => new Category
            {
                Id = apiCategory.Id,
                ParentId = apiCategory.ParentId,
                Name = apiCategory.Name,
                Color = apiCategory.Color,
                Order = apiCategory.Order,
                PaymentType = PaymentTypes.Values.First(x => x.Id == apiCategory.PaymentTypeId),
            })
            .ToList();
    }
}
