// 刪除確認視窗，使用方法 onclick = "return DeleteConfirm()"
function DeleteConfirm() {
    if (!confirm("您確定要刪除嗎?"))
        return false;
}