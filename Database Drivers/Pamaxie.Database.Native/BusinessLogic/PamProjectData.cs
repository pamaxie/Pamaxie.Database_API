using System;
using System.Collections.Generic;
using Pamaxie.Data;

namespace Pamaxie.Database.Redis;

/// <summary>
/// Defines how <see cref="IPamProject"/> data is stored
/// </summary>
public class PamProjectData : IPamDbObject
{
    /// <summary>
    /// <inheritdoc cref="IPamDbObject.Uid"/>
    /// </summary>
    public string Uid { get; set; }
    
    /// <summary>
    /// Non unique name of the Project
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Owner of this Project
    /// </summary>
    public IPamUser Owner { get; set; }
    
    /// <summary>
    /// Owner organization of this Project if <see cref="IPamProject.ProjectAttributes.OrgOwned"/>
    /// (see <see cref="Flags"/> for that property)
    /// </summary>
    public IPamOrg OwnerOrg { get; set; }
    
    /// <summary>
    /// Api tokens that this API has with the last time they were used
    /// </summary>
    public List<(string Token, DateTime LastUsage)> ApiTokens { get; set; }
    
    /// <summary>
    /// Users that are active for this API
    /// </summary>
    public List<(string UserName, PamProjectPermissions Permission)> Users { get; set; }
    
    /// <summary>
    /// Flags that hold certain boolean values for this object
    /// </summary>
    public IPamProject.ProjectAttributes Flags { get; set; }
    
    
    /// <summary>
    /// <inheritdoc cref="IPamDbObject.TTL"/>
    /// </summary>
    public DateTime? TTL { get; set; }
    
    /// <summary>
    /// Which Objects have been loaded already
    /// </summary>
    public LoadedObjectsEnum LoadedObjects { get; set; }
    
    /// <summary>
    /// Flags Enum that carries which lists are currently loaded
    /// </summary>
    [Flags]
    public enum LoadedObjectsEnum
    {
        /// <summary>
        /// No items are currently loaded
        /// </summary>
        None = 0,
        
        /// <summary>
        /// <see cref="PamProjectData.OwnerOrg"/> is loaded
        /// </summary>
        OwnerOrg = 1,
        
        /// <summary>
        /// <see cref="PamProjectData.Owner"/> is loaded
        /// </summary>
        Projects = 2
    }
}