namespace BlazorAdmin.Data.Entities
{
	/// <summary>
	/// 角色的菜单
	/// </summary>
	public class RoleMenu
	{
		public int Id { get; set; }

		/// <summary>
		/// 角色id
		/// <summary>
		public int RoleId { get; set; }

		/// <summary>
		/// 菜单id
		/// </summary>
		public int MenuId { get; set; }
	}
}
