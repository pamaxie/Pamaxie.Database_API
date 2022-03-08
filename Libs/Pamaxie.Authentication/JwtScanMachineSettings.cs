using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Pamaxie.Authentication
{
    /// <summary>
    /// Authentication token generator
    /// </summary>
    public sealed class JwtScanMachineSettings
    {
        public string ScanMachineGuid { get; set; }
        public long ProjectId { get; set; }
        public bool IsScanMachine { get; set; }
        
        public bool IsPamMachine { get; set; }
    }
}