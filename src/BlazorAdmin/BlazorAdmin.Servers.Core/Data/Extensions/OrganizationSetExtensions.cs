using BlazorAdmin.Data.Entities.Rbac;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Extensions
{
    public static class OrganizationSetExtensions
    {
        public static List<Organization> GetAllSubOrganiations(this DbSet<Organization> organizations, int orgId)
        {
            var orgList = organizations.AsNoTracking().ToList();
            return GetSubOrganiationIdRecursive(orgList, orgId);
        }

        private static List<Organization> GetSubOrganiationIdRecursive(this List<Organization> organizations, int orgId)
        {
            var result = new List<Organization>();
            var findOrg = organizations.FirstOrDefault(o => o.Id == orgId);
            if (findOrg != null)
            {
                result.Add(findOrg);
            }
            result.AddRange(organizations.Where(o => o.ParentId == orgId)
                .SelectMany(o => GetSubOrganiationIdRecursive(organizations, o.Id)));
            return result;
        }
    }
}
