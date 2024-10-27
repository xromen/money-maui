using Money.Shared.Interfaces;

namespace Money.Web.Services
{
    public class FormFactorService : IFormFactor
    {
        public string GetFormFactor() => "Web";

        public string GetPlatform() => Environment.OSVersion.ToString();
    }
}
