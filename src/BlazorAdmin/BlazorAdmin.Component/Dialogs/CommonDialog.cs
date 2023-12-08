using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Component.Dialogs
{
    public static class CommonDialog
    {
        public delegate Task DialogMethodDelegate(CommonDialogEventArgs args);

        /// <summary>
        /// 删除确认对话框
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="title">标题</param>
        /// <param name="confirmButtonText">按钮文本</param>
        /// <param name="confirmCallBackMethod">确认后的回调，此方法会在dialog close 之前执行。需要确认后再执行，使用返回值判断即可</param>
        /// <returns>返回是否确认</returns>
        public static async Task<bool> ShowDeleteDialog(this IDialogService dialogService,
            string? title = null, string? confirmButtonText = null, DialogMethodDelegate? confirmCallBackMethod = null)
        {
            var confirmCallBack = new EventCallback<CommonDialogEventArgs>(null,
                async (CommonDialogEventArgs e) =>
                {
                    if (confirmCallBackMethod != null)
                    {
                        await confirmCallBackMethod(e);
                    };
                });
            var parameters = new DialogParameters
            {
                {"Title", title},
                {"ConfirmButtonText", confirmButtonText},
                {"ConfirmCallBack", confirmCallBack }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true, };
            var result = await dialogService.Show<CommonDeleteDialog>(string.Empty, parameters, options).Result;
            return !result.Canceled;
        }

        /// <summary>
        /// 用户密码确认
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="confirmCallBackMethod"></param>
        /// <returns></returns>
        public static async Task<bool> ShowConfirmUserPasswodDialog(this IDialogService dialogService,
            DialogMethodDelegate? confirmCallBackMethod = null)
        {
            var confirmCallBack = new EventCallback<CommonDialogEventArgs>(null,
                async (CommonDialogEventArgs e) =>
                {
                    if (confirmCallBackMethod != null)
                    {
                        await confirmCallBackMethod(e);
                    };
                });
            var parameters = new DialogParameters
            {
                {"ConfirmCallBack", confirmCallBack }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true, };
            var result = await dialogService.Show<ConfirmUserPasswordDialog>(string.Empty, parameters, options).Result;
            return !result.Canceled;
        }
    }
}
