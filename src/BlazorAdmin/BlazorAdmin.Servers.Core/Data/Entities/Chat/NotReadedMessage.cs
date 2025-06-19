using BlazorAdmin.Servers.Core.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Servers.Core.Data.Entities.Chat
{
    [Comment("未读消息")]
    [Table("CHAT_NOT_READED_MESSAGE")]
    [IgnoreAudit]
    public class NotReadedMessage
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("用户")]
        public int UserId { get; set; }

        [Comment("群聊ID")]
        public int? GroupId { get; set; }

        [Comment("发送用户ID")]
        public int? SendUserId { get; set; }

        [Comment("消息ID")]
        public int MessageId { get; set; }
    }
}
