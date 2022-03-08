using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Pamaxie.Database.Native.NoSql;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamScanMachineInteraction : PamNoSqlInteractionBase, IPamScanMachineInteraction
{
    public PamScanMachineInteraction(PamaxieDatabaseService owner) : base(owner) { }

    public async Task<(bool wasCreated, string createdId)> CreateAsync(long projectId, long apiKeyId, string machineGuid)
    {
        ScanMachine machine = new ScanMachine()
        {
            Key = machineGuid,
            ApiKeyId = apiKeyId,
            ProjectId = projectId
        };

        return await base.CreateAsync(machine);
    }
}