using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Log
{
    [Comment("审计日志")]
    [Index(nameof(TransactionId))]
    [Index(nameof(OperateTime))]
    public class AuditLog
    {
        [Comment("主键")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Comment("事务Id")]
        public Guid TransactionId { get; set; }

        [Comment("用户Id")]
        public int UserId { get; set; }

        [Comment("实体名称")]
        public string EntityName { get; set; } = null!;

        [Comment("2删除 3修改 4添加")]
        public int Operation { get; set; }

        [Comment("操作时间")]
        public DateTime OperateTime { get; set; }
    }
}
