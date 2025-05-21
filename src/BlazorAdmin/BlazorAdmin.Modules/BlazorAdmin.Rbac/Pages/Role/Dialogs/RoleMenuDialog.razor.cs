using BlazorAdmin.Servers.Core.Data.Entities.Rbac;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Rbac.Pages.Role.Dialogs
{
    public partial class RoleMenuDialog
    {
        [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }

        [Parameter] public int RoleId { get; set; }


        private List<Servers.Core.Data.Entities.Rbac.Menu> MenuList = new();

        private List<MenuTableRow> MenuTableRows = new();

        private List<int> CheckedMenuIdList = new();

        #region initialized

        protected override async Task OnInitializedAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            CheckedMenuIdList = context.RoleMenus.Where(rm => rm.RoleId == RoleId).Select(rm => rm.MenuId).ToList();
            MenuList = context.Menus.ToList();

            // 处理菜单层级结构
            BuildMenuLevelRows(null, 0);
        }

        //private List<TreeItemData<MenuItemViewModel>> AppendMenuItems(int? parentId, List<Servers.Core.Data.Entities.Rbac.Menu> menus)
        //{
        //    // 构建顶级菜单
        //    BuildMenuLevelRows(null, 0);
        //}

        private void BuildMenuLevelRows(int? parentId, int level)
        {
            // 获取当前层级的菜单项
            var menuItems = MenuList.Where(m => m.Type == 1 && m.ParentId == parentId)
                .OrderBy(m => m.Order)
                .ToList();

            foreach (var menu in menuItems)
            {
                // 创建当前菜单行
                var menuRow = new MenuTableRow
                {
                    Menu = new MenuItemViewModel
                    {
                        Id = menu.Id,
                        IsChecked = CheckedMenuIdList.Contains(menu.Id),
                        MenuIcon = menu.Icon,
                        MenuName = menu.Name,
                        Level = level
                    },
                    Buttons = MenuList.Where(m => m.Type == 2 && m.ParentId == menu.Id)
                        .OrderBy(m => m.Order)
                        .Select(button => new MenuItemViewModel
                        {
                            Id = button.Id,
                            IsChecked = CheckedMenuIdList.Contains(button.Id),
                            MenuName = button.Name
                        }).ToList()
                };
                
                MenuTableRows.Add(menuRow);
                
                // 递归处理子菜单
                BuildMenuLevelRows(menu.Id, level + 1);
            }
        }

        #endregion

        #region

        private async Task SubmitRoleMenu()
        {
            using var context = await _dbFactory.CreateDbContextAsync();

            // 获取所有选中的菜单和按钮ID
            var idList = GetSelectedValues();

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
            await context.SaveChangesAsync();

            _snackbarService.Add("提交成功！", Severity.Success);
            MudDialog?.Close(DialogResult.Ok(true));
        }

        private List<int> GetSelectedValues()
        {
            var result = new List<int>();
            
            // 添加选中的菜单ID
            result.AddRange(MenuTableRows.Where(row => row.Menu.IsChecked).Select(row => row.Menu.Id));
            
            // 添加选中的按钮ID
            foreach (var row in MenuTableRows)
            {
                result.AddRange(row.Buttons.Where(btn => btn.IsChecked).Select(btn => btn.Id));
            }
            
            return result;
        }

        #endregion

        #region checkbox checked change event

        private void MenuCheckedChanged(bool value, MenuItemViewModel item)
        {
            item.IsChecked = value;
            
            // 如果取消选中菜单，同时取消选中其所有按钮和子菜单
            if (!value)
            {
                // 取消所有按钮
                var row = MenuTableRows.FirstOrDefault(r => r.Menu.Id == item.Id);
                if (row != null)
                {
                    foreach (var button in row.Buttons)
                    {
                        button.IsChecked = false;
                    }
                }
                
                // 递归取消所有子菜单
                UncheckChildMenus(item.Id);
            }
            else
            {
                // 如果选中菜单，确保其所有父菜单也被选中
                CheckParentMenus(item.Id);
            }
        }

        private void UncheckChildMenus(int parentId)
        {
            // 查找所有子菜单行
            var childRows = MenuTableRows.Where(r => MenuList.Any(m => m.Id == r.Menu.Id && m.ParentId == parentId)).ToList();
            
            foreach (var childRow in childRows)
            {
                childRow.Menu.IsChecked = false;
                
                // 取消所有按钮
                foreach (var button in childRow.Buttons)
                {
                    button.IsChecked = false;
                }
                
                // 递归处理子菜单的子菜单
                UncheckChildMenus(childRow.Menu.Id);
            }
        }

        private void CheckParentMenus(int menuId)
        {
            // 找到当前菜单
            var menu = MenuList.FirstOrDefault(m => m.Id == menuId);
            if (menu?.ParentId != null)
            {
                // 找到父菜单行
                var parentRow = MenuTableRows.FirstOrDefault(r => r.Menu.Id == menu.ParentId);
                if (parentRow != null && !parentRow.Menu.IsChecked)
                {
                    parentRow.Menu.IsChecked = true;
                    
                    // 递归处理父菜单的父菜单
                    CheckParentMenus(parentRow.Menu.Id);
                }
            }
        }

        private void ButtonCheckedChanged(bool value, MenuItemViewModel item)
        {
            item.IsChecked = value;
            
            // 如果选中按钮，确保其所属菜单也被选中
            if (value)
            {
                var row = MenuTableRows.FirstOrDefault(r => r.Buttons.Any(b => b.Id == item.Id));
                if (row != null)
                {
                    row.Menu.IsChecked = true;
                    
                    // 确保所有父菜单也被选中
                    CheckParentMenus(row.Menu.Id);
                }
            }
        }

        #endregion

        #region Models

        private class MenuItemViewModel
        {
            public int Id { get; set; }

            public string? MenuIcon { get; set; }

            public string? MenuName { get; set; }

            public bool IsChecked { get; set; }
            
            public int Level { get; set; } = 0;
        }

        private class MenuTableRow
        {
            public MenuItemViewModel Menu { get; set; } = new();
            
            public List<MenuItemViewModel> Buttons { get; set; } = new();
        }

        #endregion
    }
}
