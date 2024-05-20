using BlazorAdmin.Setting.Pages.Setting.Com;
using MudBlazor;

namespace BlazorAdmin.Setting.Pages.Setting
{
    public partial class SettingPage
    {
        private MudListItem<SettingModel>? SelectedItem;

        private SettingModel? SelectedValue;

        private List<SettingModel> SettingGroups = new List<SettingModel>{
            new SettingModel
            {
                Name = "JWT配置",
                Icon = Icons.Material.Filled.Security
            },
        };

        private Dictionary<string, Type> SettingComDic = new Dictionary<string, Type>
        {
             {"JWT配置",typeof(JwtCom)},
        };

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            SelectedValue = SettingGroups.First();
        }

        private class SettingModel
        {
            public string Name { get; set; } = null!;

            public string Icon { get; set; } = null!;
        }

    }
}
