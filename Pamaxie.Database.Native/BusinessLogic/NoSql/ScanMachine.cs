using System;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.NoSql;

public class ScanMachine : IPamNoSqlObject
{
    //Unique key assigned to each machine
    public string Key { get; set; }
    
    //Id of the Project that was used
    public long ProjectId { get; set; }
    
    //Id of the API key that has been used
    public long ApiKeyId { get; set; }

    //Last time someone accessed our API with this key
    public DateTime LastUsed { get; set; }
    
    /// <summary>
    /// TTL to live for this login
    /// </summary>
    public DateTime? TTL { get; set; }
}