namespace Kumquat.ExampleIntegration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Globalization;
	using System.IO;
	using Kumquat.ExampleIntegration.Interfaces;
	using Kumquat.ExampleIntegration.Model;
	using Kumquat.SAS.SC.Interfaces;

	public class ExampleDataProviderDamHelper : IDataProviderDamHelper
	{
		protected readonly IExampleServiceHelper ServiceHelper;
		protected readonly ICacheHandler CacheHandler;
		protected readonly ILogHelper LogHelper;
		protected readonly ISettingsHelper SettingsHelper;
		protected readonly IConverterFactory ConverterFactory;

		public ExampleDataProviderDamHelper(IExampleServiceHelper serviceHelper, ICacheHandler cacheHandler, ILogHelper logHelper, IConverterFactory converterFactory, ISettingsHelper settingsHelper)
		{
			var enabledMethods = new List<DataProviderMethods>();
			enabledMethods.Add(DataProviderMethods.BlobStreamExists);
			enabledMethods.Add(DataProviderMethods.MoveItem);
			enabledMethods.Add(DataProviderMethods.GetBlobStream);
			enabledMethods.Add(DataProviderMethods.GetChildIDs);
			enabledMethods.Add(DataProviderMethods.GetItemDefinition);
			enabledMethods.Add(DataProviderMethods.GetItemFields);
			enabledMethods.Add(DataProviderMethods.GetItemVersions);
			enabledMethods.Add(DataProviderMethods.GetParentID);

			this.EnabledMethods = enabledMethods;
			this.ServiceHelper = serviceHelper;
			this.CacheHandler = cacheHandler;
			this.LogHelper = logHelper;
			this.ConverterFactory = converterFactory;
			this.SettingsHelper = settingsHelper;
		}

		public IEnumerable<DataProviderMethods> EnabledMethods { get; set; }

		public string GetItemName(string key)
		{
			var id = this.GetIdFromKey(key);

			IUser user = null;
			if (this.CacheHandler != null)
			{
				var cacheHit = this.CacheHandler.GetCachedItem<List<IUser>>("ExampleServiceHelper_GetUsers");
				if (cacheHit != null)
				{
					user = cacheHit.FirstOrDefault(x => x.Id == id);
				}
			}

			if (user == null)
			{
				user = this.GetUserFromKey(key);
			}

			if (user == null)
			{
				this.LogHelper.LogError("User was not found from key: " + key, this);
				return "Undefined";
			}

			var value = user.Username;
			var replaceSetting = this.SettingsHelper.GetSetting("ExampleIntegration.Replacements", " - , ");
			var replacements = replaceSetting.Split('|');

			foreach (var replacement in replacements)
			{
				value = value.Replace(replacement, "");
			}

			return value;
		}

		public IEnumerable<string> ProcessingParentFieldNames => new string[] { "ExternalId" };
		public string FolderIdPrefix => "Folder_";

		public IEnumerable<string> GetChildKeys(Dictionary<string, string> values)
		{
			var field = this.ProcessingParentFieldNames.FirstOrDefault();

			if (String.IsNullOrEmpty(field))
			{
				this.LogHelper.LogError("Unexpected empty field for ProcessingParentFieldNames", this);
				return new string[] { };
			}

			var users = this.GetUsers();

			var userIds = users.Select(x => x.Id.ToString(CultureInfo.InvariantCulture));

			return userIds;
		}

		public Stream GetBlobStream(string key)
		{
			return null;
		}

		public bool BlobStreamExists(string key)
		{
			return false;
		}

		public IDictionary<string, string> GetItemFields(string key)
		{
			var output = new Dictionary<string, string>();
			var user = this.GetUserFromKey(key);
			var field = this.ProcessingParentFieldNames.FirstOrDefault();

			if (user == null || String.IsNullOrEmpty(field))
			{
				this.LogHelper.LogError("Either user or field was null: " + key, this);
				return output;
			}

			//TODO: Fill this out.
			output.Add(field, user.Id.ToString(CultureInfo.InvariantCulture));
			

			return output;
		}

		//TODO: Refactor all this...
		private int GetIdFromKey(string key)
		{
			key = key.Replace(this.FolderIdPrefix, string.Empty);
			var keyArray = key.Split('|');

			if (!keyArray.Any())
			{
				this.LogHelper.LogError("keyArray was empty for key: " + key, this);
				return 0;
			}

			int id;
			if (!int.TryParse(keyArray[0], out id))
			{
				this.LogHelper.LogError("Id could not be converted for: " + keyArray[0], this);
				return 0;
			}

			return id;
		}

		private IUser GetUserFromKey(string key)
		{
			key = key.Replace(this.FolderIdPrefix, string.Empty);
			var keyArray = key.Split('|');

			if (!keyArray.Any())
			{
				this.LogHelper.LogError("keyArray was empty for key: " + key, this);
				return null;
			}

			int id;
			if (!int.TryParse(keyArray[0], out id))
			{
				this.LogHelper.LogError("Id could not be converted for: " + keyArray[0], this);
				return null;
			}

			var users = this.ServiceHelper.GetUsers();

			return users.FirstOrDefault(x => x.Id == id);
		}

		private IEnumerable<IUser> GetUsers()
		{
			return this.ServiceHelper.GetUsers();
		}
	}
}
