using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC.Interfaces
{
    public interface IDataProviderDamHelperFactory
    {
        IDataProviderDamHelper CreateNew(string name);
    }
}
