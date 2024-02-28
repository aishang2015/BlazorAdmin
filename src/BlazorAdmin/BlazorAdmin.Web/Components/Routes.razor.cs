using System.Reflection;

namespace BlazorAdmin.Web.Components
{
    public partial class Routes
    {
        public static List<Assembly> AdditionalAssemblies = new List<Assembly>()
        {
            typeof(Rbac._Imports).Assembly,
            typeof(Log._Imports).Assembly,
            typeof(Setting._Imports).Assembly,
            typeof(Im._Imports).Assembly,
            typeof(About.Client._Imports).Assembly,
        };
    }
}
