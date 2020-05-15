// 搭配 Datatables 套件的 Table 使用
function SetDatatables(elementId) {
    // "#" + elementId + "" => 動態賦予 ID 方法
    var table = $("#" + elementId + "").DataTable({
        // ※特別注意 : retrieve 和 destroy 不可以一起使用，會有無法預期的錯誤。
        // retrieve: 如果 id = table 的 Table 已經建置過，又被呼叫，此參數會避免再次初始化，回傳剛剛建置的 Table。
        // ※注意：這無法在重新初始化重設參數        
        retrieve: true,
        // destroy: 如果 id = table 的 Table 已經建置過，又被呼叫，此參數會移除舊有的 Table 建置，讓你重新初始化一次。
        // ※注意：因為用到初始化會用到大量程式碼，可能影響效能。例如: 動態啟用和禁用功能
        //destroy: true,
        // 隱藏搜尋元件
        searching: false,
        // url 設定中文語系，emptyTable 沒資料的時候顯示的字樣
        language: { "url": "../MoiraTool/Plugins/Datatables/dataTables.Chinese-traditional.json", "emptyTable": "沒有資料" },
        // 決定顯示幾筆資料的選單內容
        lengthMenu: [10],
        // 隱藏決定顯示幾筆資料的選單
        lengthChange: false,
        // 不可以案欄位標題排序
        ordering: false,
        // 設置依欄位位置列寬
        // Html Table 的 Style 記得要設 width:100%
        columnDefs: [
            { "width": "33%", "targets": 0 },
            { "width": "33%", "targets": 1 },
            { "width": "33%", "targets": 2 }
        ],        
        // 自動寬度
        autoWidth: false,    
    });

    // 追加 click 事件 到 table 中所有 包含 data class 的 td
    // ※ function on 的用法:
    // attach an evend handler function for one or more events to the selected elements.
    // 將一個或多個事件的事件處理程序函數附加到所選元素
    //$("#" + elementId + " tbody" + "").on('click', 'td.data', function () {
    //    var data = table.row(this).data();
    //    alert('You clicked on ' + data[1] + '\'s row');
    //});
}