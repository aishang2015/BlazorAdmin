using BlazorAdmin.Core.Dynamic;
using BlazorAdmin.Data;
using BlazorAdmin.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyModel;
using System.Dynamic;
using System.Reflection;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;

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

		private Type QueryUtilType = null!;

		private Type? EntityType = null!;

		#endregion


		protected override async Task OnParametersSetAsync()
		{
			await base.OnParametersSetAsync();

			EntityType = DynamicLoader.LoadedDynamicEntityInfos
				.FirstOrDefault(t => string.Equals(t.EntityName, EntityName, StringComparison.CurrentCultureIgnoreCase))?
				.EntityType;

			if (EntityType == null)
			{
				throw new KeyNotFoundException($"{EntityName} not found.");
			}

			Title = EntityType.Name;

			QueryUtilType = DynamicLoader.LoadedDynamicAssembly!.GetTypes()
				.FirstOrDefault(t => t.Name == $"{EntityType.Name}Util")!;

			await InitialDataAsync();
		}

		private async Task InitialDataAsync()
		{
			dynamic utilInstance = Activator.CreateInstance(QueryUtilType!)!;
			var methodInfo = utilInstance.GetType().GetMethod($"Get{EntityType!.Name}");
			var context = await _dbFactory.CreateDbContextAsync();
			var result = methodInfo!.Invoke(utilInstance, new object[] { context, Page, Size });
			var list = result.Item1;
			Total = result.Item2;

			Fields.Clear();
			var dynamicFieldInfoList = DynamicLoader.LoadedDynamicEntityInfos
				.FirstOrDefault(t => string.Equals(t.EntityName, EntityName, StringComparison.CurrentCultureIgnoreCase))!
				.DynamicPropertyInfos.OrderBy(p => p.Order);
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
				foreach (var property in EntityType.GetProperties())
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

		private class Field
		{
			public string? Title { get; set; }

			public string? Name { get; set; }

			public bool IsDisplay { get; set; }
		}
	}
}
