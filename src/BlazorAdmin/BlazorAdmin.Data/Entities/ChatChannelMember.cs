using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Data.Entities
{
    [Comment("聊天成员")]
    public class ChatChannelMember
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("聊天频道Id")]
        public int ChatChannelId { get; set; }

        [Comment("成员Id")]
        public int MemberId { get; set; }
    }
}
