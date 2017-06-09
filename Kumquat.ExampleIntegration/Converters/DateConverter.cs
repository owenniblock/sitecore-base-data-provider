using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.ExampleIntegration.Converters
{
    using Kumquat.SAS.SC.Interfaces;

    public class DateConverter : IConverter
    {
        const string SitecoreDateFormat = "yyyyMMddTHHmmss";
        public string Convert(string input)
        {
            var date = DateTime.MinValue;

            DateTime.TryParse(input, out date);

            return date.ToString(SitecoreDateFormat);
        }
    }
}
