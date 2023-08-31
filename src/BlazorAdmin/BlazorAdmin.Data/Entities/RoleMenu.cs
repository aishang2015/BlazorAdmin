using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Entities
{
	[Comment("角色的菜单")]
	public class RoleMenu
	{
		[Comment("主键")]
		public int Id { get; set; }

		[Comment("角色id")]
		public int RoleId { get; set; }

		[Comment("菜单id")]
		public int MenuId { get; set; }
	}
}
