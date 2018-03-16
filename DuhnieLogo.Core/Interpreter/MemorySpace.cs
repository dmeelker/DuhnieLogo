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

        public bool ContainsLocal(string key)
        {
            key = NormalizeKey(key);

            return data.ContainsKey(key);
        }

        public bool ContainsRecursive(string key)
        {
            key = NormalizeKey(key);

            if (data.ContainsKey(key))
                return true;

            if (parentSpace != null)
                return parentSpace.ContainsRecursive(key);

            return false;
        }

        public object Get(string key)
        {
            key = NormalizeKey(key);

            if (data.ContainsKey(key))
                return data[key];

            if (parentSpace != null)
                return parentSpace.Get(key);

            throw new Exception($"Unknown variable {key}");
        }

        public void Set(string key, object value)
        {
            key = NormalizeKey(key);
            data[key] = value;
        }

        private string NormalizeKey(string key)
        {
            return key.ToLower();
        }
    }
}
