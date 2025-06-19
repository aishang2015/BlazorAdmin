using BlazorAdmin.Servers.Core.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Servers.Core.Data.Entities.Chat
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
