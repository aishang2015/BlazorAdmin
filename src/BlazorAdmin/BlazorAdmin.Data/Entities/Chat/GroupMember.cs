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
    [Comment("群聊成员")]
    [Table("CHAT_GROUP_MEMBER")]
    [IgnoreAudit]
    public class GroupMember
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("群聊ID")]
        public int GroupId { get; set; }

        [Comment("群聊成员ID")]
        public int MemberId { get; set; }
    }
}
