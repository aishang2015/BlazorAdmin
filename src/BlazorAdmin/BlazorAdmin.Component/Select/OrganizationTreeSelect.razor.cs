using BlazorAdmin.Data.Entities.Rbac;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace BlazorAdmin.Component.Select
{
    public partial class OrganizationTreeSelect
    {
        [Parameter] public int? SelectedValue { get; set; }

        [Parameter] public EventCallback<int?> SelectedValueChanged { get; set; }

        private List<Organization> OrganizationList = new();

        private List<TreeItemData<OrganizationItem>> OrganizationItems = new();

        private OrganizationItem? SelectedTreeItem = new();

        private string? SelectedMenuName = null;

        private bool _popoverOpen;

        private bool _isMouseOnPopover;

        protected override async Task OnInitializedAsync()
        {
            await InitialOrganizationTree();
        }

        protected override void OnParametersSet()
        {
            if (SelectedValue is not null)
            {
                var organization = OrganizationList.FirstOrDefault(m => m.Id == SelectedValue);
                SelectedMenuName = organization?.Name;
            }
        }

        private async Task InitialOrganizationTree()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            OrganizationList = context.Organizations.OrderBy(o => o.Order).ToList();
            OrganizationItems = AppendOrganizationItems(null, OrganizationList);
        }

        private List<TreeItemData<OrganizationItem>> AppendOrganizationItems(int? parentId,
            List<Organization> organizations)
        {
            return organizations.Where(m => m.ParentId == parentId).OrderBy(m => m.Order)
                .Select(m => new TreeItemData<OrganizationItem>
                {
                    Value = new OrganizationItem
                    {
                        Id = m.Id,
                        Name = m.Name,
                    },
                    Children = AppendOrganizationItems(m.Id, organizations)
                }).ToList();
        }

        private async Task SelectedTreeItemChanged(OrganizationItem item)
        {
            if (item != null)
            {
                _popoverOpen = false;
                SelectedTreeItem = item;
                SelectedMenuName = item.Name;
                await SelectedValueChanged.InvokeAsync(item.Id);
            }
        }

        private async Task CleanSelected()
        {
            _popoverOpen = false;
            SelectedTreeItem = null;
            SelectedMenuName = null;
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

        private record OrganizationItem
        {
            public int Id { get; set; }

            public string? Name { get; set; }
        }
    }
}
