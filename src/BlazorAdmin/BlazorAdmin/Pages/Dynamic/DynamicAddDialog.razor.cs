using BlazorAdmin.Core.Dynamic;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Pages.Dynamic
{
	public partial class DynamicAddDialog
	{
		[CascadingParameter]
		MudDialogInstance? MudDialog { get; set; }

		[Parameter]
		public DynamicEntityInfo EntityInfo { get; set; } = null!;

		[Parameter]
		public dynamic UtilInstance { get; set; } = null!;

		private string? Title;

		private bool Success;

		private string[] Errors = { };

		private MudForm FormRef = new();

		private Dictionary<string, dynamic> FormValueDic = new();

		protected override void OnInitialized()
		{
			base.OnInitialized();
			Title = EntityInfo.Title;

			foreach (var p in EntityInfo.DynamicPropertyInfos)
			{
				dynamic refCom = p.PropertyType switch
				{
					var t when t == typeof(int) => new MudNumericField<int>(),
					var t when t == typeof(string) => new MudTextField<string>(),
					var t when t == typeof(bool) => new MudSwitch<bool>(),
					_ => throw new NotImplementedException()
				};
				FormValueDic.Add(p.PropertyName, refCom);
			}
		}

		public async Task FormSubmit()
		{
			await FormRef.Validate();

			var obj = Activator.CreateInstance(EntityInfo.EntityType);
			EntityInfo.DynamicPropertyInfos.Where(p => p.IsDisplay)
				.ToList().ForEach(p =>
				{
					dynamic value;
					if (p.PropertyType == typeof(int))
					{
						value = (FormValueDic[p.PropertyName] as MudNumericField<int>)!.Value;
					}

					else if (p.PropertyType == typeof(string))
					{
						value = (FormValueDic[p.PropertyName] as MudTextField<string>)!.Value;
					}

					else if (p.PropertyType == typeof(bool))
					{
						value = (FormValueDic[p.PropertyName] as MudSwitch<bool>)!.Checked;
					}
					else
					{
						throw new NotImplementedException();
					}

					EntityInfo.EntityType.GetProperty(p.PropertyName)!.SetValue(obj, value);

				});

			var methodInfo = UtilInstance.GetType().GetMethod($"Add{EntityInfo!.EntityName}");
			var context = await _dbFactory.CreateDbContextAsync();
			methodInfo!.Invoke(UtilInstance, new object[] { context, obj! });
			MudDialog?.Close(DialogResult.Ok(true));
		}
	}
}
