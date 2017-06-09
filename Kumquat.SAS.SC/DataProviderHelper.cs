using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC
{
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using Kumquat.SAS.SC.Interfaces;
    using Kumquat.Utilities;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Drawing.Exif.Properties;

    public class DataProviderHelper : IDataProviderHelper
    {
        public void MapFields(CoreItem.Builder result, IDictionary<string, string> fields, IEnumerable<string> idFields)
        {
            var database = Factory.GetDatabase("master");
            var settings = database.GetItem(new ID("{D6D93348-9CFD-42A3-9E78-9F602CDCBEB6}"));
            var container = Bootstrapper.Container;
            var publicFieldValue = settings.Fields["Public Fields"];
            var publicFieldNames = publicFieldValue.Value.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            var urlParamsToParse = settings["Mapped Fields"];
            var fieldMappings = Sitecore.Web.WebUtil.ParseUrlParameters(urlParamsToParse);

            var fieldConvertorsParamsToParse = settings["Field Converters"];
            var fieldConvertors = Sitecore.Web.WebUtil.ParseUrlParameters(fieldConvertorsParamsToParse);


            var privateBuilder = new StringBuilder();
            var publicBuilder = new StringBuilder();

            var privateFields = new NameValueCollection();
            var publicFields = new NameValueCollection();
            var converterFactory = container.GetInstance<IConverterFactory>();

            //TODO: Replace idFields with content configured field mapping.
            foreach (var field in fields)
            {
                var value = field.Value;

                Regex rgx = new Regex("[^a-zA-Z0-9-]");
                var key = rgx.Replace(field.Key, "");
                var converterValue = fieldConvertors[key];
                if (converterValue != null)
                {
                    var converter = converterFactory.CreateNew(converterValue);

                    if (converter != null)
                    {
                        value = converter.Convert(value);
                    }
                }

                if (idFields.Contains(field.Key))
                {
                    result.AddField(field.Key, value);
                }

                var mappedValue = fieldMappings[key];
                if (mappedValue != null)
                {
                    result.AddField(mappedValue, value);
                }

                if (publicFieldNames.Contains(key))
                {
                    publicFields.Add(key, value);
                }
                else
                {
                    privateFields.Add(key, value);
                }
            }

            foreach (var key in publicFields.AllKeys)
            {
                var value = publicFields[key];
                if (!String.IsNullOrEmpty(value))
                {
                    publicBuilder.Append(
                        String.Concat(Uri.EscapeDataString(key).Replace("'", "%27"), "=",
                            Uri.EscapeDataString(value).Replace("'", "%27"), "&"));
                }
                else
                {
                    publicBuilder.Append(
                        String.Concat(Uri.EscapeDataString(key).Replace("'", "%27"), "&"));
                }
            }

            foreach (var key in privateFields.AllKeys)
            {
                var value = privateFields[key];
                if (!String.IsNullOrEmpty(value))
                {
                    privateBuilder.Append(
                        String.Concat(Uri.EscapeDataString(key).Replace("'", "%27"), "=",
                            Uri.EscapeDataString(value).Replace("'", "%27"), "&"));
                }
                else
                {
                    privateBuilder.Append(
                        String.Concat(Uri.EscapeDataString(key).Replace("'", "%27"), "&"));
                }
            }

            var privateUpper = "";
            var publicUpper = "";

            if (privateBuilder.Length > 0)
            {
                privateUpper = privateBuilder.ToString(0, privateBuilder.Length - 1);
            }

            if (publicBuilder.Length > 0)
            {
                publicUpper = publicBuilder.ToString(0, publicBuilder.Length - 1);
            }

            Regex reg = new Regex(@"%[a-f0-9]{2}");
            privateUpper = reg.Replace(privateUpper, m => m.Value.ToUpperInvariant()).TrimEnd('=');
            publicUpper = reg.Replace(publicUpper, m => m.Value.ToUpperInvariant()).TrimEnd('=');

            if (privateUpper.Length > 0)
                result.AddField("Private Metadata",
                    privateUpper);
            if (publicUpper.Length > 0)
                result.AddField("Public Metadata", publicUpper);

        }

        public ID ResolveTemplateId(string itemName, string key, string folderIdPrefix)
        {
            if (key.StartsWith(folderIdPrefix))
            {
                var folderTemplateId = Settings.GetSetting("Kumquat.FolderTemplateId");
                return new ID(folderTemplateId);
            }

            var extension = Path.GetExtension(itemName);

            var defaultTemplateId = Settings.GetSetting("Kumquat.Template.Default");

            var prefix = "Kumquat.Template";
            var setting = String.Concat(prefix, extension.ToUpperInvariant());
            var templateId = Settings.GetSetting(setting);

            if (!String.IsNullOrEmpty(templateId))
            {
                return new ID(templateId);
            }

            return new ID(defaultTemplateId);
        }
    }
}
