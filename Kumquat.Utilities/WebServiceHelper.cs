using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities
{
    using System.IO;
    using System.Net;
    using System.Reflection;
    using Kumquat.SAS.SC.Interfaces;
    using Kumquat.Utilities.Attributes;
    using Kumquat.Utilities.Interfaces;
    using Kumquat.Utilities.Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Formatting = System.Xml.Formatting;

    public class WebServiceHelper : IWebServiceHelper
    {
        protected readonly ILogHelper LogHelper;

        public WebServiceHelper(ILogHelper logHelper)
        {
            this.LogHelper = logHelper;
        }

        public IBaseResponse<TS, TF> CallService<TS, TF>(string url, object request, bool post,
            HttpStatusCode successCode, params JsonConverter[] converters)
        {
            var output = new BaseResponse<TS, TF>();
            var req = WebRequest.Create(url) as HttpWebRequest;

            if (converters == null)
            {
                converters = new JsonConverter[2];
                converters[0] = new TypeConverter<TS>();
                converters[1] = new TypeConverter<TF>();
            }
            
            if (req == null)
            {
                this.LogHelper.LogError("Request was null for url: " + url, this);
                throw new NullReferenceException();
            }

            req.Accept = "application/json; charset=utf-8";
            req.ContentType = "application/json; charset=utf-8";

            if (post)
            {
                req.Method = "POST";
            }

            if (post && request != null)
            {
                var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore
                    };
                var body = JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.None, settings);

                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    streamWriter.Write(body);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }

            using (var response = this.GetHttpResponse(req))
            {
                if (response == null)
                {
                    this.LogHelper.LogError("Response was null for url: " + url, this);
                    throw new NullReferenceException();
                }

                string serviceData;
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                    {
                        this.LogHelper.LogError("Response Stream was null for url: " + url, this);
                        throw new NullReferenceException();
                    }
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    serviceData = reader.ReadToEnd();
                }

                if (!string.IsNullOrEmpty(serviceData))
                {
                    if (response.StatusCode == successCode)
                    {
                        var settings = new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            NullValueHandling = NullValueHandling.Ignore,
                            Converters = converters
                        };
                        var responseItem = JsonConvert.DeserializeObject<TS>(serviceData, settings);
                        
                        output.ResponseItem = responseItem;
                        var success = this.IsCallSuccessful(responseItem);
                        output.Success = success;

                        if (!success)
                        {
                            try
                            {
                                var error = JsonConvert.DeserializeObject<TF>(serviceData, settings);
                                output.ErrorItem = error;
                            }
                            catch (Exception ex)
                            {
                                this.LogHelper.LogError("Unhandled exception occured when deserializing object: " + serviceData, ex, this);
                            }
                            
                        }

                    }
                    else
                    {
                        output.Success = false;
                        //TODO: Try and deserialize - if not - dump it in to the error string...?
                        try
                        {
                            var settings = new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                NullValueHandling = NullValueHandling.Ignore,
                                Converters = converters
                            };
                            var error = JsonConvert.DeserializeObject<TF>(serviceData, settings);
                            output.ErrorItem = error;
                        }
                        catch (Exception ex)
                        {
                            this.LogHelper.LogError("Unhandled exception occured when deserializing object: " + serviceData, ex, this);
                        }
                    }

                    return output;
                }

                response.Close();
            }

            output.Success = false;
            return output;
        }

        public IBaseResponse<TS, TF> CallService<TS, TF>(string url, object request, bool post, params JsonConverter[] converters)
        {
            return this.CallService<TS, TF>(url, request, post, HttpStatusCode.OK, converters);
        }

        public IBaseResponse<TS, TF> CallService<TS, TF>(string url, object request, params JsonConverter[] converters)
        {
            return this.CallService<TS, TF>(url, request, false, HttpStatusCode.OK, converters);
        }

        public IBaseResponse<TS, TF> CallService<TS, TF>(string url, params JsonConverter[] converters)
        {
            return this.CallService<TS, TF>(url, null, false, HttpStatusCode.OK, converters);
        }

        public IBaseResponse<TS, TF> CallService<TS, TF>(string url)
        {
            return this.CallService<TS, TF>(url, null, false, HttpStatusCode.OK, null);
        }

        private HttpWebResponse GetHttpResponse(HttpWebRequest request)
        {
            try
            {
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
				return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    this.LogHelper.LogError("Unhandled exception occured when getting response.", ex, this);
                    throw;
                }

                return (HttpWebResponse)ex.Response;
            }
        }

        public bool IsCallSuccessful(object response)
        {
            var type = response?.GetType();

            if (type == null)
            {
                return false;
            }

            var properties = type.GetProperties(); //.GetProperty(propName).GetValue(src, null)

            foreach (var propertyInfo in properties)
            {
                foreach (object attribute in propertyInfo.GetCustomAttributes(true))
                {
                    if (attribute is BaseSuccessAttribute)
                    {
                        var successTest = attribute as BaseSuccessAttribute;
                        var value = response.GetType().GetProperty(propertyInfo.Name).GetValue(response, null);
                        if (!successTest.IsSuccessful(value))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public Stream GetImageMemoryStreamFromUrl(string url)
        {
            using (var client = new WebClient())
            {
                var imageBytes = client.DownloadData(url);
                var memoryStream = new MemoryStream(imageBytes);
                return memoryStream;
            }
        }

        public void GetImageStreamFromUrl(string url, WebClient client)
        {
            var uri = new Uri(url);
            client.DownloadDataAsync(uri);
        }
    }
}
