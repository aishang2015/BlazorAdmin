using BlazorAdmin.Pages.Dialogs.AuditLog;
using BlazorAdmin.Pages.Dialogs.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MudBlazor;

namespace BlazorAdmin.Pages.Infos
{
	public partial class AuditLog
	{
		private List<AuditLogModel> AuditLogs = new();

		private int Page = 1;

		private int Size = 10;

		private int Total = 0;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			await InitialAsync();
		}


		private async Task InitialAsync()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			var query = from log in context.AuditLogs
						join user in context.Users on log.UserId equals user.Id
						orderby log.OperateTime descending
						select new
						{
							log.Id,
							log.TransactionId,
							user.Name,
							log.Operation,
							log.OperateTime,
							log.EntityName,
						};
			var model = context.GetService<IDesignTimeModel>().Model;
			AuditLogs = query.Skip((Page - 1) * Size).Take(Size).ToList()
				.Select((d, i) => new AuditLogModel
				{
					Number = i + (Page - 1) * Size + 1,
					Id = d.Id,
					TransactionId = d.TransactionId,
					EntityName = model.FindEntityType(d.EntityName)?.GetComment(),
					OperateTime = d.OperateTime,
					Operation = d.Operation,
					UserName = d.Name
				}).ToList();
			Total = query.Count();
		}

		private async Task ViewDetail(Guid id)
		{
			var parameters = new DialogParameters
			{
				{"AuditLogId",id }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			await _dialogService.Show<AuditLogDetailDialog>(string.Empty, parameters, options).Result;

		}

		private async void PageChangedClick(int page)
		{
			Page = page;
			await InitialAsync();
		}

		private class AuditLogModel
		{
			public int Number { get; set; }

			public Guid Id { get; set; }

			public Guid TransactionId { get; set; }

			public string UserName { get; set; } = null!;

			public string? EntityName { get; set; }

			public int Operation { get; set; }

			public DateTime OperateTime { get; set; }
		}
	}
}
