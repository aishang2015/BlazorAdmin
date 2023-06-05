using BlazorAdmin.Data;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Services
{

	public interface IAccessService
	{
		Task<List<int>> GetCanAccessedMenus();

		Task<bool> CheckUriCanAccess(string url);
	}

	public class AccessService : IAccessService
	{

		private readonly IDbContextFactory<BlazorAdminDbContext> _dbContextFactory;

		private readonly AuthenticationStateProvider _stateProvider;

		public AccessService(IDbContextFactory<BlazorAdminDbContext> dbContextFactory,
			AuthenticationStateProvider stateProvider)
		{
			_dbContextFactory = dbContextFactory;
			_stateProvider = stateProvider;
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
			using var context = await _dbContextFactory.CreateDbContextAsync();
			var userState = await _stateProvider.GetAuthenticationStateAsync();

			if (userState.User.Identity == null || !userState.User.Identity.IsAuthenticated)
			{
				return false;
			}

			var userId = userState.User.GetUserId();

			var query = from r in context.Roles
						join ur in context.UserRoles on r.Id equals ur.RoleId
						join rm in context.RoleMenus on r.Id equals rm.RoleId
						join m in context.Menus on rm.MenuId equals m.Id
						where ur.UserId == userId && r.IsEnabled && url == m.Route
						select m;
			return query.Any();
		}

	}
}
