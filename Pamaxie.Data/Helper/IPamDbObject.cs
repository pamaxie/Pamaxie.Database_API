using System;

namespace Pamaxie.Data;

/// <summary>
/// Defines how a <see cref="IPamSqlObject"/> should be structured
/// </summary>
public interface IPamSqlObject
{
    /// <summary>
    /// Unique Id of this Database Entry
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Defines the Time To Live for the data object, if set to Null entry does not expire.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public DateTime? TTL { get; set; }
}