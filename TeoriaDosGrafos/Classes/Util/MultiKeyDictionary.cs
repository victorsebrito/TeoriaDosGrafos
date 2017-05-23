﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeoriaDosGrafos.Classes.Util
{
    public class MultiKeyDictionary<T1, T2, T3> : Dictionary<T1, Dictionary<T2, T3>>
    {
        new public Dictionary<T2, T3> this[T1 key]
        {
            get
            {
                if (!ContainsKey(key))
                    Add(key, new Dictionary<T2, T3>());

                Dictionary<T2, T3> returnObj;
                TryGetValue(key, out returnObj);

                return returnObj;
            }
        }
    }
}
