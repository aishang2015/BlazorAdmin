using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BlazorAdmin.Core.Services
{

    public interface IAccessService
    {
        Task<List<int>> GetCanAccessedMenus();

        Task<bool> CheckUriCanAccess(string url);

        Task<bool> CheckHasElementRights(string identify);
    }

    public class AccessService : IAccessService
    {

        private readonly IDbContextFactory<BlazorAdminDbContext> _dbContextFactory;

        private readonly AuthenticationStateProvider _stateProvider;

        private readonly IMemoryCache _memoryCache;

        public AccessService(
            IDbContextFactory<BlazorAdminDbContext> dbContextFactory,
            AuthenticationStateProvider stateProvider,
            IMemoryCache memoryCache)
        {
            _dbContextFactory = dbContextFactory;
            _stateProvider = stateProvider;
            _memoryCache = memoryCache;
        }


        public async Task<List<int>> GetCanAccessedMenus()
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var userState = await _stateProvider.GetAuthenticationStateAsync();

            if (userState.User.Identity == null || !userState.User.Identity.IsAuthenticated)
            {
                return new List<int>();
            }

            var userId = userState.User.GetUserId();

            var query = from r in context.Roles
                        join ur in context.UserRoles on r.Id equals ur.RoleId
                        join rm in context.RoleMenus on r.Id equals rm.RoleId
                        where ur.UserId == userId && r.IsEnabled
                        select rm.MenuId;

            return query.ToList();
        }


        public async Task<bool> CheckUriCanAccess(string url)
        {
            var userState = await _stateProvider.GetAuthenticationStateAsync();

            if (userState.User.Identity == null || !userState.User.Identity.IsAuthenticated)
            {
                return false;
            }

            var userId = userState.User.GetUserId();

            using var context = await _dbContextFactory.CreateDbContextAsync();
            var query = from r in context.Roles
                        join ur in context.UserRoles on r.Id equals ur.RoleId
                        join rm in context.RoleMenus on r.Id equals rm.RoleId
                        join m in context.Menus on rm.MenuId equals m.Id
                        where ur.UserId == userId && r.IsEnabled && url == m.Route
                        select m;
            return query.Any();
        }

        public async Task<bool> CheckHasElementRights(string identify)
        {
            var userState = await _stateProvider.GetAuthenticationStateAsync();
            if (userState.User.Identity == null || !userState.User.Identity.IsAuthenticated)
            {
                return false;
            }
            var userId = userState.User.GetUserId();

            var identities = await _memoryCache.GetOrCreateAsync($"{userId}Identifies", async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);

                using var context = await _dbContextFactory.CreateDbContextAsync();

                var query = from r in context.Roles
                            join ur in context.UserRoles on r.Id equals ur.RoleId
                            join rm in context.RoleMenus on r.Id equals rm.RoleId
                            join m in context.Menus on rm.MenuId equals m.Id
                            where ur.UserId == userId && r.IsEnabled && m.Type == 2
                            select m.Identify;

                return query.ToList();
            });
            return identities is not null ? identities.Contains(identify) : false;

        }

    }
}
