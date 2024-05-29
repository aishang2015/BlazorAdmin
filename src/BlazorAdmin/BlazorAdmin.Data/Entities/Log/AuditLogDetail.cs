using BlazorAdmin.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Log
{
    [Table("LOG_AUDIT_DETAIL")]
    [Comment("审计日志详情")]
    [IgnoreAudit]
    [Index(nameof(AuditLogId))]
    public class AuditLogDetail
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("审计日志Id")]
        public Guid AuditLogId { get; set; }

        [Comment("实体名称")]
        public string EntityName { get; set; } = null!;

        [Comment("属性名")]
        public string PropertyName { get; set; } = null!;

        [Comment("旧值")]
        public string? OldValue { get; set; }

        [Comment("新值")]
        public string? NewValue { get; set; }
    }
}
