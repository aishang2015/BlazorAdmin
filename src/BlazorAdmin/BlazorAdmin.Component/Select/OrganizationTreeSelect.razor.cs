using BlazorAdmin.Data.Entities.Rbac;
using Microsoft.AspNetCore.Components;

namespace BlazorAdmin.Component.Select
{
    public partial class OrganizationTreeSelect
    {
        [Parameter] public int? SelectedValue { get; set; }

        [Parameter] public EventCallback<int?> SelectedValueChanged { get; set; }

        private List<Organization> OrganizationList = new();

        private HashSet<OrganizationItem> OrganizationItems = new();

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

        private HashSet<OrganizationItem> AppendOrganizationItems(int? parentId,
            List<Organization> organizations)
        {
            return organizations.Where(m => m.ParentId == parentId).OrderBy(m => m.Order)
                .Select(m => new OrganizationItem
                {
                    Id = m.Id,
                    Name = m.Name,
                    Childs = AppendOrganizationItems(m.Id, organizations)
                }).ToHashSet();
        }

        private async Task SelectedTreeItemChanged(OrganizationItem item)
        {
            if (item != null)
            {
                _popoverOpen = false;
                SelectedMenuName = item.Name;
                SelectedValue = item.Id;
                await SelectedValueChanged.InvokeAsync(item.Id);
            }
        }

        private async Task CleanSelected()
        {
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

        private record OrganizationItem
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            public bool IsExpanded { get; set; }

            public HashSet<OrganizationItem> Childs { get; set; } = new();
        }
    }
}
