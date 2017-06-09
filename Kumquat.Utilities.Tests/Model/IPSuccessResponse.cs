namespace Kumquat.Utilities.Tests
{
    using Kumquat.Utilities.Attributes;

    internal class IPSuccessResponse : IIPSuccessResponse
    {
        [NotEmpty]
        public string Ip { get; set; }
    }
}