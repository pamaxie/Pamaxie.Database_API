using System;

namespace Pamaxie.Data;

/// <summary>
/// Defines how a <see cref="IPamNoSqlObject"/> should be structured
/// </summary>
public interface IPamNoSqlObject
{
    /// <summary>
    /// Unique Key of this NoSql Entry
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Defines the Time To Live for the data object, if set to Null entry does not expire.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public DateTime? TTL { get; set; }
}