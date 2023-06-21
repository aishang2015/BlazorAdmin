namespace BlazorAdmin.Core.Dynamic
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DynamicPropertyAttribute : Attribute
	{
		public string? Title { get; set; }

		public string? Format { get; set; }

		public int Order { get; set; } = 0;

		public bool IsDisplay { get; set; } = true;

		public bool AllowEdit { get; set; } = true;
	}
}
