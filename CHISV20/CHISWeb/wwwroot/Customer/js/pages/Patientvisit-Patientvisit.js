﻿$(function () {

    //待诊
    LoadPationtList(0, "", 1, true);
    $("#patientWating").click(function () {
        LoadPationtList(0, "", 1, true);
    });

    //在诊
    $("#patientTreating").click(function () {
        LoadPationtList(1, "", 1, true);
    });
    //已诊
    $("#patientTreated").click(function () {
        LoadPationtList(2, "", 1, true);
    });
    //一体机
    $("#patientMachine").click(function () {
        LoadPationtList(3, "", 1, true);
    });

    //未完成接诊
    unfinishedPationtList(0, "", 1, true);
    $("#patientHistory").click(function () {
        unfinishedPationtList(0, "", 1, true);
    });

    //明日接诊
    $("#patientTomorrow").click(function () {
        unfinishedPationtList(1, "", 1, true);
    });

    //明日以后接诊
    $("#patientAftertomorrow").click(function () {
        unfinishedPationtList(2, "", 1, true);
    });
});

//搜索函数

function SearchPatientList(_this, type, searchForm) {
    var txt = $(_this).parents("form").first().find('input[name=searchText]').val();

    if (searchForm == 'patientHistory') {
        unfinishedPationtList(type, txt, 1, true);
    } else {
        LoadPationtList(type, txt, 1, true);
    }
}

//候诊 在诊 已诊 一体机
function LoadPationtList(type, searchText, pageIndex, bInitialPager) {
    /*
    type: 0待诊断WAITING,1在诊断TREATING，2已诊断TREATED，3一体机
    */
    var f = {
        setListNumber: function (jn) {
            $('#clinic1').html(jn.waitingCount);
            $('#clinic2').html(jn.treatingCount);
            $('#clinic3').html(jn.treatedCount);
            $('#clinic4').html(jn.machineCount);
        },
        setTemplate: function (type, jn) {
            var tempname = "patientWatingTemplate";
            var contentId = "patientWatingList";
            if (type == 1) { tempname = 'patientTreatingTemplate'; contentId = 'patientTreatingList'; }
            if (type == 2) { tempname = 'patientTreatedTemplate'; contentId = 'patientTreatedList'; }
            if (type == 3) { tempname = 'patientMachineTemplate'; contentId = 'patientMachineList'; }
            //插入生成的数据            
            var html = loadTemplateHtml(tempname, jn);
            $('#' + contentId).html(html);
        }
    }

    //先清空
    f.setTemplate(type, null);
    $.loadJSON("/Customer/Patientvisit/Json_PatientList",
        {
            type: type,
            searchText: searchText,
            pageIndex: pageIndex
        }, function (jn) {
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
            $.loadJSON("/Customer/Patientvisit/Json_PatientHistoryOrFutureNumbers", {}, function (jn) {
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

    $.loadJSON("/Customer/Patientvisit/Json_PatientHistoryOrFuture",
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
                            unfinishedPationtList(type,searchText, curr);
                        }
                    });
                }
            } else $.alertError(jn.msg);

        });


}