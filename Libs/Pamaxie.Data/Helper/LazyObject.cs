using System;

namespace Pamaxie.Data;

public class LazyObject<T>
{
    /// <summary>
    /// The data that the object holds
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Is the data loaded, if not some properties in T may not be loaded.
    /// </summary>
    public bool IsLoaded { get; set; }
}