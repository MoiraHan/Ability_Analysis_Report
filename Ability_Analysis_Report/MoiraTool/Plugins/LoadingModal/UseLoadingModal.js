//顯示 loading 視窗
function showLoading() {
    $('#loadingModal').modal({ backdrop: 'static', keyboard: false });
}
//隐藏 loading 視窗
function hideLoading() {
    $('#loadingModal').modal('hide');
}