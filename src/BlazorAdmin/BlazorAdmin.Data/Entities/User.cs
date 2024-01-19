using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities
{
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

        [Comment("是否启用")]
        public bool IsEnabled { get; set; }

        [Comment("是否删除")]
        public bool IsDeleted { get; set; }

        [Comment("是否是特殊用户")]
        public bool IsSpecial { get; set; }
    }
}
