using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.ExampleIntegration.Interfaces
{
    using System.IO;
    using Kumquat.ExampleIntegration.Model;

	public interface IExampleServiceHelper
    {
        List<IUser> GetUsers();
    }
}
