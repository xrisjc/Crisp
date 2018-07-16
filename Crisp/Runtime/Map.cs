using System;
using System.Collections.Generic;

namespace Crisp.Runtime
{
    class Map : Entity
    {
        Dictionary<object, object> map;

        public Map(Dictionary<object, object> map)
        {
            this.map = map;
        }

        public IEnumerable<object> Keys => map.Keys;

        public bool TryGetValue(object key, out object value)
        {
            return map.TryGetValue(key, out value);
        }

        public void SetValue(object key, object value)
        {
            map[key] = value;
        }

        public override bool GetAttribute(string name, out object value)
        {
            switch (name)
            {
                case "length":
                    value = map.Count;
                    return true;
            }

            return base.GetAttribute(name, out value);
        }
    }
}
