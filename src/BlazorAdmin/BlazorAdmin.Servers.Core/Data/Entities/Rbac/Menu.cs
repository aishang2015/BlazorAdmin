using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Rbac
{
    [Table("RBAC_MENU")]
    [Comment("菜单")]
    public class Menu
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("上级ID")]
        public int? ParentId { get; set; }

        [Comment("菜单图标")]
        public string? Icon { get; set; }

        [Comment("菜单名称")]
        public string? Name { get; set; }

        [Comment("类型 1 菜单 2按钮")]
        public int Type { get; set; }

        [Comment("路由")]
        public string? Route { get; set; }

        [Comment("元素标识")]
        public string? Identify { get; set; }

        [Comment("排序")]
        public int Order { get; set; }
    }
}
