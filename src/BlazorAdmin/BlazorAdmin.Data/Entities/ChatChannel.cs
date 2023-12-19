using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Data.Entities
{
    [Comment("聊天频道")]
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
