using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Converters;
    using SimpleInjector;

    public class TypeConverter<T> : CustomCreationConverter<T>
    {
        public override T Create(Type objectType)
        {
            var type = typeof(T);
            if (type.IsInterface)
            {
                var container = Bootstrapper.Container;
                return (T)container.GetInstance(type);
            }

            return (T)Activator.CreateInstance(type);
        }
    }
}
