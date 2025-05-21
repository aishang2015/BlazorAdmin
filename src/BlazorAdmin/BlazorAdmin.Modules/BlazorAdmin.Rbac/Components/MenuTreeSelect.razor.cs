﻿using BlazorAdmin.Servers.Core.Data.Entities.Rbac;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Rbac.Components
{
    public partial class MenuTreeSelect
    {
        [Parameter] public int? SelectedValue { get; set; }

        [Parameter] public EventCallback<int?> SelectedValueChanged { get; set; }

        private List<Menu> MenuList = new();

        private List<TreeItemData<MenuItem>> MenuItems = new();

        private MenuItem? SelectedItem;

        private string? SelectedMenuName = null;

        private bool _popoverOpen;

        private bool _isMouseOnPopover;

        protected override async Task OnInitializedAsync()
        {
            await InitialMenuTree();
        }

        protected override void OnParametersSet()
        {
            if (SelectedValue is not null)
            {
                var menu = MenuList.FirstOrDefault(m => m.Id == SelectedValue);
                SelectedMenuName = menu?.Name;
            }
        }

        private async Task InitialMenuTree()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            MenuList = context.Menus.Where(m => m.Type == 1).ToList();
            MenuItems = AppendMenuItems(null, MenuList);
        }

        private List<TreeItemData<MenuItem>> AppendMenuItems(int? parentId, List<Menu> menus)
        {
            return menus.Where(m => m.ParentId == parentId).OrderBy(m => m.Order)
                .Select(m => new TreeItemData<MenuItem>
                {
                    Value = new MenuItem
                    {
                        Id = m.Id,
                        MenuName = m.Name,
                        Route = m.Route,
                        MenuType = m.Type,
                    },
                    Children = AppendMenuItems(m.Id, menus)
                }).ToList();
        }

        private async Task SelectedTreeItemChanged(MenuItem item)
        {
            if (item != null)
            {
                SelectedItem = item;

                _popoverOpen = false;
                SelectedMenuName = item.MenuName;
                SelectedValue = item.Id;
                await SelectedValueChanged.InvokeAsync(item.Id);
            }
        }

        private async Task CleanSelected()
        {
            SelectedItem = null;

            _popoverOpen = false;
            SelectedMenuName = null;
            SelectedValue = null;
            await SelectedValueChanged.InvokeAsync(null);
        }

        private void ElementFocused()
        {
            _popoverOpen = true;
        }

        private void MouseBlur()
        {
            if (!_isMouseOnPopover)
            {
                _popoverOpen = false;
            }
        }

        private void MouseEnterPopover()
        {
            _isMouseOnPopover = true;
        }

        private void MouseLeavePopover()
        {
            _isMouseOnPopover = false;
        }

        private record MenuItem
        {
            public int Id { get; set; }

            public string? MenuName { get; set; }

            public string? Route { get; set; }

            // 1 菜单 2 按钮
            public int MenuType { get; set; }
        }
    }
}
