using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities.Setting
{
    [Comment("系统设置")]
    public class Setting
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("键")]
        public string Key { get; set; } = null!;

        [Comment("值")]
        public string Value { get; set; } = null!;
    }
}
