namespace Kumquat.SAS.SC
{
    using System;
    using System.Collections.Generic;
    using Kumquat.SAS.SC.Interfaces;

    public class DataProviderDamHelperFactory : Dictionary<string, Func<IDataProviderDamHelper>>, IDataProviderDamHelperFactory
    {
        public IDataProviderDamHelper CreateNew(string name)
        {
            return this[name]();
        }
    }
}
