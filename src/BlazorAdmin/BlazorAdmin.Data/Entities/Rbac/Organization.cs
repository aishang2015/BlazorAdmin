using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities.Rbac
{
    [Comment("组织")]
    public class Organization
    {
        [Comment("主键")]
        public int Id { get; set; }

        [Comment("上级ID")]
        public int? ParentId { get; set; }

        [Comment("组织名称")]
        public string Name { get; set; } = null!;

        [Comment("排序")]
        public int Order { get; set; }
    }
}
