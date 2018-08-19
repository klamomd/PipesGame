﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PipeTap.Utilities
{
    // Unity does not have Tuples, so here's an implementation for me to use.
    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
