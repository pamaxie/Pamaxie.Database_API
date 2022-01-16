using System;
using System.Collections.Generic;
using Pamaxie.Data;

namespace Pamaxie.Database.Redis;

/// <summary>
/// Defines how <see cref="IPamOrg"/> data is stored
/// </summary>
internal sealed class PamOrgData : IPamDbObject
{
    /// <summary>
    /// <inheritdoc cref="IPamDbObject.Uid"/>
    /// </summary>
    public string Uid { get; set; }
    
    /// <summary>
    /// Name of the org
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Unique Guid of the owner of the org
    /// </summary>
    public string OwnerGuid { get; set; }
    
    /// <summary>
    /// Name of the owner
    /// </summary>
    public string OwnerName { get; set; }

    /// <summary>
    /// Owner of the org, please see if loaded via <see cref="LoadedObjectsEnum.Owner"/>
    /// </summary>
    public IPamUser Owner { get; set; }

    /// <summary>
    /// Domain name of the Org
    /// </summary>
    public string DomainName { get; set; }
    
    /// <summary>
    /// Country the org resides/operates in
    /// </summary>
    public Countries Country { get; set; }
    
    /// <summary>
    /// Guids of the projects
    /// </summary>
    public List<string> ProjectGuids { get; set; }
    
    /// <summary>
    /// Projects of the org, please see if loaded via <see cref="LoadedObjectsEnum.Projects"/> flag
    /// </summary>
    public List<IPamProject> Projects { get; set; }

    /// <summary>
    /// Unique Guids of the users that are part of the project
    /// </summary>
    public List<string> UserGuids { get; set; }
    
    /// <summary>
    /// Users that are part of the project, please see if loaded via <see cref="LoadedObjectsEnum.Users"/> flag
    /// </summary>
    public List<IPamUser> Users { get; set; }
    
    /// <summary>
    /// Permissions that the users have
    /// </summary>
    public List<(string UserGuids, PamOrgPermissions Permission)> UserPermissions { get; set; }
    
    /// <summary>
    /// Flags this Organization carries
    /// </summary>
    public IPamOrg.OrgAttributes Flags { get; set; }
    
    /// <summary>
    /// Flags which properties are loaded
    /// </summary>
    public LoadedObjectsEnum LoadedObjects { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="IPamDbObject.TTL"/>
    /// </summary>
    public DateTime? TTL { get; set; }

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
        /// <see cref="PamOrgData.Owner"/> is loaded
        /// </summary>
        Owner = 1,
        
        /// <summary>
        /// <see cref="PamOrgData.Projects"/> is loaded
        /// </summary>
        Projects = 2,
        
        /// <summary>
        /// <see cref="PamOrgData.Users"/> is loaded
        /// </summary>
        Users = 3
    }

    
    
}