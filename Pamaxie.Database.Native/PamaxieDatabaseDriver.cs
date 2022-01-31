using System;
using Pamaxie.Database.Extensions;

namespace Pamaxie.Database.Native;

/// <inheritdoc cref="IPamaxieDatabaseDriver"/>
public sealed class PamaxieDatabaseDriver : IPamaxieDatabaseDriver
{
    public PamaxieDatabaseDriver()
    {
        this.DatabaseTypeGuid = new Guid("283f10c3-5022-4cda-a5a7-e491a1e84b32");
        Service = new PamaxieDatabaseService(this);
        Configuration = new PamaxieDatabaseConfig(this);
    }

    /// <inheritdoc cref="IPamaxieDatabaseDriver.DatabaseTypeName"/>
    public string DatabaseTypeName => "Native";

    /// <inheritdoc cref="IPamaxieDatabaseDriver.DatabaseTypeGuid"/>
    public Guid DatabaseTypeGuid { get; }

    /// <inheritdoc cref="IPamaxieDatabaseDriver.Configuration"/>
    public IPamaxieDatabaseConfiguration Configuration { get; }

    /// <inheritdoc cref="IPamaxieDatabaseDriver.Service"/>
    public IPamaxieDatabaseService Service { get; }
}