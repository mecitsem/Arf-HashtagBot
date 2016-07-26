using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arf.Services.Helpers
{
    public class ConfigurationHelper
    {
        public static string GetAppSettingsValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
