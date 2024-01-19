using BlazorAdmin.Setting.Pages.Setting.Com;
using MudBlazor;

namespace BlazorAdmin.Setting.Pages.Setting
{
    public partial class SettingPage
    {
        private MudListItem? SelectedItem;

        private object SelectedValue = "JWT配置";

        private List<dynamic> SettingGroups = [
            new
            {
                Name = "JWT配置",
                Icon = Icons.Material.Filled.Security
            }
            ];

        private Dictionary<string, Type> SettingComDic = new Dictionary<string, Type>
        {
             {"JWT配置",typeof(JwtCom)}
        };

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

    }
}
