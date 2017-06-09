namespace Kumquat.Utilities.Interfaces
{
    using System.Net;

    public interface IBaseResponse<TS, TF>
    {
        bool Success { get; set; }

        HttpStatusCode ResponseStatus { get; set; }

        TS ResponseItem { get; set; }

        TF ErrorItem { get; set; }
    }
}