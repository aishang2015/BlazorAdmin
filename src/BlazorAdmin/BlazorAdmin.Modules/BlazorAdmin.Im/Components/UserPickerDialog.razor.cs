using BlazorAdmin.Core.Auth;
using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data.Entities.Chat;
using BlazorAdmin.Data.Entities.Rbac;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Linq;

namespace BlazorAdmin.Im.Components
{
    public partial class UserPickerDialog
    {
        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        private string _userName = string.Empty;

        private List<User> _allUserList = new List<User>();

        private List<User> _userlist = new List<User>();

        private HashSet<int> _checkedUserSet = new HashSet<int>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            using var context = await _dbFactory.CreateDbContextAsync();
            _allUserList = context.Users.Where(u => !u.IsDeleted && !u.IsSpecial && u.IsEnabled).AsNoTracking()
                .ToList().OrderBy(u => u.RealName).ToList();
            await InitUserList();
        }

        private async Task InitUserList()
        {
            var userId = await _stateProvider.GetUserIdAsync();
            _userlist = _allUserList
                .AndIf(!string.IsNullOrEmpty(_userName),
                u => u.RealName.Contains(_userName))
                .Where(u => u.Id != userId).ToList();
        }


        private void CheckedChanged(int userId, bool isChecked)
        {
            if (isChecked)
            {
                _checkedUserSet.Add(userId);
            }
            else
            {
                _checkedUserSet.Remove(userId);
            }
        }

        private async Task SearchedUserNameChanged(string value)
        {
            _userName = value;
            await InitUserList();
        }

        private async Task CreateChannel()
        {
            var userId = await _stateProvider.GetUserIdAsync();
            using var mainContext = await _dbFactory.CreateDbContextAsync();

            if (_checkedUserSet.Count == 1)
            {
                if (!mainContext.PrivateMessages.Any(m =>
                (m.SenderId == _checkedUserSet.First() && m.ReceiverId == userId) ||
                (m.ReceiverId == _checkedUserSet.First() && m.SenderId == userId)))
                {
                    await _messageSender.SendChannelMessage(
                        userId,
                        null,
                        _checkedUserSet.First(),
                        string.Empty, 0);
                }
            }
            else if (_checkedUserSet.Count > 1)
            {
                using var trans = mainContext.Database.BeginTransaction();
                var group = mainContext.Groups.Add(new Group
                {
                    Name = "群聊",
                });
                mainContext.SaveChanges();

                mainContext.GroupMembers.Add(new GroupMember
                {
                    GroupId = group.Entity.Id,
                    MemberId = userId
                });
                foreach (var user in _checkedUserSet)
                {
                    mainContext.GroupMembers.Add(new GroupMember
                    {
                        GroupId = group.Entity.Id,
                        MemberId = user
                    });
                }
                mainContext.SaveChanges();
                trans.Commit();
            }

            MudDialog?.Close(DialogResult.Ok(true));
        }
    }
}
