$(function () { 
    $(document).click(function (e) {
        e = window.event || e;
        var obj = e.srcElement || e.target;
      
        if ($(obj).attr("ah-id") =="sidebarBtn") return;
        if ($(obj).parents(".ah-pv-sidebar").length == 0) {
            $(".ah-pv-sidebar").hide();
        }
    });
});

var PATIENTVISIT = {
    isSidebar: function () {
        var $this = $(".ah-pv-sidebar");
        $this.toggle(); 
    }
}

//搜索函数
function SearchPatientList(_this, type, searchForm) {
    var txt = $(_this).parents("form").first().find('input[name=searchText]').val();

    if (searchForm == 'patientHistory') {
        unfinishedPationtList(type, txt, 1, true);
    } else if (searchForm == 'haveTreatedList') {
        LoadPationtList(type, txt, 1, true, { isIncludeToday: $('#isIncludeToday').is(':checked') });//增加额外的是否包含所有的当日接诊信息
    } else {
        LoadPationtList(type, txt, 1, true);
    }
}

//候诊 在诊 已诊 一体机
function LoadPationtList(type, searchText, pageIndex, bInitialPager, opts) {
    /*
    type: 0待诊断WAITING,1在诊断TREATING，2已诊断TREATED，3一体机
    */
    var f = {
        setListNumber: function (jn) {
            $('#clinic1').html(jn.waitingCount).attr("ah-val", jn.waitingCount);
            $('#clinic2').html(jn.treatingCount).attr("ah-val", jn.treatingCount);
            $('#clinic3').html(jn.treatedCount).attr("ah-val", jn.treatedCount);
            $('#clinic4').html(jn.machineCount).attr("ah-val", jn.machineCount);
        },
        setTemplate: function (type, jn) {
            var tempname = "patientWatingTemplate";
            var contentId = "patientWatingList";
            var count = "waitingCount";
            if (type == 1) { tempname = 'patientTreatingTemplate'; contentId = 'patientTreatingList'; count ="treatingCount" }
            if (type == 2) { tempname = 'patientTreatedTemplate'; contentId = 'patientTreatedList'; count = "treatedCount"}
            if (type == 3) { tempname = 'patientMachineTemplate'; contentId = 'patientMachineList'; count = "machineCount"}
            var html = "";
             //插入生成的数据  
            if (jn != null&&jn[count]<=0) {
                html = f.returnNotDataHtml();
            } else {
                html = loadTemplateHtml(tempname, jn);
            }
            $('#' + contentId).html(html);
        },
        returnNotDataHtml: function () {
            return "<div style='display: flex;align-items: center;justify-content: center;'><img style='width: 57%;max-width: 300px;margin-top: 100px;' src='../../images/not-data2.png' /></div>";
        }
    }

    //先清空
    f.setTemplate(type, null);
    $.loadJSON("/Doctor/Json_PatientList",
        $.extend({
            type: type,
            searchText: searchText,
            pageIndex: pageIndex
        }, opts), function (jn) {
            if (jn.rlt) {
                 
                f.setListNumber(jn);
                //根据来诊类型 加载不同模板
                f.setTemplate(type, jn);
                if (bInitialPager) {
                    //设置分页
                    var pagerId = "pager_PatientWating";
                    if (type == 1) pagerId = "pager_PatientTreating";
                    if (type == 2) pagerId = "pager_PatientTreated";
                    if (type == 3) pagerId = "pager_PatientMachine";
                    $('#' + pagerId).extendPagination({
                        totalCount: jn.totalRecords,
                        limit: jn.pageSize,
                        callback: function (curr, totalCount) {
                            LoadPationtList(type, searchText, curr);
                        }
                    });
                }
            } else $.alertError(jn.msg);

        });
}

//未完成 明日约诊 明日以后
function unfinishedPationtList(type, searchText, pageIndex, bInitialPager) {
    /*    
    type=0 HISTORY 历史 /  1TOMORROW 明日/  2AFTERTOMORROW 明日之后
    */
    var f = {
        setListNumber: function () {
            $.loadJSON("/Doctor/Json_PatientHistoryOrFutureNumbers", {}, function (jn) {
                if (jn.rlt) {
                    $('#clinic5').html(jn.waitingHistoryNum);
                    $('#clinic6').html(jn.waitingTomorrowNum);
                    $('#clinic7').html(jn.waitingAfterTomorrowNum);
                }
            })
        },
        setTemplate: function (type, jn) {
            var tempname = "patientHistoryTemplate";
            var contentId = "patientHistoryList";
            if (type == 1) { tempname = 'patientTomorrowTemplate'; contentId = 'patientTomorrowList'; }
            if (type == 2) { tempname = 'patientAftertomorrowTemplate'; contentId = 'patientAftertomorrowList'; }
            //插入生成的数据            
            var html = loadTemplateHtml(tempname, jn);
            $('#' + contentId).html(html);
        }
    }
    var stringType = '';
    if (type == '0') { stringType = "HISTORY" };
    if (type == '1') { stringType = "TOMORROW" };
    if (type == '2') { stringType = "AFTERTOMORROW" };

    $.loadJSON("/Doctor/Json_PatientHistoryOrFuture",
        {
            type: stringType,
            pageIndex: pageIndex,
            searchText: searchText
        }, function (jn) {
            if (jn.rlt) {
                f.setListNumber();
                //根据来诊类型 加载不同模板
                f.setTemplate(type, jn);
                if (bInitialPager) {
                    //设置分页
                    var pagerId = "pager_PatientHistory";
                    if (type == 1) pagerId = "pager_PatientTomorrow";
                    if (type == 2) pagerId = "pager_PatientAftertomorrow";
                    $('#' + pagerId).extendPagination({
                        totalCount: jn.totalRecords,
                        limit: jn.pageSize,
                        callback: function (curr, totalCount) {
                            unfinishedPationtList(type, searchText, curr);
                        }
                    });
                }
            } else $.alertError(jn.msg);

        });


}

//取消接诊
function cancelRegist(urlpms) {
    $.confirm("取消", "取消接诊会删除此预约信息。(如以接诊，将不可删除)。是否取消？", function () {
        $.loadJSON("/doctor/CancelRegist?" + urlpms, {}, function (jn) {
            if (jn.rlt) {
                LoadPationtList(0, "", 1, true);
            } else {
                $.alertError("出错:" + jn.msg);
            }
        });
    });
}


