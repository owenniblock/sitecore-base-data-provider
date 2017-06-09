namespace Kumquat.ExampleIntegration.Model
{
	public interface IAddress
	{
		string Street { get; set; }
		string Suite { get; set; }
		string City { get; set; }
		string Zipcode { get; set; }
		IGeo Geo { get; set; }
	}
}