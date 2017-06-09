using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC
{
    using Kumquat.SAS.SC.Interfaces;
    using Sitecore.Diagnostics;

    public class LogHelper : ILogHelper
    {
        public void LogError(string message, object owner)
        {
            Log.Error(message, owner);
        }

        public void LogError(string message, Exception exception, object owner)
        {
            Log.Error(message, exception, owner);
        }

        public void LogError(string message, Exception exception, Type ownerType)
        {
            Log.Error(message, exception, ownerType);
        }

        public void LogError(string message, Exception exception, string loggerName)
        {
            Log.Error(message, exception, loggerName);
        }

        public void LogError(string message, Type ownerType)
        {
            Log.Error(message, ownerType);
        }
    }
}
