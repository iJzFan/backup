

$(function () {
    initialDateTimePicker();
});

function initialDateTimePicker() {
    //出生日期控件
    $('.form_date').datetimepicker({
        language: 'zh-CN',
        weekStart: 1,
        todayBtn: 1,
        autoclose: 1,
        todayHighlight: 1,
        pickerPosition: "bottom-left",
        startView: 2,
        minView: 2,
        forceParse: 0
    });
}