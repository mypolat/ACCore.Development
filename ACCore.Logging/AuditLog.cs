using System.ComponentModel.DataAnnotations;

namespace ACCore.Logging
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string EnvDomainName { get; set; }
        public string EnvMachineName { get; set; }
        public string EnvUsername { get; set; }
        public string UserId { get; set; }
        public string UserIpAddress { get; set; }
        public string Username { get; set; }
        public string Table { get; set; }
        public string Action { get; set; }
        public string PrimaryId { get; set; }
        public string Entity { get; set; }
        public string OriginalValues { get; set; }
        public string CurrentValues { get; set; }
    }
}
