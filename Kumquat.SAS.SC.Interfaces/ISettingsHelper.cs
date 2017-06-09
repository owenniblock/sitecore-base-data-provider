using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC.Interfaces
{
    public interface ISettingsHelper
    {
        string GetSetting(string name);
        string GetSetting(string name, string defaultValue);
        int GetIntSetting(string name, int defaultValue);
    }
}
