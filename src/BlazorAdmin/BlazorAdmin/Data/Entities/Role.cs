namespace BlazorAdmin.Data.Entities
{
	/// <summary>
	/// 角色
	/// </summary>
	public class Role
	{
		public int Id { get; set; }

		/// <summary>
		/// 角色名
		/// <summary>
		public string? Name { get; set; }

		/// <summary>
		/// 是否启用
		/// <summary>
		public bool IsEnabled { get; set; }
	}
}
