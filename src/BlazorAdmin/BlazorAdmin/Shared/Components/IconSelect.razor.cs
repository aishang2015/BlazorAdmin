using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Reflection;

namespace BlazorAdmin.Shared.Components
{
	public partial class IconSelect
	{
		[Parameter] public string? SelectedIcon { get; set; }

		[Parameter] public EventCallback<string?> SelectedIconChanged { get; set; }

		private bool _popoverOpen;

		private bool _isMouseOnPopover;

		private List<VirtualizedIcon> IconList = new();

		private List<VirtualizedIcon> FilterIconList = new();

		public MudChip SelectedType = new();

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



		protected override void OnParametersSet()
		{
			if (string.IsNullOrEmpty(SelectedIcon))
			{
				SelectedIcon = Icons.Material.Filled.HolidayVillage;
			}
		}

		private void SelectedTypeChanged(MudChip chip)
		{
			SelectedType = chip;
			if (chip != null)
			{
				GetIcons((Type)chip.Value);
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
			FilterIcons();
		}

		private async Task SelectIcon(string content)
		{
			SelectedIcon = content;
			await SelectedIconChanged.InvokeAsync(content);
			_popoverOpen = false;
		}

		private void GetIcons(Type iconType)
		{
			var fields = iconType.GetFields(BindingFlags.Public | BindingFlags.Static);
			IconList.Clear();
			IconList.AddRange(fields.Select(f => new VirtualizedIcon
			{
				Name = f.Name,
				Content = f.GetValue(null)?.ToString()
			}));
			FilterIcons();
		}

		private void FilterIcons()
		{
			if (!string.IsNullOrEmpty(SearchKeyWord))
			{
				FilterIconList = IconList.Where(i => i.Name!.Contains(SearchKeyWord, StringComparison.OrdinalIgnoreCase)).ToList();
			}
			else
			{
				FilterIconList = IconList.ToList();
			}
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
