using BlazorAdmin.Core.Dynamic;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Dynamic;

namespace BlazorAdmin.Pages.Dynamic
{
	public partial class Dynamic
	{
		[Parameter] public string EntityName { get; set; } = null!;

		private int Page = 1;

		private int Size = 10;

		private int Total = 0;

		private string? Title;

		private List<Field> Fields = new();

		private List<dynamic> TableItems = new();

		#region 

		private DynamicEntityInfo? EntityInfo;

		private dynamic UtilInstance = null!;

		#endregion


		protected override async Task OnParametersSetAsync()
		{
			await base.OnParametersSetAsync();

			EntityInfo = DynamicLoader.LoadedDynamicEntityInfos
				.FirstOrDefault(t => string.Equals(t.EntityName, EntityName, StringComparison.CurrentCultureIgnoreCase));

			if (EntityInfo == null)
			{
				throw new KeyNotFoundException($"{EntityName} not found.");
			}

			Title = EntityInfo.Title;

			var utilType = DynamicLoader.LoadedDynamicAssembly!.GetTypes()
				.FirstOrDefault(t => t.Name == $"{EntityInfo.EntityName}Util")!;
			UtilInstance = Activator.CreateInstance(utilType!)!;

			await InitialDataAsync();
		}

		private async Task InitialDataAsync()
		{
			var methodInfo = UtilInstance.GetType().GetMethod($"Get{EntityInfo!.EntityName}");
			var context = await _dbFactory.CreateDbContextAsync();
			var result = methodInfo!.Invoke(UtilInstance, new object[] { context, Page, Size });
			var list = result.Item1;
			Total = result.Item2;

			Fields.Clear();
			var dynamicFieldInfoList = EntityInfo.DynamicPropertyInfos.OrderBy(p => p.Order);
			foreach (var property in dynamicFieldInfoList!)
			{
				Fields.Add(new Field
				{
					Title = property.Title,
					Name = property.PropertyName,
					IsDisplay = property.IsDisplay,
				});
			}

			TableItems.Clear();
			foreach (var data in list)
			{
				IDictionary<string, object> obj = new ExpandoObject()!;
				foreach (var property in EntityInfo!.EntityType.GetProperties())
				{
					obj.Add(property.Name, property.GetValue(data, null));
				}
				TableItems.Add(obj);
			}
		}

		private async Task PageChangedClick(int page)
		{
			Page = page;
			await InitialDataAsync();
		}

		private async Task AddOneRecord()
		{
			var parameters = new DialogParameters
			{
				{ "EntityInfo", EntityInfo },
				{ "UtilInstance", UtilInstance }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<DynamicAddDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialDataAsync();
			}
		}

		private async Task DeleteOneRecord(dynamic context)
		{
			var keys = new List<object>();
			foreach (var property in EntityInfo!.DynamicPropertyInfos.Where(p => p.IsKey))
			{
				var value = ((IDictionary<string, object>)context)[property.PropertyName!];
				keys.Add(value);
			}

			var parameters = new DialogParameters
			{
				{ "EntityInfo", EntityInfo },
				{ "UtilInstance", UtilInstance },
				{ "Keys",keys.ToArray()}
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<DynamicDeleteDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialDataAsync();
			}
		}

		private async Task EditOneRecord(dynamic context)
		{
			var parameters = new DialogParameters
			{
				{ "EntityInfo", EntityInfo },
				{ "UtilInstance", UtilInstance },
				{ "Context", context}
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<DynamicEditDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialDataAsync();
			}

		}

		private class Field
		{
			public string? Title { get; set; }

			public string? Name { get; set; }

			public bool IsDisplay { get; set; }
		}
	}
}
