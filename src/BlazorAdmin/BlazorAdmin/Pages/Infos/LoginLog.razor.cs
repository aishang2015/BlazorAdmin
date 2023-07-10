using BlazorAdmin.Core.Extension;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Pages.Infos
{
	public partial class LoginLog
	{
		private List<LoginLogModel> LoginLogs = new();

		private int Page = 1;

		private int Size = 10;

		private int Total = 0;

		private string? SearchedLoginName;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			await InitialAsync();
		}
		private async Task InitialAsync()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			var query = context.LoginLogs.
				AndIfExist(SearchedLoginName, l => l.UserName == SearchedLoginName);
			LoginLogs = query.OrderByDescending(l => l.Id)
				.Skip((Page - 1) * Size).Take(Size).ToList()
				.Select((l, i) => new LoginLogModel
				{
					Id = l.Id,
					Number = i + 1 + (Page - 1) * Size,
					Agent = l.Agent,
					Ip = l.Ip,
					Time = l.Time,
					IsSuccessd = l.IsSuccessd,
					UserName = l.UserName,
				}).ToList();
			Total = query.Count();
		}

		private async void PageChangedClick(int page)
		{
			Page = page;
			await InitialAsync();
		}

		private class LoginLogModel
		{
			public int Id { get; set; }
			public int Number { get; set; }
			public string UserName { get; set; } = null!;
			public DateTime Time { get; set; }
			public string? Agent { get; set; }
			public string? Ip { get; set; }
			public bool IsSuccessd { get; set; }
		}
	}
}
