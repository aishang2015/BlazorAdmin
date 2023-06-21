using BlazorAdmin.Core.Dynamic;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities
{
	[Comment("用户")]
	[DynamicEntity(Title = "用户")]
	public class User
	{
		[Comment("主键")]
		[DynamicProperty(IsDisplay = false)]
		public int Id { get; set; }

		[Comment("用户头像")]
		[DynamicProperty(Title = "用户头像", Order = 1)]
		public string? Avatar { get; set; }

		[Comment("用户名")]
		[DynamicProperty(Title = "用户名", Order = 3)]
		public string Name { get; set; } = null!;

		[Comment("密码哈希")]
		[DynamicProperty(IsDisplay = false)]
		public string PasswordHash { get; set; } = null!;

		[Comment("是否启用")]
		[DynamicProperty(Title = "是否启用", Order = 2)]
		public bool IsEnabled { get; set; }
	}
}
