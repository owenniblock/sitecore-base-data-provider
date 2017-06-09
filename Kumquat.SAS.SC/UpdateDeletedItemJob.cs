using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC
{
    using Sitecore.Data;
    using Sitecore.Data.IDTables;
    using Sitecore.Data.Items;

    public class UpdateDeletedItemJob
    {
        public void RunJob(string idTablePrefix, ID itemId)
        {
            var db = Database.GetDatabase("master");
            var item = db.GetItem(itemId);

            if (item != null)
            {
                item.Editing.BeginEdit();
                using (new EditContext(item))
                {
                    item.Delete();
                }
                item.Editing.EndEdit();
            }

            IDTable.RemoveID(idTablePrefix, itemId);
        }
    }
}
