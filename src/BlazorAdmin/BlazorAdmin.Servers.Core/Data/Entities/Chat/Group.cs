﻿using BlazorAdmin.Servers.Core.Data.Attributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Servers.Core.Data.Entities.Chat
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
