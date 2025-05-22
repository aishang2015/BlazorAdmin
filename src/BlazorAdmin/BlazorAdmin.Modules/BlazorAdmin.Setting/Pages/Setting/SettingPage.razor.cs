using BlazorAdmin.Setting.Pages.Setting.Com;
using MudBlazor;

namespace BlazorAdmin.Setting.Pages.Setting
{
    public partial class SettingPage
    {
        private MudListItem<SettingModel>? SelectedItem;

        private SettingModel? SelectedValue;

        private List<SettingModel> SettingGroups = new();

        private Dictionary<string, Type> SettingComDic = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            SettingGroups = new List<SettingModel>{
                new SettingModel
                {
                    Name = _loc["JwtComTitle"],
                    Icon = Icons.Material.Filled.Security
                },
            };
            SettingComDic = new Dictionary<string, Type>
            {
                 {_loc["JwtComTitle"],typeof(JwtCom)},
            };
            SelectedValue = SettingGroups.First();
        }

        private class SettingModel
        {
            public string Name { get; set; } = null!;

            public string Icon { get; set; } = null!;
        }

    }
}
