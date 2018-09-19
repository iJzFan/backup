//import { setInterval, clearInterval } from "timers";

/*===================================================收费======== START ================================*/
var CHARGE = {
    //根据url的参数初始化
    getUrlRegInit: function () {
        var reg = new RegExp("(^|&)" + "treatId" + "=([^&]*)(&|$)");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) {
            CHARGE.chargeNeedPayInfo(r[2])
        };
    },
    //设置收费二维码监听
    setPayCode: function () {
        $.post("/Charge/GetBarcodeUrl", {}, function (jn) {
            if (jn.rlt) {
                $("#test1").attr("href", jn.BarcodePayMonitorUrl);
                $("#ah-set-payCode").qrcode({
                    text: jn.BarcodePayMonitorUrl,
                    width: 150,
                    height: 150,
                });
            } else {
                $.err(jn.msg);
            }
        });
        //鼠标监听
        $(document).on("click touchstart", function (e) {
            e = window.event || e; // 兼容IE7
            obj = $(e.srcElement || e.target);
            //用户信息点击其他位置
            if ($(obj).parents(".ah-set-payCode").first().length <= 0) {
                if ($(obj).parents(".charge-set-wrap").first().length <= 0) {
                    if ($(obj).attr("class") != "charge-set-wrap") {
                        $(".ah-set-payCode").hide();
                    }
                }
            }
        });
        $("#set-pay-code").click(function () {
            $(".ah-set-payCode").toggle("fast", function () { })
        });
    },
    //回到默认页
    returnDetail: function () {
        window.location.href = "/Charge/CHIS_Charge";
    },
    //修改地址回调
    modifyAddressCallback: function (treatId, type) {
        console.log(type);
        if (type == 'pay') {
            //收费
            $.loadUrl("/Charge/CHIS_Charge?pagefn=23&treatId=" + treatId);
        } else if (type == 'dispensing') {
            //发药
            window.location.href = "/Dispensing/DispensingDetail?pagefn=105&&treatId=" + treatId;
        }
    },
    //未支付列表
    chargeNeedPayInfo: function (treatId) {
        $.post("/Charge/LoadCustomerNeedPayDetail", { treatId: treatId }, function (html) {
            $("div[ah-id='chargeIndex']").html(html);
        });
    },
    //已支付列表
    chargePayedInfo: function (payedId) {
        $.post("/Charge/LoadCustomerPayedDetail", { payedId: payedId }, function (html) {
            $("div[ah-id='chargeIndex']").html(html);
        });
    },
    //修改地址
    ChangeAddress: function (_this, customerId, selectedAddressId, type) {
        var $top = $(_this).parents('[ah-d-section="customer_address"]').first();
        var $addressId = $top.find('[ah-d-name="AddressId"]').first();
        var $details = $top.find('[ah-d-name="AddressDetails"]').first();
        var treatid = $('#TreatId').val();
        console.log(type)
        $.open({
            type: 2,
            title: '选择地址',
            area: ['50%', '715px'],
            content: ["/dispensing/ChangeAddress?customerId=" + customerId + "&selectedAddressId=" + selectedAddressId + "&treatId=" + treatid + "&type=" + type], //iframe的url，no代表不显示滚动条
            end: function () { }
        });
    },
    //收费药品详情
    getChargeDrugInfo: function (_this) {
        var state = $(_this).parent().hasClass("ah-charge-drug-info");
        $(".ah-charge-drug-info").each(function () {
            $(this).removeClass("ah-charge-drug-info");
        });
        if (!state) {
            $(_this).parent().addClass("ah-charge-drug-info");
        }
    },
    //搜索未支付
    getChargeListNeedPay: function (_this) {
        var searchText = $("#search_need_pay_input").val();
        var bAllClinic = $(_this).parents("form").first().find("[name=bAllClinic]").is(":checked");
        $.post("/Charge/GetChargeListNeedPay", { searchText: searchText, bAllClinic: bAllClinic }, function (html) {
            $("div[id='wait_payment']").html(html);
            $.updataAllLayui();
        });
    },
    //搜索已支付
    getChargeListPayed: function (_this) {
        var searchText = $("#search_pay_input").val();
        var bAllClinic = $(_this).parents("form").first().find("[name=bAllClinic]").is(":checked");
        $.post("/Charge/GetChargeListPayed", { searchText: searchText, bAllClinic: bAllClinic }, function (html) {
            $("div[id='already_payment']").html(html);
            $.updataAllLayui();
        });
    },
};
/*===================================================收费======== End ================================*/

/*===================================================支付======== START ================================*/
var PAYMENT = {
    //去支付
    Payment: function (treatid, cReorder) {
        var bReorder = cReorder.is(':checked');
        cReorder.prop("checked", false);
        $.updataAllLayui();

        var vw = document.body.clientWidth;
        var opt = {
            type: 2,
            shadeClose: false,
            closeBtn: 0,
            title: '支付<span class="ah-open-titleTips">(如若网络支付失败，请点击二维码下方对应支付的图片进行刷新)</span>',
            content: ["/Charge/ChargePayment?treatid=" + treatid + "&isReOrder=" + bReorder], //iframe的url，no代表不显示滚动条
            end: function () { }
        }
        if (vw > 1280) {
            opt.area = ['680px', '650px']
        }
        $.open(opt);
    },
    //支付成功
    PayMentSuccess: function (paymodel) {
        $.ajax({
            url: "/Charge/ChargePaymentSuccess"
            , dataType: "json"
            , data: paymodel
            , success: function (jn) {
                if (jn.rlt) {
                    $.ok("支付成功！");
                    //如果可以自动发药
                    if (jn.bAutoSendDrug) {
                        var n = 4;
                        //自动发药三次
                        var handle = setInterval(function () {
                            if (n > 0) {
                                $.get("/openapi/Dispensing/SendAllDrugsByPayOrderId", { payOrderId: jn.payOrderId }, function (jn) {
                                    if (jn.rlt) {
                                        $.ok("自动发药成功"); n = 0;
                                        CHARGE.returnDetail();
                                        clearInterval(handle);
                                    }
                                    else {
                                        if (n == 0) {
                                            clearInterval(handle);
                                            $.alertError("请手动发药，自动发药失败：" + jn.msg);
                                            CHARGE.returnDetail();
                                        }
                                    }
                                });
                            }
                            n--;
                            if (n <= 0) clearInterval(handle);
                        }, 500);
                    } else {
                        CHARGE.returnDetail();
                    }
                } else {
                    $.alertError("暂未查询到相关支付！ 支付未成功:" + jn.msg);
                }
            }
        });
    },
    //支付失败提示语
    PaymentFailTips: function () {
        $.err("如网络原因，请刷新后重新支付！");
    },
}
/*===================================================支付======== End ================================*/

/*===================================================消费明细======== START ================================*/
var CHARGELIST = {
    //获取
    LoadChargeList: function (pageIndex) {
        var searchText = $('#search').find("[name=searchText]").val();
        var dt0 = $('#search').find("[name=dt0]").val();
        var dt1 = $('#search').find("[name=dt1]").val();
        $.post("/Charge/LoadChargeList", { searchText: searchText, dt0: dt0, dt1: dt1, pageIndex: pageIndex }, function (html) {
            $('div[ah-id="CHARGELIST-TABLE"]').html(html);
        });
    },
    //查看明细详情
    ChargeListDetail: function (_this) {
        layer.open({
            type: 2,
            title: '收费明细详情',
            area: ['60%', '70%'],
            content: ["/Charge/ChargeListDetail?payId=" + $(_this).attr("ah-pid")],
        })
    }
};
/*===================================================消费明细======== End ================================*/


function setShow(bshow, id) {
    var $c = null;
    if (typeof id == "string") { $c = $(id); } else {
        $c = $(id).parents(".layui-tab-item");
    }
    if (bshow) {
        $c.removeClass("ah-pay-hidelist")
    }
    else {
        $c.addClass("ah-pay-hidelist");
    }

    $(".layui-tab-title li span").each(function () {
        if (!bshow) {
            $(this).html("<i class='glyphicon glyphicon-triangle-bottom' style='margin-left:10px'></i>")
        }
        else {
            $(this).html("");
        }
    });
}