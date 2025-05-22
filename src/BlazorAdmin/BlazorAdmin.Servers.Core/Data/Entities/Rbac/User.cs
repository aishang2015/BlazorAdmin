using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Servers.Core.Data.Entities.Rbac
{
    [Table("RBAC_USER")]
    [Comment("用户")]
    public class User
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("用户头像")]
        public string? Avatar { get; set; }

        [Comment("用户名")]
        public string Name { get; set; } = null!;

        [Comment("姓名")]
        public string RealName { get; set; } = null!;

        [Comment("密码哈希")]
        public string PasswordHash { get; set; } = null!;

        [Comment("电子邮件")]
        public string? Email { get; set; }

        [Comment("手机号码")]   
        public string? PhoneNumber { get; set; }

        [Comment("是否启用")]
        public bool IsEnabled { get; set; }

        [Comment("是否删除")]
        public bool IsDeleted { get; set; }

        [Comment("是否是特殊用户")]
        public bool IsSpecial { get; set; }

        [Comment("一定时间内登录失败次数")]
        public DateTime? LoginValiedTime { get; set; }
    }
}
