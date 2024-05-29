using BlazorAdmin.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Chat
{
    [Table("CHAT_CHANNEL")]
    [Comment("聊天频道")]
    [IgnoreAudit]
    public class ChatChannel
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("频道类型")]
        public int Type { get; set; }

        [Comment("频道名称")]
        public string? Name { get; set; }
    }

    public enum ChatChannelType
    {
        普通对话 = 1,
        普通群聊 = 2,
        系统对话 = 3,
    }

}
