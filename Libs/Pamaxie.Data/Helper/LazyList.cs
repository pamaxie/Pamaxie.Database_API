using System;
using System.Collections;
using System.Collections.Generic;

namespace Pamaxie.Data;

public class LazyList<T> : List<T>
{
    public bool IsLoaded { get; set; }
}