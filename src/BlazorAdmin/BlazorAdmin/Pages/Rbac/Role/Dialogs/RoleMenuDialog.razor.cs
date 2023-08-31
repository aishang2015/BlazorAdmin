using BlazorAdmin.Data.Entities;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Pages.Rbac.Role.Dialogs
{
	public partial class RoleMenuDialog
	{
		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }

		[Parameter] public int RoleId { get; set; }


		private List<Data.Entities.Menu> MenuList = new();

		private HashSet<MenuTreeItem> MenuTreeItemSet = new();

		private List<int> CheckedMenuIdList = new();

		#region initialized

		protected override async Task OnInitializedAsync()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			CheckedMenuIdList = context.RoleMenus.Where(rm => rm.RoleId == RoleId).Select(rm => rm.MenuId).ToList();
			MenuList = context.Menus.ToList();
			MenuTreeItemSet = AppendMenuItems(null, MenuList);
		}

		private HashSet<MenuTreeItem> AppendMenuItems(int? parentId, List<Data.Entities.Menu> menus)
		{
			return menus.Where(m => m.ParentId == parentId).OrderBy(m => m.Order)
				.Select(m => new MenuTreeItem
				{
					Id = m.Id,
					IsChecked = CheckedMenuIdList.Contains(m.Id),
					MenuIcon = m.Icon,
					MenuName = m.Name,
					Childs = AppendMenuItems(m.Id, menus)
				}).ToHashSet();
		}

		#endregion

		#region

		private async Task SubmitRoleMenu()
		{
			using var context = await _dbFactory.CreateDbContextAsync();

			var idList = GetSelectedValues(MenuTreeItemSet);

			var roleMenuList = context.RoleMenus.Where(rm => rm.RoleId == RoleId).ToList();

			var deletedMenuList = roleMenuList.Where(rm => !idList.Contains(rm.MenuId));
			context.RoleMenus.RemoveRange(deletedMenuList);

			var addMenuIds = idList.Where(id => roleMenuList.All(rm => rm.MenuId != id))
				.Select(id => new RoleMenu
				{
					RoleId = RoleId,
					MenuId = id
				});
			context.RoleMenus.AddRange(addMenuIds);
			await context.CustomSaveChangesAsync(_stateProvider);

			_snackbarService.Add("提交成功！", Severity.Success);
			MudDialog?.Close(DialogResult.Ok(true));
		}

		private List<int> GetSelectedValues(HashSet<MenuTreeItem> set)
		{
			return set.Where(i => i.IsChecked).Select(i => i.Id)
				.Concat(set.SelectMany(i => GetSelectedValues(i.Childs)))
				.ToList();
		}

		#endregion


		#region checkbox checked change event

		private void CheckedChanged(bool value, MenuTreeItem item)
		{
			LoopCheckedChange(value, item);
			if (value)
			{
				LoopParentChecked(item);
			}
		}

		private void LoopCheckedChange(bool value, MenuTreeItem item)
		{
			item.IsChecked = value;
			foreach (var i in item.Childs)
			{
				LoopCheckedChange(value, i);
			}
		}

		private void LoopParentChecked(MenuTreeItem item)
		{
			item.IsChecked = true;
			var parent = FindParentItem(item, MenuTreeItemSet);
			if (parent != null)
			{
				LoopParentChecked(parent);
			}
		}

		private MenuTreeItem? FindParentItem(MenuTreeItem item, HashSet<MenuTreeItem> itemList)
		{
			var parent = itemList.FirstOrDefault(i => i.Childs.Contains(item));
			if (parent == null)
			{
				foreach (var i in itemList)
				{
					parent = FindParentItem(item, i.Childs);
					if (parent != null)
					{
						return parent;
					}
				}
			}
			return parent;
		}

		#endregion

		private class MenuTreeItem
		{
			public int Id { get; set; }

			public string? MenuIcon { get; set; }

			public string? MenuName { get; set; }

			public bool IsExpanded { get; set; }

			public bool IsChecked { get; set; }

			public HashSet<MenuTreeItem> Childs { get; set; } = new();

		}
	}
}
