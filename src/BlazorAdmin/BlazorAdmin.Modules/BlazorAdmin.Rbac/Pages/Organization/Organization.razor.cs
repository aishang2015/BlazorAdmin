using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Rbac.Pages.Organization
{
    public partial class Organization
    {
        private HashSet<OrganizationItem> OrganizationItems = new();

        private OrganizationItem? SelectedOrganizationItem;

        private DotNetObjectReference<Organization> _organizationPageRef = null!;

        private bool EditVisible;

        private OrganizationModel EditModel = new();

        protected override async Task OnInitializedAsync()
        {
            _organizationPageRef = DotNetObjectReference.Create(this);
            await InitialOrganizationTree();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var sortModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/sort.js");
            await sortModule.InvokeVoidAsync("setSortable", _organizationPageRef);
        }

        private async Task InitialOrganizationTree()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var menus = context.Organizations.OrderBy(m => m.Order).ToList();
            OrganizationItems = AppendOrganizationItems(null, menus);
        }


        [JSInvokable]
        public async Task DragEnd(int id, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
            {
                return;
            }

            using var context = await _dbFactory.CreateDbContextAsync();
            var organization = context.Organizations.Find(id);
            if (organization != null)
            {
                // 同级菜单
                var allMenus = context.Organizations.Where(m => m.ParentId == organization.ParentId).ToList();

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
                organization.Order = newIndex + 1;
                await context.AuditSaveChangesAsync();
            }
        }

        private HashSet<OrganizationItem> AppendOrganizationItems(int? parentId,
                List<Data.Entities.Rbac.Organization> organizations)
        {
            return organizations.Where(m => m.ParentId == parentId).OrderBy(m => m.Order)
                .Select(m => new OrganizationItem
                {
                    Id = m.Id,
                    Name = m.Name,
                    Childs = AppendOrganizationItems(m.Id, organizations)
                }).ToHashSet();
        }

        private void AddOrganizationClick()
        {
            EditVisible = true;
            EditModel = new();
        }

        private void CancelClick()
        {
            EditVisible = false;
        }

        private async Task EditSubmit()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            if (EditModel.Id == null)
            {
                var organizationCount = context.Organizations.Count(m => m.ParentId == EditModel.ParentId);

                context.Organizations.Add(new Data.Entities.Rbac.Organization
                {
                    Name = EditModel.Name!,
                    ParentId = EditModel.ParentId,
                    Order = organizationCount + 1
                });
            }
            else
            {
                var organization = context.Organizations.Find(EditModel.Id);
                if (organization == null)
                {
                    _snackbarService.Add("此组织不存在！", Severity.Error);
                    return;
                }
                organization.Name = EditModel.Name!;
                organization.ParentId = EditModel.ParentId;
            }

            await context.AuditSaveChangesAsync();
            await InitialOrganizationTree();
            _snackbarService.Add("保存成功！", Severity.Success);
            EditVisible = false;
        }

        private async Task EditMenuClick(int organizationId)
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var organization = context.Organizations.Find(organizationId);
            if (organization != null)
            {
                EditModel = new OrganizationModel
                {
                    Id = organizationId,
                    Name = organization.Name,
                    ParentId = organization.ParentId,
                };

                EditVisible = true;
            }
        }

        private record OrganizationItem
        {
            public int Id { get; set; }

            public string Name { get; set; } = null!;

            public bool IsExpanded { get; set; }

            public HashSet<OrganizationItem> Childs { get; set; } = new();
        }

        private record OrganizationModel
        {
            public int? Id { get; set; }

            public int? ParentId { get; set; }

            [Required(ErrorMessage = "请输入组织名")]
            [MaxLength(200, ErrorMessage = "组织名过长")]
            public string? Name { get; set; }
        }
    }
}
