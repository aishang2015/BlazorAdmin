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
    [Comment("群聊")]
    [Table("CHAT_GROUP")]
    [IgnoreAudit]
    public class Group
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("名称")]
        public string? Name { get; set; }
    }
}
