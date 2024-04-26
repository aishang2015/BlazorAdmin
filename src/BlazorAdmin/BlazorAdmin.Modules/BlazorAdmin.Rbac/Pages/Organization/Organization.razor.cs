using BlazorAdmin.Component.Dialogs;
using BlazorAdmin.Data;
using BlazorAdmin.Rbac.Pages.Organization.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Rbac.Pages.Organization
{
    public partial class Organization
    {
        private HashSet<OrganizationItem> OrganizationItems = new();

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
                await context.SaveChangesAsync();
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

            await context.SaveChangesAsync();
            await InitialOrganizationTree();
            _snackbarService.Add("保存成功！", Severity.Success);
            EditVisible = false;
        }

        private async Task EditOrganizationClick(int organizationId)
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

                    var organizations = db.Organizations.ToList();
                    var subtreeIdList = FindAllSubTreeIds(SelectedOrganizationItem.Id, organizations);
                    subtreeIdList.Add(SelectedOrganizationItem.Id);

                    using var tran = db.Database.BeginTransaction();

                    await db.Organizations.Where(m => subtreeIdList.Contains(m.Id)).ExecuteDeleteAsync();
                    await db.OrganizationUsers.Where(ou => subtreeIdList.Contains(ou.OrganizationId)).ExecuteDeleteAsync();
                    await tran.CommitAsync();

                    _snackbarService.Add("删除成功！", Severity.Success);
                    await InitialOrganizationTree();
                    SelectedOrganizationItem = null;
                    StateHasChanged();
                });
        }

        private List<int> FindAllSubTreeIds(int parentId, List<Data.Entities.Rbac.Organization> organizations)
        {
            return organizations.Where(m => m.ParentId == parentId).Select(m => m.Id).ToList()
                .Concat(organizations.Where(m => m.ParentId == parentId).SelectMany(m => FindAllSubTreeIds(m.Id, organizations)))
                .ToList();
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
            var result = await _dialogService.Show<MemberSelect>
                ("添加新成员", parameters, options).Result;
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
                await db.SaveChangesAsync();
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
                await db.SaveChangesAsync();
                IntialOrganizationMembers(db);
                _snackbarService.Add("设置成功！", Severity.Success);
            }
        }

        #endregion

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
