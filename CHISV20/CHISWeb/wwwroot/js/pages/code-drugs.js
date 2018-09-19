/**
 * 初始化载入
 */
function initialLoad() {
    if (pagedata.status == "REJECT") {
        $('input[ah-id="rejectReson"]').show();
    }
    $('select[ah-id="drugExamineStatus"]').change(function () {
        if ($(this).val() == "REJECT") {
            $('input[ah-id="rejectReson"]').show();
        } else {
            $('input[ah-id="rejectReson"]').hide();
        }
    });

    //上传药品图片
    $('#EditDrugImg').click(function () {
        var handle = layer.open({
            type: 2,
            title: "上传图片",
            area: ['60%', '98%'],
            content: '/tools/UploadPic?posType=drug&sourceId=drugimg&fileName=' + $('#CHIS_Code_Drug_Main_DrugPicUrl').val()
        });
    });


    switch (pagedata.typeModel) {
        case '1':
            break;
        case '2':
            $("#DrugStockTypeId").val(3).attr("disabled", "disabled");
            $("#MedialMainKindCode").val("ZYM").attr("disabled", "disabled");
            $("#select_USId").val(42);
            $("#select_DosageContent").val(1);
            $("#select_DosageUnitId").val(42);
            break;
        case '3':
            $("#DrugStockTypeId").val(20).attr("disabled", "disabled");
            $("#MedialMainKindCode").val("MT").attr("disabled", "disabled");
            $("#select_DosageUnitId").val(26);
            $("#select_DosageContent").val(1);
            break;
        case '4':
            $("#DrugStockTypeId").val(12994).attr("disabled", "disabled");
            $("#MedialMainKindCode").val("CZ").attr("disabled", "disabled");
            $("#select_USId").val(12995);
            $("#select_DosageUnitId").val(12995);
            $("#select_DosageContent").val(1);
            break;
        default:
            break;
    }

}


/* =============== 发送提交请求 start==========================*/
var sendProc = {};
function submitForm(opt) {
    sendProc.sendSuccess = opt.sendSuccess;
    sendProc.sendFailed = opt.sendFailed;
    $('#form1').submit();
}
function sendFormSuccess(jn) { if (sendProc.sendSuccess) sendProc.sendSuccess(jn); }
function sendFormFailed() { if (sendProc.sendFailed) sendProc.sendFailed(jn); }

//增加药品申请
function ApplyDrug(opt) {
    sendProc.sendSuccess = opt.sendSuccess;
    sendProc.sendFailed = opt.sendFailed;
    var url = "/Code/ApplyDrug?op=" + pagedata.op;
    $('#form1').attr("action", url).submit();
}

/* =============== 发送提交请求 end==========================*/



 
//保存审批
function PendingDrug() {
    var status = $('select[ah-id="drugExamineStatus"]').val();
    var rejectReson = $('input[ah-id="rejectReson"]').val();
    $.post("/Code/PendingDrug", {
        drugId: pagedata.drugId, status: status, rejectReson: rejectReson
}, function (jn) {
    if (jn.rlt) {
        $('#CHIS_Code_Drug_Main_IsEnable').val(status == "ALLOWED" ? "True" : "False");
    }
    $.msg(jn.message);
});

}

//保存记录
function submitForm() { 
    sendProc.sendSuccess = function (jn) {
        if (jn.rlt) {
            top.window.$("#gridList").trigger("reloadGrid");
            $.ok("操作成功");
            top.$('.layui-layer-shade').remove();
            top.$('.layui-layer').remove();
        } else $.err("操作错误！" + jn.msg);
    };
    $('#form1').submit();
}





//上传图片回调
function setPicName(fileName, sourceId) {
    if (sourceId == "drugimg") {
        $('#drugImg').attr("src", pagedata.drugRoot + fileName);
        $('#CHIS_Code_Drug_Main_DrugPicUrl').val(fileName);
    }
}

//含量调整
function dosageChange(_this) {
    var txt = $(_this).find("option:selected").text();
    $('#def_dosage').text(txt);
}







 