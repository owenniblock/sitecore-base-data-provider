using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC.Interfaces
{
    using Sitecore.Data;
    using Sitecore.Data.Items;

    public interface IDataProviderHelper
    {
        void MapFields(CoreItem.Builder result, IDictionary<string, string> fields, IEnumerable<string> idFields);

        ID ResolveTemplateId(string itemName, string key, string folderIdPrefix);
    }
}
