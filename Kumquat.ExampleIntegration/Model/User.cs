using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//I cheated and generated these with http://json2csharp.com/
namespace Kumquat.ExampleIntegration.Model
{
    using Kumquat.ExampleIntegration.Interfaces;

	public class Geo : IGeo
	{
		public string Lat { get; set; }
		public string Lng { get; set; }
	}

	public class Error : IError
	{
		
	}

	public class Address : IAddress
	{
		public string Street { get; set; }
		public string Suite { get; set; }
		public string City { get; set; }
		public string Zipcode { get; set; }
		public IGeo Geo { get; set; }
	}

	public class Company : ICompany
	{
		public string Name { get; set; }
		public string CatchPhrase { get; set; }
		public string Bs { get; set; }
	}

	public class User : IUser
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public IAddress Address { get; set; }
		public string Phone { get; set; }
		public string Website { get; set; }
		public ICompany Company { get; set; }
	}
}
