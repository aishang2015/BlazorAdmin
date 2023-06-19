using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities
{
	[Comment("用户")]
	public class User
	{
		[Comment("主键")]
		public int Id { get; set; }

		[Comment("用户角色")]
		public string? Avatar { get; set; }

		[Comment("用户角色")]
		public string Name { get; set; } = null!;

		[Comment("密码哈希")]
		public string PasswordHash { get; set; } = null!;

		[Comment("是否启用")]
		public bool IsEnabled { get; set; }
	}
}
