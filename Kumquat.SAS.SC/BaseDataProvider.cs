namespace Kumquat.SAS.SC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using Kumquat.SAS.SC.Interfaces;
    using Kumquat.Utilities;
    using Sitecore;
    using Sitecore.Caching;
    using Sitecore.Collections;
    using Sitecore.Configuration;
    using Sitecore.ContentSearch;
    using Sitecore.Data;
    using Sitecore.Data.DataProviders;
    using Sitecore.Data.IDTables;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.Jobs;
    using Sitecore.Reflection;
    using Sitecore.Resources.Media;

    public class BaseDataProvider : DataProvider
    {
        const string SitecoreDateFormat = "yyyyMMddTHHmmss";

        protected readonly IDataProviderDamHelper DataProviderDamHelper;
        protected readonly IDataProviderHelper DataProviderHelper;
        protected readonly ISettingsHelper SettingsHelper;
        protected readonly string IdTablePrefix;
        protected readonly string ParentTemplateId;

        public BaseDataProvider(string dataProviderName, string idTablePrefix, string parentTemplateId)
        {
            this.IdTablePrefix = idTablePrefix;
            this.ParentTemplateId = parentTemplateId;

            var container = Bootstrapper.Container;
            this.DataProviderHelper = container.GetInstance<IDataProviderHelper>();
            this.SettingsHelper = container.GetInstance<ISettingsHelper>();
            var factory = container.GetInstance<IDataProviderDamHelperFactory>();
            this.DataProviderDamHelper = factory.CreateNew(dataProviderName);
        } 

        public override VersionUriList GetItemVersions(ItemDefinition itemDefinition, CallContext context)
        {
            if (this.CanProcessItem(itemDefinition.ID, context))
            {
                var database = Factory.GetDatabase("master");
                var versionUriList = new VersionUriList();
                foreach (var language in LanguageManager.GetLanguages(database))
                {
                    versionUriList.Add(language, Sitecore.Data.Version.First);
                }
                context.Abort();
                return versionUriList;
            }
            return base.GetItemVersions(itemDefinition, context);
        }

        public override ItemDefinition GetItemDefinition(ID itemId, CallContext context)
        {
            ItemDefinition itemDef = null;
            if (this.CanProcessItem(itemId, context))
            {
                context.Abort();

                var key = this.GetIdTableExternalKey(itemId, this.IdTablePrefix);
                var itemName = this.DataProviderDamHelper.GetItemName(key);

                if (itemName == "Undefined")
                {
                    var job = JobManager.Start(new JobOptions(this.IdTablePrefix + "Delete", this.IdTablePrefix, this.IdTablePrefix,
                                                      new UpdateDeletedItemJob(), "RunJob", new object[] { this.IdTablePrefix, itemId })
                    {
                        ContextUser = Context.User,
                        AfterLife = TimeSpan.FromMilliseconds(Settings.Publishing.PublishDialogPollingInterval * 50),
                        Priority = Settings.Publishing.ThreadPriority
                    });

                    return new ItemDefinition(ID.Null, itemName, ID.Null, ID.Null);
                }

                var templateId = this.DataProviderHelper.ResolveTemplateId(itemName, key, this.DataProviderDamHelper.FolderIdPrefix);

                itemDef = new ItemDefinition(itemId, ItemUtil.ProposeValidItemName(Path.GetFileNameWithoutExtension(itemName)), templateId, ID.Null);

                try
                {
                    ItemCache itemCache = CacheManager.GetItemCache(context.DataManager.Database);
                    if (itemCache != null)
                    {
                        ReflectionUtil.CallMethod(itemCache, "RemoveItem", true, false, false, new object[] { itemDef.ID });
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Can't clear cache for the item", exception, this);
                }
                // Cache items for this data provider
                ((ICacheable)itemDef).Cacheable = true;
            }
            return itemDef;
        }

        public override IDList GetChildIDs(ItemDefinition itemDefinition, CallContext context)
        {
            if (this.CanProcessParent(itemDefinition.ID, context))
            {
                context.Abort();

                var idList = new IDList();
                ID parentId = itemDefinition.ID;

                var database = context.DataManager.Database;
                var actualItem = database.GetItem(parentId);

                var values = this.DataProviderDamHelper.ProcessingParentFieldNames.ToDictionary(fieldName => fieldName, fieldName => actualItem[fieldName]);

                var keyList = this.DataProviderDamHelper.GetChildKeys(values);

                var keys = keyList as string[] ?? keyList.ToArray();
                if (!keys.Any())
                {
                    return new IDList();
                }

                foreach (var key in keys)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    var keyString = key + "|" + parentId.ToShortID();

                    IDTableEntry idEntry = IDTable.GetID(this.IdTablePrefix,
                            keyString);
                    ID newID;
                    if (idEntry == null)
                    {
                        var itemName = this.DataProviderDamHelper.GetItemName(key);

                        if (itemName == "Undefined")
                        {
                            continue;
                        }

                        newID = ID.NewID;
                        IDTable.Add(this.IdTablePrefix,
                            keyString,
                            newID, parentId, ItemUtil.ProposeValidItemName(Path.GetFileNameWithoutExtension(itemName)));
                    }
                    else
                    {
                        newID = idEntry.ID;
                    }
                    idList.Add(newID);
                }
                
                context.DataManager.Database.Caches.DataCache.Clear();

                return idList;
            }

            return base.GetChildIDs(itemDefinition, context);
        }

        public override ID GetParentID(ItemDefinition itemDefinition, CallContext context)
        {
            if (this.CanProcessItem(itemDefinition.ID, context))
            {

                context.Abort();

                IDTableEntry[] idEntries = IDTable.GetKeys(this.IdTablePrefix, itemDefinition.ID);

                if (idEntries != null && idEntries.Length > 0)
                {

                    return idEntries[0].ParentID;

                }

                return null;
            }

            return base.GetParentID(itemDefinition, context);
        }

        //This one is slightly different - we're not expecting it to be enabled.
        public override LanguageCollection GetLanguages(CallContext context)
        {
            return (LanguageCollection)null;
        }

        //This one is slightly different - we're not expecting it to be enabled.
        public override bool MoveItem(ItemDefinition itemDefinition, ItemDefinition destination, CallContext context)
        {
            return true;
        }


        public override FieldList GetItemFields(ItemDefinition itemDefinition, VersionUri versionUri, CallContext context)
        {
            if (this.CanProcessItem(itemDefinition.ID, context))
            {
                context.Abort();

                if (context.DataManager.DataSource.ItemExists(itemDefinition.ID))
                {

                    ReflectionUtil.CallMethod(
                        typeof (ItemCache), CacheManager.GetItemCache(context.DataManager.Database),
                        "RemoveItem", true, true, new object[] { itemDefinition.ID});
                }

                var sitecoreId = itemDefinition.ID;

                var key = this.GetIdTableExternalKey(sitecoreId, this.IdTablePrefix);
                var itemName = this.DataProviderDamHelper.GetItemName(key);

                if (itemName == "Undefined")
                {
                    return base.GetItemFields(itemDefinition, versionUri, context);
                }

                var result = new CoreItem.Builder(itemDefinition.ID, ItemUtil.ProposeValidItemName(Path.GetFileNameWithoutExtension(itemName)), 
					itemDefinition.TemplateID, context.DataManager);

				result.AddField("__Created", DateTime.Now.ToString(SitecoreDateFormat));
				result.AddField("__Updated", DateTime.Now.ToString(SitecoreDateFormat));
				result.AddField("__Owner", this.SettingsHelper.GetSetting("Kumquat.IconFormat", "sitecore\\admin"));

				var fields = this.DataProviderDamHelper.GetItemFields(key);
                this.DataProviderHelper.MapFields(result, fields, this.DataProviderDamHelper.ProcessingParentFieldNames);

	            var values = result.ItemData.Fields[new ID("{E992FBF9-2F76-4BBA-A613-D17087AA609C}")];
	            values += result.ItemData.Fields[new ID("{3D70C570-614F-455A-8DEF-E4424033BCC5}")];

				var revision = GetHashString(values);
	            result.AddField("__Revision", revision);

				if (key.StartsWith(this.DataProviderDamHelper.FolderIdPrefix))
                {
                    result.AddField("__Sortorder", "1");
                    return result.ItemData.Fields;
                }

	            if (this.DataProviderDamHelper.EnabledMethods.Contains(DataProviderMethods.BlobStreamExists))
	            {
		            result.AddField("Blob", itemDefinition.ID.ToString());

					var iconFormat = this.SettingsHelper.GetSetting("Kumquat.IconFormat",
					"-/media/{0}.ashx?h=16&thn=1&w=16");
					var iconUrl = String.Format(iconFormat, itemDefinition.ID.ToShortID());
					result.AddField("__Icon", iconUrl);

					var extension = "";
					if (!String.IsNullOrEmpty(itemName))
					{
						extension = itemName.Split('.').Last();
					}
					var resolver = new MimeResolver();
					var mimeType = resolver.GetMimeType(extension);
					result.AddField("Extension", extension);
					result.AddField("Mime Type", mimeType);
				}

                //result.AddField("Size", fields["size"]);
                result.AddField("Linked Asset", "1");
                result.AddField("__Sortorder", "2");

                return result.ItemData.Fields;
            }

            return base.GetItemFields(itemDefinition, versionUri, context);
        }

	    private static string GetHashString(string values)
	    {
		    var valueBytes = Encoding.ASCII.GetBytes(values);
		    System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
		    var hash = md5.ComputeHash(valueBytes);
		    var base64 = System.Convert.ToBase64String(hash);
		    return base64;
	    }

	    public override bool SaveItem(ItemDefinition itemDefinition, ItemChanges changes, CallContext context)
        {
            if (!this.DataProviderDamHelper.EnabledMethods.Contains(DataProviderMethods.SaveItem))
            {
                throw new NotSupportedException();
            }

            return base.SaveItem(itemDefinition, changes, context);
        }

        public override Stream GetBlobStream(Guid blobId, CallContext context)
        {
            if (!this.DataProviderDamHelper.EnabledMethods.Contains(DataProviderMethods.GetBlobStream))
            {
                throw new NotSupportedException();
            }

            if (this.CanProcessItem(new ID(blobId), context))
            {
                var key = this.GetIdTableExternalKey(new ID(blobId), this.IdTablePrefix);
                return this.DataProviderDamHelper.GetBlobStream(key);

            }

            return base.GetBlobStream(blobId, context);
        }

        public override bool BlobStreamExists(Guid blobId, CallContext context)
        {
            if (!this.DataProviderDamHelper.EnabledMethods.Contains(DataProviderMethods.BlobStreamExists))
            {
                throw new NotSupportedException();
            }

            if (this.CanProcessItem(new ID(blobId), context))
            {
                var key = this.GetIdTableExternalKey(new ID(blobId), this.IdTablePrefix);
                return this.DataProviderDamHelper.BlobStreamExists(key);
            }

            return base.BlobStreamExists(blobId, context);
        }

        private bool CanProcessItem(ID id, CallContext context)
        {
            if (IDTable.GetKeys(this.IdTablePrefix, id).Length > 0)
            {
                return true;
            }

            return false;
        }

        private bool CanProcessParent(ID id, CallContext context)
        {
            var database = context.DataManager.Database;
            var item = database.Items[id];
            var canProduce = item != null && new ID(this.ParentTemplateId) == item.TemplateID;
            return canProduce;
        }

        private string GetIdTableExternalKey(ID id, string prefix)
        {
            var idEntries = IDTable.GetKeys(prefix, id);

            if (idEntries == null || !idEntries.Any() || string.IsNullOrEmpty(idEntries[0].Key))
            {
                return null;
            }

            return idEntries[0].Key;
        }
    }
}
