namespace BlazorAdmin.Data.Entities
{
	/// <summary>
	/// 菜单
	/// </summary>
	public class Menu
	{
		public int Id { get; set; }

		/// <summary>
		/// 上级ID
		/// <summary>
		public int? ParentId { get; set; }

		/// <summary>
		/// 菜单名称
		/// <summary>
		public string Name { get; set; } = null!;

		public int Left { get; set; }

		public int Right { get; set; }
	}
}
