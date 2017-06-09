using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities.Interfaces
{
    using System.IO;
    using System.Net;
    using Newtonsoft.Json;

    public interface IWebServiceHelper
    {
        IBaseResponse<TS, TF> CallService<TS, TF>(string url, object request, bool post,
            HttpStatusCode successCode, params JsonConverter[] converters);
        IBaseResponse<TS, TF> CallService<TS, TF>(string url, object request, bool post, params JsonConverter[] converters);
        IBaseResponse<TS, TF> CallService<TS, TF>(string url, object request, params JsonConverter[] converters);
        IBaseResponse<TS, TF> CallService<TS, TF>(string url, params JsonConverter[] converters);
        IBaseResponse<TS, TF> CallService<TS, TF>(string url);
        Stream GetImageMemoryStreamFromUrl(string url);
        void GetImageStreamFromUrl(string url, WebClient client);
    }
}
