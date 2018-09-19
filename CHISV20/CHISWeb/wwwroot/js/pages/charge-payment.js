$(function () {
    var wxqrurl = $("input[ah-code='WxQrcodeString']").val();
    var aliqrurl = $("input[ah-code='AliQrcodeString']").val();
    var pay = $('input[ah-id="pay"]');
    var use_cash = $("#use_cash");
    showPayQr("WX", wxqrurl);
    showPayQr("ALI", aliqrurl);
    var keyInputProc = function (target) {
        if (!$(target).is(":visible")) return;
        var count = parseFloat($(target).val() - pagedata.Amount).toFixed(2);
        var html = "";
        if (!($(target).val())) {
            html += "<span style='color:#ccc;'>尚未输入收款金额</span>";
            $('#pay_btn_success').prop("disabled", true);
            pagedata.subStates = false;
        }
        else if (count < 0) {
            html += "<span>不足</span>";            
            $('#pay_btn_success').prop("disabled", true);
            pagedata.subStates = false;
        } else if (count > 0) {
            html += "<span id='cash_return' style='color:#037e23'>" + count + "</span>";
            $('#pay_btn_success').prop("disabled", false);
            pagedata.subStates = true;
        } else {
            html = "";
            $('#pay_btn_success').prop("disabled", false);
            pagedata.subStates = true;
        }
        $('div[ah-id="pay-change"]').html(html);
    }
    pay.bind('input propertychange', function (event) { keyInputProc(event.target); });
    keyInputProc(document.getElementById("pay_input"));
    $('#pay_btn_success').prop("disabled", false);

    $('input[ah-id="pay"]').keydown(function (e) {
        if (e.keyCode == 13) {
            if (pagedata.subStates) {
                PayMentSuccess();
            } else {
                $.err("请核对费用后再完成支付!");
            }
        }
    }).on("focus", function () {
        $(this).select();
        use_cash.prop("checked", true);
        window.layui.form.render();
    });

    //绑定选择事件
    $("#use_cash").on("change", function () {
        if ($(this).is(":checked")) {
            $(this).parents(".ah-pay-xj").first().addClass("ah-pay-xj-select");
        } else {
            $(this).parents(".ah-pay-xj").first().removeClass("ah-pay-xj-select");
        }
    });
    
});
//支付成功 如果是现金，则直接操作，其他则侦测后端
function PayMentSuccess() {
    var payOrderId = $('input[ah-rid="PayOrderId"]').first().val();
    var paymodel = {
        PayOrderId: payOrderId,
        payRemark: "",
        IsCash: $('.ah-pay-xj:visible').find("#use_cash").is(":checked"),
        GetCashAmount: parseInt(parseFloat($('#pay_input').val()) * 100.0001),
        ReturnCashAmount:parseInt(parseFloat($('#cash_return').text()) * 100.0001),
        PayAmount:parseInt(parseFloat($('#cash_amount').text()) * 100.0001)
    };
    console.log(paymodel);
    parent.PAYMENT.PayMentSuccess(paymodel);

}
//支付失败
function PayMentFail() {
    var payOrderId = $('input[ah-rid="PayOrderId"]').first().val();
    $.post("/Charge/ChargePaymentCancel?payOrderId=" + payOrderId);//通知后端该笔订单付款取消或者失败
    parent.PAYMENT.PaymentFailTips();
    var index = parent.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
    parent.layer.close(index); //再执行关闭
}

function refresh2D(type, treatId) {
    var rmk = "";
    var url = "/Charge/GetWxPay2DCode";//WX
    if (type == "ALI") url = "/Charge/GetAliPay2DCode";
    $.post(url, { treatId: treatId, payRemark: rmk }, function (jn) {
        //重新生成二维码
        if (jn.rlt) {
            setQrPms(type, jn);
            showPayQr(type, jn.code_url);
        } else { $.err("出错:" + jn.msg); }
    });
}
function showPayQr(type, url) {
    var c = type == "ALI" ? "#payCode_zfb" : "#payCode_wechat";
    console.log(c);
    url && $(c).empty().qrcode({
        text: url,
        width: 150,
        height: 150,
    });
}
function setQrPms(type, jn) {
    if (type == "WX") {
        $('input[ah-rid="PayOrderId"]').val(jn.payOrderId);
        $('input[ah-code="WxQrcodeString"]').val(jn.code_url);
    }
    if (type == "ALI") {
        $('input[ah-rid="PayOrderId"]').val(jn.payOrderId);
        $('input[ah-code="AliQrcodeString"]').val(jn.code_url);
    }
}