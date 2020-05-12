$(document).ready(function () {
    // 設定日期欄位套用 bs datepicker
    $('#DocumentDate').datepicker({
        format: 'yyyy/mm/dd',
        language: "zh-TW",
        autoclose: true,
        todayBtn: true,
        todayHighlight: true
    });
    $('#FromDate').datepicker({
        format: 'yyyy/mm/dd',
        language: "zh-TW",
        autoclose: true,
        todayBtn: true,
        todayHighlight: true
    });
});