using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities.Model
{
    using System.Net;
    using Kumquat.Utilities.Interfaces;

    public class BaseResponse<TS, TF> : IBaseResponse<TS, TF>
    {
        public bool Success { get; set; }
        public HttpStatusCode ResponseStatus { get; set; }
        public TS ResponseItem { get; set; }
        public TF ErrorItem { get; set; }
    }
}
