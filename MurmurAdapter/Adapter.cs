using MurmurPlugin;
using System;
using System.Linq;
using System.Reflection;

namespace MurmurAdapter
{
    public class Adapter
    {
        public IInstance Instance;

        public Adapter(string version)
        {
            init(new MurmurVersion(version));
        }

        public Adapter(MurmurVersion version)
        {
            init(version);
        }
    
        private void init(MurmurVersion version)
        {
            var assemblyName = MurmurVersion.GetFileNameFromVersion(version.ToString());
            Assembly assembly = Assembly.LoadFrom(assemblyName);
            Type T = assembly.GetType("Murmur.Instance");
            Instance = (IInstance)Activator.CreateInstance(T);
        }
    }
}
