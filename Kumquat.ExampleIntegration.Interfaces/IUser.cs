namespace Kumquat.ExampleIntegration.Model
{
	public interface IUser
	{
		int Id { get; set; }
		string Name { get; set; }
		string Username { get; set; }
		string Email { get; set; }
		IAddress Address { get; set; }
		string Phone { get; set; }
		string Website { get; set; }
		ICompany Company { get; set; }
	}
}