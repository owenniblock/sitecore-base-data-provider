using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC
{
    using Kumquat.SAS.SC.Interfaces;
    using Sitecore.Configuration;

    public class SettingsHelper : ISettingsHelper
    {
        public string GetSetting(string name)
        {
            return Settings.GetSetting(name);
        }

        public string GetSetting(string name, string defaultValue)
        {
            return Settings.GetSetting(name, defaultValue);
        }

        public int GetIntSetting(string name, int defaultValue)
        {
            return Settings.GetIntSetting(name, defaultValue);
        }
    }
}
