//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OSD_Auto_Unlock
{
    using System;
    
    public partial class GetRecoveryKeyForUser_Result
    {
        public string RecoveryKeyId { get; set; }
        public string RecoveryKey { get; set; }
        public byte[] BitLockerRecoveryKeyPackage { get; set; }
        public byte[] DeprecatedRecoveryKeyPackage { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public string VolumeGuid { get; set; }
        public string MachineName { get; set; }
        public string MachineDomainName { get; set; }
    }
}
