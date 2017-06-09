namespace Kumquat.SAS.SC
{
    using System;
    using System.Collections.Generic;
    using Kumquat.SAS.SC.Interfaces;

    public class ConverterFactory : Dictionary<string, Func<IConverter>>, IConverterFactory
    {
        public IConverter CreateNew(string name)
        {
            return this[name]();
        }
    }
}
