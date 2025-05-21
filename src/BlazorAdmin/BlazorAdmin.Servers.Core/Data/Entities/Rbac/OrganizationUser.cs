using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAdmin.Data.Entities.Rbac
{
    [Table("RBAC_ORGANIZATION_USER")]
    [Comment("组织用户")]
    public class OrganizationUser
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("组织Id")]
        public int OrganizationId { get; set; }

        [Comment("用户Id")]
        public int UserId { get; set; }

        [Comment("是否是负责人")]
        public bool IsLeader { get; set; }
    }
}
