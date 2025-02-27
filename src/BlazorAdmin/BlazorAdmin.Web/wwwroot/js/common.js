export function downloadFileFromBase64(base64String, fileName) {
    // 将base64字符串转换为Blob对象
    const byteCharacters = atob(base64String);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray]);

    // 创建下载链接
    const downloadLink = document.createElement('a');
    downloadLink.href = URL.createObjectURL(blob);
    downloadLink.download = fileName;

    // 添加到文档并触发点击
    document.body.appendChild(downloadLink);
    downloadLink.click();

    // 清理
    document.body.removeChild(downloadLink);
    URL.revokeObjectURL(downloadLink.href);
}