using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.Core.Interpreter
{
    public class MemorySpace
    {
        private readonly Dictionary<string, object> data = new Dictionary<string, object>();
        private readonly MemorySpace parentSpace;

        public MemorySpace()
        {

        }

        public MemorySpace(MemorySpace parent)
        {
            parentSpace = parent;
        }

        public bool Contains(string key)
        {
            if (data.ContainsKey(key))
                return true;

            if (parentSpace != null)
                return parentSpace.Contains(key);

            return false;
        }

        public object Get(string key)
        {
            if(data.ContainsKey(key))
                return data[key];

            if (parentSpace != null)
                return parentSpace.Get(key);

            throw new Exception($"Unknown variable {key}");
        }

        public void Set(string key, object value)
        {
            data[key] = value;
        }
    }
}
