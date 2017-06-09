using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class BaseSuccessAttribute : System.Attribute
    {
        public BaseSuccessAttribute()
        {
        }

        public virtual bool IsSuccessful(object propertyValue)
        {
            return false;
        }
    }
}
