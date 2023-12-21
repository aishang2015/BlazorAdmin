using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Data.Entities
{
    [Comment("聊天消息")]
    public class ChatMessage
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("消息类型")]
        public int Type { get; set; }

        [Comment("发送人Id")]
        public int SenderId { get; set; }

        [Comment("发送时间")]
        public DateTime CreatedTime { get; set; }

        [Comment("发送内容")]
        public string? Content { get; set; }
    }

    public enum MessageTypeEnum
    {
        普通消息 = 1
    }
}
