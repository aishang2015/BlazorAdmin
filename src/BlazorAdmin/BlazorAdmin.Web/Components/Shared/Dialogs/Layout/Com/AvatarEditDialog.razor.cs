using Cropper.Blazor.Components;
using Cropper.Blazor.Extensions;
using Cropper.Blazor.Models;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.RegularExpressions;

namespace BlazorAdmin.Web.Components.Shared.Dialogs.Layout.Com
{
	public partial class AvatarEditDialog
	{
		[Parameter] public IBrowserFile? BrowserFile { get; set; }
		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }

		private string src = "";

		private Options _options = new Options
		{
			AspectRatio = 1,
			ViewMode = ViewMode.Vm1,
		};
		private CropperComponent? cropperComponent;
		private ElementReference ElementReference;


		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				_options.Preview = new ElementReference[]
				{
					ElementReference
				};

				if (BrowserFile != null && cropperComponent != null)
				{
					string oldSrc = src;

					src = await cropperComponent.GetImageUsingStreamingAsync(BrowserFile, BrowserFile.Size);


					cropperComponent?.Destroy();
					cropperComponent?.RevokeObjectUrlAsync(oldSrc);

					StateHasChanged();
				}
			}
		}

		public void OnErrorLoadImageEvent(Microsoft.AspNetCore.Components.Web.ErrorEventArgs errorEventArgs)
		{
			Destroy();
			StateHasChanged();
		}

		private void Destroy()
		{
			cropperComponent?.Destroy();
			cropperComponent?.RevokeObjectUrlAsync(src);
		}

		public async Task SaveAvatar()
		{
			GetCroppedCanvasOptions getCroppedCanvasOptions = new GetCroppedCanvasOptions
			{
				MaxHeight = 4096,
				MaxWidth = 4096,
				ImageSmoothingQuality = ImageSmoothingQuality.High.ToEnumString()
			};

			var croppedCanvasDataURL = await cropperComponent!.GetCroppedCanvasDataURLAsync(getCroppedCanvasOptions);
			var fileName = SaveDataUrlToFile(croppedCanvasDataURL, Path.Combine(AppContext.BaseDirectory, "Avatars"));

			var userState = await _stateProvider.GetAuthenticationStateAsync();
			using var context = await _dbFactory.CreateDbContextAsync();
			var user = context.Users.Find(userState.User.GetUserId());
			if (user != null)
			{
				var oldFileName = user.Avatar;
				user.Avatar = fileName;
				await context.SaveChangesAsync();
				if (!string.IsNullOrEmpty(oldFileName))
				{
					File.Delete(Path.Combine(AppContext.BaseDirectory, "Avatars", oldFileName));
				}
				_snackbarService.Add("头像设置成功！", Severity.Success);
			}
			MudDialog?.Close(DialogResult.Ok(true));
		}

		private string SaveDataUrlToFile(string dataUrl, string savePath)
		{
			if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}

			var matchGroups = Regex.Match(dataUrl, @"^data:((?<type>[\w\/]+))?;base64,(?<data>.+)$").Groups;
			var base64Data = matchGroups["data"].Value;
			var binData = Convert.FromBase64String(base64Data);

			var endfix = matchGroups["type"].Value switch
			{
				"image/png" => ".png",
				"image/jpeg" => ".jpeg",
				"image/gif" => ".gif",
				_ => ".jpg"
			};

			var newFileName = Guid.NewGuid().ToString("N") + endfix;
			File.WriteAllBytes(Path.Combine(savePath, newFileName), binData);
			return newFileName;
		}
	}
}
