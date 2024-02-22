using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Data.Entities.Setting
{
    [Comment("用户设置")]
    public class UserSetting
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("用户")]
        public int UserId { get; set; }

        [Comment("键")]
        public string Key { get; set; } = null!;

        [Comment("值")]
        public string Value { get; set; } = null!;
    }
}
