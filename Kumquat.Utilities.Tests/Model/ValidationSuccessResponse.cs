namespace Kumquat.Utilities.Tests
{
    using Kumquat.Utilities.Attributes;

    internal class ValidationSuccessResponse
    {
        [Equals(true)]
        public bool Validate { get; set; }
    }
}