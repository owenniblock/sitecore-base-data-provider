using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kumquat.ExampleIntegration.Website
{
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using Kumquat.ExampleIntegration.Converters;
    using Kumquat.ExampleIntegration.Interfaces;
    using Kumquat.ExampleIntegration.Model;
    using Kumquat.SAS.SC;
    using Kumquat.SAS.SC.Interfaces;
    using Kumquat.Utilities;
    using Kumquat.Utilities.Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class InitialiseContainer
    {
        public static void Initialise()
        {
            var container = Bootstrapper.Container;
            container.Register<IUser, User>();
            container.Register<IAddress, Address>();
            container.Register<ICompany, Company>();
            container.Register<IGeo, Geo>();
            container.RegisterCollection<IUser>(new[] { typeof(IUser) });
            container.Register<IError, Error>();
            container.Register<IDataProviderHelper, DataProviderHelper>();
            container.RegisterSingleton<IDataProviderDamHelperFactory>(new DataProviderDamHelperFactory
            {
                { "example", () => container.GetInstance<ExampleDataProviderDamHelper>() }
            });
            container.RegisterSingleton<IConverterFactory>(new ConverterFactory
            {
                { "DateConverter", () => container.GetInstance<DateConverter>() }
            });
            container.Register<IExampleServiceHelper, ExampleServiceHelper>();
            container.Register<IWebServiceHelper, WebServiceHelper>();
            container.Register<ICacheHandler, CacheHandler>();
            container.Register<ILogHelper, LogHelper>();
            container.Register<ISettingsHelper, SettingsHelper>();
        }
    }
}