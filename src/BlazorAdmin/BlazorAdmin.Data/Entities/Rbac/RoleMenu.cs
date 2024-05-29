using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Rbac
{
    [Table("RBAC_ROLE_MENU")]
    [Comment("角色的菜单")]
    public class RoleMenu
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("角色id")]
        public int RoleId { get; set; }

        [Comment("菜单id")]
        public int MenuId { get; set; }
    }
}
