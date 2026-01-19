// JumboDogX Web UI JavaScript functions

/**
 * ファイルをダウンロードする
 * @param {string} filename - ダウンロードするファイル名
 * @param {string} content - ファイルの内容
 */
window.downloadFile = function (filename, content) {
    const blob = new Blob([content], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
