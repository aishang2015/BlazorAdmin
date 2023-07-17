using BlazorAdmin.Pages.Dialogs.Rbac.Menu;
using Microsoft.JSInterop;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Pages.Rbac
{
	public partial class Menu
	{
		private Dictionary<string, object> InputAttributes { get; set; } =
			new Dictionary<string, object>()
				{
				   { "autocomplete", "off2" },
				};

		private HashSet<MenuItem> MenuItems = new();

		private MenuItem? SelectedMenuItem;

		private bool EditVisible;

		private MenuModel MenuEditModel = new();

		// 当前页面的引用，用于js回调
		private DotNetObjectReference<Menu> _menuPageRef = null!;

		private bool ReRenderFlg = true;

		protected override async Task OnInitializedAsync()
		{
			_menuPageRef = DotNetObjectReference.Create(this);
			await InitialMenuTree();
		}

		private async Task InitialMenuTree()
		{

			using var context = await _dbFactory.CreateDbContextAsync();
			var menus = context.Menus.OrderBy(m => m.Order).ToList();
			MenuItems = AppendMenuItems(null, menus);

			ReRenderFlg = false;
			await Task.Yield();
			ReRenderFlg = true;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			var authModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/sort.js");
			await authModule.InvokeVoidAsync("setSortable", _menuPageRef);
		}

		private async Task EditSubmit()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			if (MenuEditModel.Id == null)
			{
				var menusCount = context.Menus.Count(m => m.ParentId == MenuEditModel.ParentId);

				context.Menus.Add(new Data.Entities.Menu
				{
					Icon = MenuEditModel.Type == 1 ? MenuEditModel.Icon : null,
					Name = MenuEditModel.Name,
					Identify = MenuEditModel.Identify,
					ParentId = MenuEditModel.ParentId,
					Route = MenuEditModel.Route,
					Type = MenuEditModel.Type,
					Order = menusCount + 1
				});
			}
			else
			{
				var menu = context.Menus.Find(MenuEditModel.Id);
				if (menu == null)
				{
					_snackbarService.Add("此菜单不存在！", Severity.Error);
					return;
				}
				menu.Icon = MenuEditModel.Type == 1 ? MenuEditModel.Icon : null;
				menu.Name = MenuEditModel.Name;
				menu.Identify = MenuEditModel.Identify;
				menu.ParentId = MenuEditModel.ParentId;
				menu.Route = MenuEditModel.Route;
				menu.Type = MenuEditModel.Type;
			}

			await context.CustomSaveChangesAsync(_stateProvider);
			await InitialMenuTree();
			_snackbarService.Add("保存成功！", Severity.Success);
			EditVisible = false;
		}

		private HashSet<MenuItem> AppendMenuItems(int? parentId, List<Data.Entities.Menu> menus)
		{
			return menus.Where(m => m.ParentId == parentId).OrderBy(m => m.Order)
				.Select(m => new MenuItem
				{
					Id = m.Id,
					Icon = m.Icon,
					MenuName = m.Name,
					Route = m.Route,
					MenuType = m.Type,
					Childs = AppendMenuItems(m.Id, menus)
				}).ToHashSet();
		}

		private void AddMenuClick()
		{
			EditVisible = true;
			MenuEditModel = new()
			{
				Icon = Icons.Material.Filled.Home
			};
		}

		private async Task EditMenuClick(int menuId)
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			var menu = context.Menus.Find(menuId);
			if (menu != null)
			{
				MenuEditModel = new MenuModel
				{
					Id = menuId,
					Identify = menu.Identify,
					Icon = menu.Icon,
					Name = menu.Name,
					ParentId = menu.ParentId,
					Route = menu.Route,
					Type = menu.Type
				};

				EditVisible = true;
			}
		}

		private async Task DeleteMenuClick()
		{
			if (SelectedMenuItem == null)
			{
				return;
			}

			using var context = await _dbFactory.CreateDbContextAsync();
			var menus = context.Menus.ToList();
			var subtreeIdList = FindAllSubTreeIds(SelectedMenuItem.Id, menus);
			subtreeIdList.Add(SelectedMenuItem.Id);

			var parameters = new DialogParameters
			{
				{"MenuIdList",subtreeIdList}
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<DeleteMenuDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialMenuTree();
				SelectedMenuItem = null;
			}
		}

		private void CancelClick()
		{
			EditVisible = false;
		}

		private List<int> FindAllSubTreeIds(int parentId, List<Data.Entities.Menu> menus)
		{
			return menus.Where(m => m.ParentId == parentId).Select(m => m.Id).ToList()
				.Concat(menus.Where(m => m.ParentId == parentId).SelectMany(m => FindAllSubTreeIds(m.Id, menus)))
				.ToList();
		}

		[JSInvokable]
		public async Task DragEnd(int id, int oldIndex, int newIndex)
		{
			if (oldIndex == newIndex)
			{
				return;
			}

			using var context = await _dbFactory.CreateDbContextAsync();
			var menu = context.Menus.Find(id);
			if (menu != null)
			{
				// 同级菜单
				var allMenus = context.Menus.Where(m => m.ParentId == menu.ParentId).ToList();

				if (oldIndex > newIndex)
				{
					foreach (var m in allMenus.Where(m => m.Order >= newIndex + 1 && m.Order < oldIndex + 1))
					{
						m.Order++;
					}
				}
				else
				{
					foreach (var m in allMenus.Where(m => m.Order <= newIndex + 1 && m.Order > oldIndex + 1))
					{
						m.Order--;
					}
				}
				menu.Order = newIndex + 1;
				await context.CustomSaveChangesAsync(_stateProvider);
			}
		}

		private record MenuItem
		{
			public int Id { get; set; }

			public string? Icon { get; set; }

			public string? MenuName { get; set; }

			public string? Route { get; set; }

			public bool IsExpanded { get; set; }

			// 1 菜单 2 按钮
			public int MenuType { get; set; }

			public HashSet<MenuItem> Childs { get; set; } = new();
		}

		private record MenuModel
		{
			public int? Id { get; set; }

			public int? ParentId { get; set; }

			public string? Icon { get; set; }

			[Required(ErrorMessage = "请输入菜单名")]
			[MaxLength(200, ErrorMessage = "菜单名过长")]
			public string? Name { get; set; }

			/// <summary>
			/// 类型 1 菜单 2按钮
			/// </summary>
			public int Type { get; set; } = 1;


			[MaxLength(1000, ErrorMessage = "路由过长")]
			public string? Route { get; set; }

			/// <summary>
			/// 元素标识
			/// </summary>
			[MaxLength(200, ErrorMessage = "元素标识过长")]
			public string? Identify { get; set; }
		}
	}
}
