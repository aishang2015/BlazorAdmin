using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities
{
	[Comment("角色")]
	public class Role
	{
		[Comment("主键")]
		public int Id { get; set; }

		[Comment("角色名")]
		public string? Name { get; set; }

		[Comment("是否启用")]
		public bool IsEnabled { get; set; }
	}
}
