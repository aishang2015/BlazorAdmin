using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data.Entities.Chat;
using BlazorAdmin.Data.Entities.Rbac;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

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
            var state = await _stateProvider.GetAuthenticationStateAsync();
            _userlist = _allUserList
                .AndIf(!string.IsNullOrEmpty(_userName),
                u => u.RealName.Contains(_userName))
                .Where(u => u.Id != state.User.GetUserId()).ToList();
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
            var state = await _stateProvider.GetAuthenticationStateAsync();
            using var mainContext = await _dbFactory.CreateDbContextAsync();
            using var trans = mainContext.Database.BeginTransaction();

            ChatChannelType type;
            if (_checkedUserSet.Count == 1)
            {
                var systemChannel = (from channel in mainContext.ChatChannels
                                     join cm1 in mainContext.ChatChannelMembers on channel.Id equals cm1.ChatChannelId
                                     join cm2 in mainContext.ChatChannelMembers on channel.Id equals cm2.ChatChannelId
                                     where cm1.MemberId == _checkedUserSet.First() &&
                                         cm2.MemberId == state.User.GetUserId()
                                     select channel).FirstOrDefault();
                if (systemChannel != null)
                {
                    return;
                }
                type = ChatChannelType.普通对话;
            }
            else
            {
                type = ChatChannelType.普通群聊;
            }

            // 创建频道
            var entry = mainContext.ChatChannels.Add(new ChatChannel { Name = "群聊", Type = (int)type });
            await mainContext.SaveChangesAsync();

            // 创建成员
            mainContext.ChatChannelMembers.Add(new ChatChannelMember { ChatChannelId = entry.Entity.Id, MemberId = state.User.GetUserId() });
            foreach (var userId in _checkedUserSet)
            {
                mainContext.ChatChannelMembers.Add(new ChatChannelMember { ChatChannelId = entry.Entity.Id, MemberId = userId });
            }

            await mainContext.SaveChangesAsync();
            trans.Commit();

            // 创建聊天信息分库
            using var channelDbContext = _chatDbFactory.CreateDbContext(entry.Entity.Id);
            channelDbContext.Database.EnsureCreated();

            MudDialog?.Close(DialogResult.Ok(true));
        }
    }
}
