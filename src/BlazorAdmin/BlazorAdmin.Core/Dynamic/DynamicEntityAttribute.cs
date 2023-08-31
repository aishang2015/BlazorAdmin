namespace BlazorAdmin.Core.Dynamic
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DynamicEntityAttribute : Attribute
	{
		public string? Title { get; set; }

		public bool HaveNumberColumn { get; set; } = true;

		public bool AllowEdit { get; set; }

		public bool AllowDelete { get; set; }

		public bool AllowAdd { get; set; }
	}
}
