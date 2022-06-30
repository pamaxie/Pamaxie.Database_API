using System.Threading.Tasks;
using Pamaxie.Data;

namespace Pamaxie.Database.Extensions.DataInteraction;

/// <summary>
/// Interface for managing scan results of data
/// </summary>
public interface IPamScanInteraction : IPamInteractionBase<IPamNoSqlObject, string>
{
    /// <summary>
    /// Gets pre-serialized data
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task<string> GetSerializedData(string key);
    
    
    /// <summary>
    /// Deletes an item via their key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task<bool> DeleteAsync(string key);

    /// <summary>
    /// Gets an item within a specified hamming distance of said item
    /// </summary>
    /// <param name="key">Hash of the item to get</param>
    /// <param name="key">Hamming distance in percent to get from</param>
    /// <returns></returns>
    public Task<IPamNoSqlObject> GetWithHammingDistance(string key, double hammingDistance);
}