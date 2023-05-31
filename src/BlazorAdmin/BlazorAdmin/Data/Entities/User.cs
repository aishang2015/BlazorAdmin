namespace BlazorAdmin.Data.Entities
{
	/// <summary>
	/// 用户
	/// </summary>
	public class User
	{
		public int Id { get; set; }

		/// <summary>
		/// 头像
		/// <summary>
		public string? Avatar { get; set; }

		/// <summary>
		/// 用户名
		/// <summary>
		public string Name { get; set; } = null!;

		/// <summary>
		/// 密码哈希
		/// <summary>
		public string PasswordHash { get; set; } = null!;

		/// <summary>
		/// 是否启用
		/// <summary>
		public bool IsEnabled { get; set; }
	}
}
