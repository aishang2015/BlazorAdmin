namespace BlazorAdmin.Core.Dynamic
{
	public class DynamicEntityInfo
	{
		public string EntityName { get; set; } = null!;

		public Type EntityType { get; set; } = null!;

		public string? Title { get; set; }

		public bool AllowEdit { get; set; } = true;

		public bool AllowDelete { get; set; } = true;

		public bool AllowAdd { get; set; } = true;

		public List<DynamicPropertyInfo> DynamicPropertyInfos { get; set; } = new();
	}

	public class DynamicPropertyInfo
	{
		public string PropertyName { get; set; } = null!;

		public Type PropertyType { get; set; } = null!;

		public string? Title { get; set; }

		public string? Format { get; set; }

		public int Order { get; set; } = 0;

		public bool IsDisplay { get; set; } = true;

		public bool AllowEdit { get; set; } = true;
	}
}
