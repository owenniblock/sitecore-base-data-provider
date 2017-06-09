using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class EqualsAttribute : BaseSuccessAttribute
    {
        public EqualsAttribute(object expectedValue)
        {
            this.ExpectedValue = expectedValue;
        }

        public object ExpectedValue { get; }

        public override bool IsSuccessful(object propertyValue)
        {
            return this.ExpectedValue.Equals(propertyValue);
        }
    }
}
