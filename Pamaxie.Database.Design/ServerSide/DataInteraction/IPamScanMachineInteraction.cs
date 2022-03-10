using System;
using System.Threading.Tasks;
using Pamaxie.Data;

namespace Pamaxie.Database.Extensions.DataInteraction;

/// <summary>
/// Interface for managing scan results of data
/// </summary>
public interface IPamScanMachineInteraction : IPamInteractionBase<IPamNoSqlObject, string>
{
    /// <summary>
    /// Creates a new ScanMachine Guid value in the service, throws exception inside the service if the value already exists
    /// </summary>
    /// <exception cref="ArgumentException">The value already exist in the service</exception>
    public Task<(bool wasCreated, string createdId)> CreateAsync(long projectId, long apiKeyId, string machineGuid);
}