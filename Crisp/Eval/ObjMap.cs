﻿using Crisp.Ast;
using System.Collections.Generic;

namespace Crisp.Eval
{
    class ObjMap : IObj, IIndexable, IMemberGet
    {
        Dictionary<IObj, IObj> items = new Dictionary<IObj, IObj>();

        public ObjMap(Map map, Environment environment)
        {
            foreach (var (index, value) in map.Initializers)
            {
                var objIndex = index.Evaluate(environment);
                var objValue = value.Evaluate(environment);
                IndexSet(objIndex, objValue);
            }
        }

        public IObj IndexGet(IObj index)
        {
            if (items.TryGetValue(index, out IObj value))
            {
                return value;
            }
            else
            {
                throw new RuntimeErrorException(
                    $"map does not contain index '{index}'");
            }
        }

        public IObj IndexSet(IObj index, IObj value)
        {
            items[index] = value;
            return value;
        }

        public (IObj, GetStatus) MemberGet(string name)
        {
            switch (name)
            {
                case "count":
                    return (new ObjInt(items.Count), GetStatus.Got);
                default:
                    return (ObjNull.Instance, GetStatus.NotFound);
            }
        }
    }
}
