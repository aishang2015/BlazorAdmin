using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Setting
{
    [Table("SYSTEM_USER_SETTING")]
    [Comment("用户设置")]
    public class UserSetting
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("用户")]
        public int UserId { get; set; }

        [Comment("键")]
        public string Key { get; set; } = null!;

        [Comment("值")]
        public string Value { get; set; } = null!;
    }
}
