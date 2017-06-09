using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC.Interfaces
{
    public interface ILogHelper
    {
        void LogError(string message, object owner);
        void LogError(string message, Exception exception, object owner);
        void LogError(string message, Exception exception, Type ownerType);
        void LogError(string message, Exception exception, string loggerName);
        void LogError(string message, Type ownerType);
    }
}
