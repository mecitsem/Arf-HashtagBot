using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arf.Services.Common;

namespace Arf.Services.Helpers
{
    public class VisionServiceHelper
    {
        public static string GetSubscriptionKey()
        {
            return ConfigurationHelper.GetAppSettingsValue(Constants.AppSettingsKey.SubscriptionKey);
        }
    }
}
