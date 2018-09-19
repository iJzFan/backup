//分页
$(function () {
    $(".ah-start-time").datetimepicker({
        language: 'zh-CN',
        minView: "month",
        todayBtn: 1,
        autoclose: 1,
        endDate: new Date()
    });
    $(".ah-end-time").datetimepicker({
        language: 'zh-CN',
        minView: "month",
        todayBtn: 1,
        autoclose: 1,
        endDate: new Date()
    });

    var pfn = { 
        //------------ 获取待发药数据------------------
        getWaitingDispensing: function (pageIndexSearch, searchText, startTime, endTime, func) {
            $('#myTabs #waiting_dispensing a').tab("show");
            $.loadJSON("/Customer/Dispensing/WaitingDispensing",
                {
                    pageIndexSearch: pageIndexSearch,
                    searchText: searchText || $('#searchform_waiting .ah-search-text').val(),
                    startTime: startTime || $('#searchform_waiting .ah-start-time').val(),
                    endTime: endTime || $('#searchform_waiting .ah-end-time').val()
                }, function (jn) {
                    if (jn.rlt) {
                        $('#waiting-badge1').html(jn.listNum);           
                        //列表
                        //模板与模板数据
                        var html = template('selInfoTemplate', jn);
                        //插入生成的数据
                        document.getElementById('waitingdispensing_list').innerHTML = html;
                        if (func) func();
                    } else $.alertError(jn.msg);
                });
        },
        //------------ 获取已发药数据---------------------
        getSuccessDispensing: function (pageIndexSearch, searchText, startTime, endTime, func) {
            $('#myTabs #success_dispensing a').tab("show");
            $.loadJSON("/Customer/Dispensing/SuccessDispensing", {
                pageIndexSearch: pageIndexSearch,
                searchText: searchText || $('#searchform_success .ah-search-text').val(),
                startTime: startTime || $('#searchform_success .ah-start-time').val(),
                endTime: endTime || $('#searchform_success .ah-end-time').val()
            }, function (jn) {
                if (jn.rlt) {
                    $('#waiting-badge2').html(jn.listNum);
                    //列表
                    //模板与模板数据
                    var html = template('successInfoTemplate', jn);
                    //插入生成的数据
                    document.getElementById('successdispensing_list').innerHTML = html;
                    if (func) func();
                } else $.alertError(jn.msg);
            });
        }
    }
    var a = {
        successFunc: function () {
            pfn.getSuccessDispensing(1, null, null, null, function () {
                var limit = 30;//一页显示数量
                //ajax获取后台数据后的操作
                var totalCount = $('#waiting-badge2').text();//总条数
                $('#callBackPager1').extendPagination({
                    totalCount: totalCount,
                    limit: limit,
                    callback: function (curr, totalCount) {
                        pfn.getSuccessDispensing(curr, null, null, null);
                    }
                });
            });
        },
        waitingFunc: function () {
            pfn.getWaitingDispensing(1, null, null, null, function () {
                var limit = 30;//一页显示数量
                //ajax获取后台数据后的操作
                var totalCount = $('#waiting-badge1').text();//总条数
                $('#callBackPager').extendPagination({
                    totalCount: totalCount,
                    limit: limit,
                    callback: function (curr, totalCount) {
                        pfn.getWaitingDispensing(curr, null, null, null);
                    }
                });
            });
        }
    }

    if (pagedata.submitType == 'YF') { a.successFunc(); } else { a.waitingFunc(); }
    $('#waiting_dispensing').click(a.waitingFunc);
    $('#success_dispensing').click(a.successFunc);

})
//刷新按钮
function refresh() {
    $("#searchform_waiting [type=input]").val('');
    $.loadJSON("/Customer/Dispensing/WaitingDispensing",{pageIndexSearch: 1}, function (jn) {
        if (jn.rlt) {
            
        }
    });
}