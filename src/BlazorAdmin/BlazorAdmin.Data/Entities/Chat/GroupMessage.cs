using BlazorAdmin.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Data.Entities.Chat
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
