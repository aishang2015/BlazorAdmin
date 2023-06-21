using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;

namespace BlazorAdmin.Core.Dynamic
{
	public static class DynamicLoader
	{
		public static Assembly? LoadedDynamicAssembly { get; set; } = null;

		public static List<DynamicEntityInfo> LoadedDynamicEntityInfos { get; set; } = new();

		public static void Load()
		{
			var classes = DependencyContext.Default!.CompileLibraries
				.Where(lib => !lib.Serviceable && lib.Type != "referenceassembly")
				.Select(lib => Assembly.Load(lib.Name))
				.SelectMany(a => a.GetTypes())
				.Where(t => t.Namespace == "BlazorAdmin.Data.Entities").ToList();

			if (classes.Any())
			{
				var template = """
					using BlazorAdmin.Data;
					using BlazorAdmin.Data.Entities;
					using System.Collections.Generic;
					using System.Linq;

					public class {Entity}Util {
						public (List<{Entity}>,int) Get{Entity}(BlazorAdminDbContext dbContext, int page ,int size){
							var result = dbContext.Set<{Entity}>().Skip((page - 1) * size).Take(size).ToList();
							var count = dbContext.Set<{Entity}>().Count();
							return (result, count);
						}
					}

					""";
				var syntaxTreeList = new List<SyntaxTree>();
				foreach (var classType in classes)
				{
					var utilCode = template.Replace("{Entity}", classType.Name);
					syntaxTreeList.Add(SyntaxFactory.ParseSyntaxTree(utilCode));

					var entityInfo = new DynamicEntityInfo();
					entityInfo.EntityName = classType.Name;
					entityInfo.EntityType = classType;
					var entityAttributeInfo = classType.GetCustomAttribute<DynamicEntityAttribute>();
					if (entityAttributeInfo != null)
					{
						entityInfo.Title = entityAttributeInfo.Title;
						entityInfo.AllowEdit = entityAttributeInfo.AllowEdit;
						entityInfo.AllowDelete = entityAttributeInfo.AllowDelete;
						entityInfo.AllowEdit = entityAttributeInfo.AllowEdit;
					}
					else
					{
						entityInfo.Title = classType.Name;
					}

					var propertyInfoList = new List<DynamicPropertyInfo>();
					foreach (var p in classType.GetProperties())
					{
						var propertyInfo = new DynamicPropertyInfo();
						propertyInfo.PropertyName = p.Name;
						propertyInfo.PropertyType = p.PropertyType;
						var propertyAttributeInfo = p.GetCustomAttribute<DynamicPropertyAttribute>();
						if (propertyAttributeInfo != null)
						{
							propertyInfo.Title = propertyAttributeInfo.Title;
							propertyInfo.Format = propertyAttributeInfo.Format;
							propertyInfo.Order = propertyAttributeInfo.Order;
							propertyInfo.IsDisplay = propertyAttributeInfo.IsDisplay;
							propertyInfo.AllowEdit = propertyAttributeInfo.AllowEdit;
						}
						else
						{
							propertyInfo.Title = p.Name;
						}

						propertyInfoList.Add(propertyInfo);
					}
					entityInfo.DynamicPropertyInfos = propertyInfoList;
					LoadedDynamicEntityInfos.Add(entityInfo);
				}

				var compilation = CSharpCompilation.Create(
					 syntaxTrees: syntaxTreeList,
					 assemblyName: $"dynamicClass.dll",
					 options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
					 references: AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic)
						.Select(x => MetadataReference.CreateFromFile(x.Location)));

				Assembly compiledAssembly;
				using (var stream = new MemoryStream())
				{
					var compileResult = compilation.Emit(stream);
					if (compileResult.Success)
					{
						compiledAssembly = Assembly.Load(stream.GetBuffer());
					}
					else
					{
						throw new Exception("Roslyn compile err .");
					}
				}
				LoadedDynamicAssembly = compiledAssembly;
			}
		}
	}
}
