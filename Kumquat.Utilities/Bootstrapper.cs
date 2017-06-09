using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities
{
    using SimpleInjector;

    public static class Bootstrapper
    {
        public static readonly Container Container;

        static Bootstrapper()
        {
            var container = new Container();

            Container = container;
        }
    }
}
