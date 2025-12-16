using BlazorAdmin.Servers.Core.Auth;
using Cropper.Blazor.Components;
using Cropper.Blazor.Extensions;
using Cropper.Blazor.Models;
using Cropper.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Text.RegularExpressions;

namespace BlazorAdmin.Layout.Components.UserAvatar.Dialogs.Com
{
    public partial class AvatarEditDialog
    {
        [Parameter] public IBrowserFile? BrowserFile { get; set; }
        [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }
        [Inject] private IUrlImageInterop UrlImageInterop { get; set; } = null!;

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

                    src = await UrlImageInterop.GetImageUsingStreamingAsync(BrowserFile, BrowserFile.Size);


                    cropperComponent?.Destroy();
                    UrlImageInterop?.RevokeObjectUrlAsync(oldSrc);

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
            UrlImageInterop?.RevokeObjectUrlAsync(src);
        }

        public async Task SaveAvatar()
        {
            GetCroppedCanvasOptions getCroppedCanvasOptions = new GetCroppedCanvasOptions
            {
                MaxHeight = 4096,
                MaxWidth = 4096,
                ImageSmoothingQuality = ImageSmoothingQuality.High.ToEnumString()
            };

            var imageReceiver = await cropperComponent!.GetCroppedCanvasDataInBackgroundAsync(getCroppedCanvasOptions);
            using MemoryStream croppedCanvasDataStream = await imageReceiver.GetImageChunkStreamAsync();
            byte[] croppedCanvasData = croppedCanvasDataStream.ToArray();
            string croppedCanvasDataURL = "data:image/png;base64," + Convert.ToBase64String(croppedCanvasData); 
            
            var fileName = SaveDataUrlToFile(croppedCanvasDataURL, Path.Combine(AppContext.BaseDirectory, "Avatars"));

            var userId = await _stateProvider.GetUserIdAsync();
            using var context = await _dbFactory.CreateDbContextAsync();
            var user = context.Users.Find(userId);
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
