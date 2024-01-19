using BlazorAdmin.Data.Attributes;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities.Log
{
    [Comment("登录日志")]
    [IgnoreAudit]
    public class LoginLog
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("登录名")]
        public string UserName { get; set; } = null!;

        [Comment("登录时间")]
        public DateTime Time { get; set; }

        [Comment("登录客户端")]
        public string? Agent { get; set; }

        [Comment("登录IP")]
        public string? Ip { get; set; }

        [Comment("是否成功")]
        public bool IsSuccessd { get; set; }
    }
}
