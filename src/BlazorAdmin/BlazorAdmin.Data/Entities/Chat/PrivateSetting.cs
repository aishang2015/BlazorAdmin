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
    [Comment("单聊设置")]
    [Table("CHAT_PRIVATE_SETTING")]
    [IgnoreAudit]
    public class PrivateSetting
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("所属用户")]
        public int UserId { get; set; }

        [Comment("对话用户")]
        public int OtherUserId { get; set; }

        [Comment("显示顺序")]
        public int Order { get; set; }
    }
}
