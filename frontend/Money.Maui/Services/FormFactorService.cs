using Money.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Money.Maui.Services
{
    public class FormFactorService : IFormFactor
    {
        public string GetFormFactor() => DeviceInfo.Idiom.ToString();

        public string GetPlatform() =>
            DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
    }
}
