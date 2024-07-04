using BlazorAdmin.Core.Extension;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Reflection;
using static MudBlazor.CategoryTypes;

namespace BlazorAdmin.Rbac.Components
{
    public partial class IconSelect
    {
        [Parameter] public string? SelectedIcon { get; set; }

        [Parameter] public EventCallback<string?> SelectedIconChanged { get; set; }

        private bool _popoverOpen;

        private bool _isMouseOnPopover;

        private List<List<VirtualizedIcon>> IconList = new();

        private List<List<VirtualizedIcon>> FilterIconList = new();

        public Type? SelectedType = typeof(Icons.Material.Filled);

        private string? SearchKeyWord;

        private string SelectIconGroup = "Material";

        private List<Type> MaterialIconTypes = new List<Type>()
        {
            typeof(Icons.Material.Filled),
            typeof(Icons.Material.Outlined),
            typeof(Icons.Material.Rounded),
            typeof(Icons.Material.Sharp),
            typeof(Icons.Material.TwoTone),
        };

        private List<Type> CustomIconTypes = new List<Type>()
        {
            typeof(Icons.Custom.Brands),
            typeof(Icons.Custom.FileFormats),
            typeof(Icons.Custom.Uncategorized),
        };

        protected override void OnInitialized()
        {
            base.OnInitialized();
            GetIcons(SelectedType!);
        }


        protected override void OnParametersSet()
        {
            if (string.IsNullOrEmpty(SelectedIcon))
            {
                SelectedIcon = Icons.Material.Filled.HolidayVillage;
            }
        }

        private void SelectIconGroupChanged(string group)
        {
            SelectIconGroup = group;
            if (group == "Material")
            {
                SelectedType = MaterialIconTypes.FirstOrDefault();
            }
            else if (group == "Custom")
            {
                SelectedType = CustomIconTypes.FirstOrDefault();
            }
            GetIcons(SelectedType!);
        }

        private void SelectedTypeChanged(Type chip)
        {
            SelectedType = chip;
            if (chip != null)
            {
                GetIcons(chip);
            }
            else
            {
                IconList.Clear();
                FilterIconList.Clear();
            }
        }

        private void TextChanged(string keyword)
        {
            SearchKeyWord = keyword;

            if (SelectedType == null)
            {
                GetIcons(SelectedType, keyword);
            }
        }

        private async Task SelectIcon(string content)
        {
            SelectedIcon = content;
            await SelectedIconChanged.InvokeAsync(content);
            _popoverOpen = false;
        }

        private void GetIcons(Type iconType, string keyword = "")
        {
            var fields = iconType.GetFields(BindingFlags.Public | BindingFlags.Static);
            IconList.Clear();
            var iconList = fields
                .AndIf(!string.IsNullOrEmpty(keyword), f => f.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Select(f => new VirtualizedIcon
                {
                    Name = f.Name,
                    Content = f.GetValue(null)?.ToString()
                }).ToList();

            for (int i = 0; i < iconList.Count();)
            {
                var oneList = new List<VirtualizedIcon>();
                for (int j = 0; j < 4; j++)
                {
                    oneList.Add(iconList[i]);
                    i++;

                    if (i >= iconList.Count())
                    {
                        break;
                    }
                }
                IconList.Add(oneList);
            }

            FilterIconList = IconList;
        }

        private void ElementFocused()
        {
            _popoverOpen = !_popoverOpen;
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


        private class VirtualizedIcon
        {
            public string? Name { get; set; }

            public string? Content { get; set; }
        }
    }
}
