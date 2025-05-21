using BlazorAdmin.Servers.Core.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Servers.Core.Data.Entities.Chat
{
    [Comment("群聊消息")]
    [Table("CHAT_GROUP_MESSAGE")]
    [IgnoreAudit]
    public class GroupMessage
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("群聊ID")]
        public int GroupId { get; set; }

        [Comment("消息类型")]
        public int MessageType { get; set; }

        [Comment("发送人")]
        public int SenderId { get; set; }

        [Comment("发送时间")]
        public DateTime SendTime { get; set; }

        [Comment("发送内容")]
        public string? Content { get; set; }
    }
}
