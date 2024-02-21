using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BlazorAdmin.Rbac.Pages.Organization.Dialogs
{
    public partial class MemberSelect
    {
        [CascadingParameter]
        MudDialogInstance? MudDialog { get; set; }

        [Parameter]
        public int OrganizationId { get; set; }

        private Task<List<UserModel>>? _runningTask;

        private string? _searchText;

        private List<UserModel> _users = new();

        private bool _isPopoverOpen = false;

        private async Task SearchTextChanged(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (_runningTask == null || _runningTask.IsCompleted)
                {
                    using var dbContext = await _dbFactory.CreateDbContextAsync();

                    var query = from user in dbContext.Users
                                join ou in dbContext.OrganizationUsers on user.Id equals ou.UserId into ouGroup
                                from ou in ouGroup.DefaultIfEmpty()
                                where user.RealName.Contains(str)
                                select new UserModel
                                {
                                    UserId = user.Id,
                                    IsInOrg = ou != null,
                                    Avatar = user.Avatar,
                                    RealName = user.RealName
                                };
                    _runningTask = query.ToListAsync();
                    _users = await _runningTask;

                    if (_users.Count > 0)
                    {
                        _isPopoverOpen = true;
                    }
                }
            }
            else
            {
                _users.Clear();
            }
        }

        private async Task SetMemberUser(UserModel user)
        {
            if (!user.IsInOrg)
            {
                using var dbContext = await _dbFactory.CreateDbContextAsync();
                var entry = dbContext.OrganizationUsers.Add(new Data.Entities.Rbac.OrganizationUser
                {
                    UserId = user.UserId,
                    OrganizationId = OrganizationId
                });
                await dbContext.SaveChangesAsync();
                MudDialog?.Close(DialogResult.Ok(true));
            }
        }


        private class UserModel
        {
            public int UserId { get; set; }
            public bool IsInOrg { get; set; }
            public string? Avatar { get; set; }
            public string RealName { get; set; } = null!;
        }
    }
}
