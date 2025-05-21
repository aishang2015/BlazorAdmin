using BlazorAdmin.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Chat
{
    [Comment("群聊设置")]
    [Table("CHAT_GROUP_SETTING")]
    [IgnoreAudit]
    public class GroupSetting
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("所属用户")]
        public int UserId { get; set; }

        [Comment("所在群聊")]
        public int GroupId { get; set; }

        [Comment("显示顺序")]
        public int Order { get; set; }
    }
}
