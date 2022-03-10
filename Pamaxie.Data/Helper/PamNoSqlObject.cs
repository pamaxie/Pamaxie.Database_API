using System;

namespace Pamaxie.Data;

public class PamNoSqlObject : IPamNoSqlObject
{
    public string Key { get; set; }
    public DateTime? TTL { get; set; }
}