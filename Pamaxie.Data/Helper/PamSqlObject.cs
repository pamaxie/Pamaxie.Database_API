using System;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.NoSql;

public sealed class PamSqlObject : IPamNoSqlObject
{
    /// <summary>
    /// <inheritdoc cref="Key"/>
    /// </summary>
    public string Key { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="TTL"/>
    /// </summary>
    public DateTime? TTL { get; set; }
}