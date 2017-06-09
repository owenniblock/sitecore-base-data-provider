using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC.Interfaces
{
    using System.IO;

    public interface IDataProviderDamHelper
    {
        IEnumerable<DataProviderMethods> EnabledMethods { get; set; }
        string GetItemName(string key);
        IEnumerable<string> ProcessingParentFieldNames { get; }
        string FolderIdPrefix { get; }
        IEnumerable<string> GetChildKeys(Dictionary<string, string> values);
        Stream GetBlobStream(string key);
        bool BlobStreamExists(string key);
        IDictionary<string, string> GetItemFields(string key);
    }
}
