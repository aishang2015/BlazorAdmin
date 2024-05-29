using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Rbac
{
    [Table("RBAC_USER_ROLE")]
    [Comment("用户角色")]
    public class UserRole
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("用户id")]
        public int UserId { get; set; }

        [Comment("角色id")]
        public int RoleId { get; set; }
    }
}
