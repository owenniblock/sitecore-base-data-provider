namespace Kumquat.ExampleIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Kumquat.ExampleIntegration.Interfaces;
    using Kumquat.ExampleIntegration.Model;
    using Kumquat.SAS.SC.Interfaces;
    using Kumquat.Utilities;
    using Kumquat.Utilities.Interfaces;

    public class ExampleServiceHelper : IExampleServiceHelper
    {
        protected readonly IWebServiceHelper ServiceHelper;
        protected readonly ICacheHandler CacheHandler;
        protected readonly ILogHelper LogHelper;
        protected readonly ISettingsHelper SettingsHelper;

        public ExampleServiceHelper(IWebServiceHelper serviceHelper, ICacheHandler cacheHandler, ILogHelper logHelper, ISettingsHelper settingsHelper)
        {
            this.ServiceHelper = serviceHelper;
            this.CacheHandler = cacheHandler;
            this.LogHelper = logHelper;
            this.SettingsHelper = settingsHelper;
        }

        public List<IUser> GetUsers()
        {
            if (this.CacheHandler != null)
            {
                var cacheHit = this.CacheHandler.GetCachedItem<List<IUser>>("ExampleServiceHelper_GetUsers");
                if (cacheHit != null)
                {
                    return cacheHit;
                }
            }

            var url = this.SettingsHelper.GetSetting("ExampleIntegration.ServiceUrl", "https://jsonplaceholder.typicode.com/users");

            var response = this.ServiceHelper.CallService<List<IUser>, IError>(url, new TypeConverter<IUser>(), 
				new TypeConverter<IAddress>(), new TypeConverter<ICompany>(), new TypeConverter<IGeo>(), new TypeConverter<IError>());

            if (response.Success)
            {
                if (this.CacheHandler != null)
                {
                    this.CacheHandler.SaveCachedItem<List<IUser>>("ExampleServiceHelper_GetUsers",
                        response.ResponseItem, DateTime.UtcNow.AddMinutes(5));
                }

                return response.ResponseItem;
            }
            else
            {
                this.LogHelper.LogError("Web service call was unsuccessful", this);
                return null;
            }
        }
    }
}
