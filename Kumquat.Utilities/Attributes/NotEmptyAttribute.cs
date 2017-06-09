using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class NotEmptyAttribute : BaseSuccessAttribute
    {
        public NotEmptyAttribute()
        {
        }

        public override bool IsSuccessful(object propertyValue)
        {
            return !string.IsNullOrEmpty(propertyValue?.ToString());
        }
    }
}
