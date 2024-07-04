using BlazorAdmin.Data.Entities.Rbac;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Rbac.Pages.Role.Dialogs
{
    public partial class RoleMenuDialog
    {
        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        [Parameter] public int RoleId { get; set; }


        private List<Data.Entities.Rbac.Menu> MenuList = new();

        private List<TreeItemData<MenuTreeItem>> MenuTreeItemSet = new();

        private List<int> CheckedMenuIdList = new();

        #region initialized

        protected override async Task OnInitializedAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            CheckedMenuIdList = context.RoleMenus.Where(rm => rm.RoleId == RoleId).Select(rm => rm.MenuId).ToList();
            MenuList = context.Menus.ToList();
            MenuTreeItemSet = AppendMenuItems(null, MenuList);
        }

        private List<TreeItemData<MenuTreeItem>> AppendMenuItems(int? parentId, List<Data.Entities.Rbac.Menu> menus)
        {
            return menus.Where(m => m.ParentId == parentId).OrderBy(m => m.Order)
                .Select(m => new TreeItemData<MenuTreeItem>
                {
                    Value = new MenuTreeItem
                    {
                        Id = m.Id,
                        IsChecked = CheckedMenuIdList.Contains(m.Id),
                        MenuIcon = m.Icon,
                        MenuName = m.Name,
                    },
                    Children = AppendMenuItems(m.Id, menus)
                }).ToList();
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
            await context.SaveChangesAsync();

            _snackbarService.Add("提交成功！", Severity.Success);
            MudDialog?.Close(DialogResult.Ok(true));
        }

        private List<int> GetSelectedValues(List<TreeItemData<MenuTreeItem>> set)
        {
            return set.Where(i => i.Value.IsChecked).Select(i => i.Value.Id)
                .Concat(set.SelectMany(i => GetSelectedValues(i.Children)))
                .ToList();
        }

        #endregion


        #region checkbox checked change event

        private void CheckedChanged(bool value, TreeItemData<MenuTreeItem> item)
        {
            LoopCheckedChange(value, item);
            if (value)
            {
                LoopParentChecked(item);
            }
        }

        private void LoopCheckedChange(bool value, TreeItemData<MenuTreeItem> item)
        {
            item.Value.IsChecked = value;
            foreach (var i in item.Children)
            {
                LoopCheckedChange(value, i);
            }
        }

        private void LoopParentChecked(TreeItemData<MenuTreeItem> item)
        {
            item.Value.IsChecked = true;
            var parent = FindParentItem(item.Value, MenuTreeItemSet);
            if (parent != null)
            {
                LoopParentChecked(parent);
            }
        }

        private TreeItemData<MenuTreeItem>? FindParentItem(MenuTreeItem item, List<TreeItemData<MenuTreeItem>> itemList)
        {
            var parent = itemList.FirstOrDefault(i => i.Children.Any(c => c.Value.Id == item.Id));
            if (parent == null)
            {
                foreach (var i in itemList)
                {
                    parent = FindParentItem(item, i.Children);
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

            public bool IsChecked { get; set; }

        }
    }
}
