﻿using BlazorAdmin.Rbac.Pages.Organization.Dialogs;
using BlazorAdmin.Servers.Core.Components.Dialogs;
using BlazorAdmin.Servers.Core.Data;
using BlazorAdmin.Servers.Core.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Rbac.Pages.Organization
{
    public partial class Organization
    {
        private List<TreeItemData<OrganizationItem>> OrganizationItems = new();

        private OrganizationItem? SelectedOrganizationItem;

        private DotNetObjectReference<Organization> _organizationPageRef = null!;

        private bool EditVisible;

        private OrganizationModel EditModel = new();

        private bool MemberVisible;

        private List<Member> OrganizationMembers = new();

        protected override async Task OnInitializedAsync()
        {
            _organizationPageRef = DotNetObjectReference.Create(this);
            await InitialOrganizationTree();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var sortModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorAdmin.Rbac/js/sort.js");
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
                await context.SaveChangesAuditAsync();
            }
        }

        private List<TreeItemData<OrganizationItem>> AppendOrganizationItems(int? parentId,
                List<Servers.Core.Data.Entities.Rbac.Organization> organizations)
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

        private void AddOrganizationClick()
        {
            EditVisible = true;
            MemberVisible = false;
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

                context.Organizations.Add(new Servers.Core.Data.Entities.Rbac.Organization
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

                if (organization.Id == EditModel.ParentId)
                {
                    _snackbarService.Add("不能将自身设置为上级组织！", Severity.Error);
                    return;
                }

                organization.Name = EditModel.Name!;
                organization.ParentId = EditModel.ParentId;
            }

            await context.SaveChangesAuditAsync();
            await InitialOrganizationTree();
            _snackbarService.Add("保存成功！", Severity.Success);
            EditVisible = false;
        }

        private async Task OrganizationSelected(OrganizationItem item)
        {
            if (!_accessService.CheckHasElementRights("OrganizationUpdateBtn").Result)
            {
                return;
            }

            SelectedOrganizationItem = item;

            using var context = await _dbFactory.CreateDbContextAsync();
            var organization = context.Organizations.Find(item.Id);
            if (organization != null)
            {
                EditModel = new OrganizationModel
                {
                    Id = item.Id,
                    Name = organization.Name,
                    ParentId = organization.ParentId,
                };

                EditVisible = true;
                MemberVisible = false;
            }
        }


        private async Task DeleteOrganizationClick()
        {
            if (SelectedOrganizationItem == null)
            {
                return;
            }

            await _dialogService.ShowDeleteDialog(null, null,
                async (CommonDialogEventArgs e) =>
                {
                    using var db = await _dbFactory.CreateDbContextAsync();

                    var organizationIds = db.Organizations.GetAllSubOrganiations(SelectedOrganizationItem.Id)
                        .Select(o => o.Id);

                    using var tran = db.Database.BeginTransaction();

                    await db.Organizations.Where(m => organizationIds.Contains(m.Id)).ExecuteDeleteAsync();
                    await db.OrganizationUsers.Where(ou => organizationIds.Contains(ou.OrganizationId)).ExecuteDeleteAsync();
                    await tran.CommitAsync();

                    _snackbarService.Add("删除成功！", Severity.Success);
                    await InitialOrganizationTree();

                    if (EditModel.Id == SelectedOrganizationItem?.Id)
                    {
                        EditModel = new();
                        MemberVisible = false;
                        EditVisible = false;
                    }
                    SelectedOrganizationItem = null;

                    StateHasChanged();
                });
        }

        #region Member edit 

        private async Task EditMemberClick(int organizationId)
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

                IntialOrganizationMembers(context);

                EditVisible = false;
                MemberVisible = true;
            }
        }

        private void IntialOrganizationMembers(BlazorAdminDbContext context)
        {
            OrganizationMembers = (from user in context.Users
                                   join ou in context.OrganizationUsers on user.Id equals ou.UserId
                                   where ou.OrganizationId == EditModel.Id
                                   select new Member
                                   {
                                       MemberId = user.Id,
                                       MemberAvatar = user.Avatar,
                                       MemberName = user.RealName,
                                       IsLeader = ou.IsLeader
                                   })
                                   .OrderByDescending(m => m.IsLeader)
                                   .ToList();
        }

        private async Task RemoveMember(int userId)
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            await context.OrganizationUsers.Where(ou =>
                ou.OrganizationId == EditModel.Id && ou.UserId == userId)
                .ExecuteDeleteAsync();
            IntialOrganizationMembers(context);
        }

        private async Task AddNewMember()
        {
            var parameters = new DialogParameters
            {
                { "OrganizationId", EditModel.Id }
            };

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            var dialog = await _dialogService.ShowAsync<MemberSelect>("添加新成员", parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                IntialOrganizationMembers(context);
            }
        }

        private async Task MakeLeader(int userId)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var ou = db.OrganizationUsers.FirstOrDefault(ou => ou.OrganizationId == EditModel.Id &&
                ou.UserId == userId);
            if (ou != null)
            {
                ou.IsLeader = true;
                db.OrganizationUsers.Update(ou);
                await db.SaveChangesAuditAsync();
                IntialOrganizationMembers(db);
                _snackbarService.Add("设置成功！", Severity.Success);
            }
        }

        private async Task MakeNormal(int userId)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var ou = db.OrganizationUsers.FirstOrDefault(ou => ou.OrganizationId == EditModel.Id &&
                ou.UserId == userId);
            if (ou != null)
            {
                ou.IsLeader = false;
                db.OrganizationUsers.Update(ou);
                await db.SaveChangesAuditAsync();
                IntialOrganizationMembers(db);
                _snackbarService.Add("设置成功！", Severity.Success);
            }
        }

        #endregion

        private record OrganizationItem
        {
            public int Id { get; set; }

            public string Name { get; set; } = null!;
        }

        private record OrganizationModel
        {
            public int? Id { get; set; }

            public int? ParentId { get; set; }

            [Required(ErrorMessage = "请输入组织名")]
            [MaxLength(200, ErrorMessage = "组织名过长")]
            public string? Name { get; set; }
        }


        private record Member
        {
            public int Number { get; set; }

            public int MemberId { get; set; }

            public string MemberName { get; set; } = null!;

            public string? MemberAvatar { get; set; }

            public bool IsLeader { get; set; }
        }
    }
}
