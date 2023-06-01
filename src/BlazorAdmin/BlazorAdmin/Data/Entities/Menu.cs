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
		/// 菜单图标
		/// </summary>
		public string? Icon { get; set; }

		/// <summary>
		/// 菜单名称
		/// <summary>
		public string? Name { get; set; }

		/// <summary>
		/// 类型 1 菜单 2按钮
		/// </summary>
		public int Type { get; set; }

		/// <summary>
		/// 路由
		/// <summary>
		public string? Route { get; set; }

		/// <summary>
		/// 元素标识
		/// </summary>
		public string? Identify { get; set; }

		/// <summary>
		/// 排序
		/// </summary>
		public int Order { get; set; }
	}
}
