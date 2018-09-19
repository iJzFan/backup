var ORDER = {
    //判断是否处于微信端
    isWeChat: function () {
        var ua = window.navigator.userAgent.toLowerCase();
        if (ua.match(/MicroMessenger/i) == 'micromessenger') {
            return true;
        } else {
            return false;
        }
    },
    //获取用户需要支付的清单
    GetCustomerNeedPayList: function () {
        $.loadJSON("http://chis.jk213.com/Charge/GetCustomerNeedPayList", {
            customerId: op.customerId,
        }, function (jn) {
            if (jn.rlt) {
                console.log(jn.items)
                //同一个请求遍历不同端的页面
                ORDER.InitCustomerNeedPayList_APP(jn.items);
                ORDER.InitCustomerNeedPayList_PC(jn.items);
            }
        });
    },
    //获取客户已经支付的信息 默认7天内
    GetCustomerPayedList: function (pageIndex) {
        $.loadJSON("http://chis.jk213.com/Charge/GetCustomerPayedList", {
            customerId: op.customerId,
            pageSize: 20,
            pageIndex: pageIndex || 1,
            dt0: (new Date()).addDays(-365).formatDate(),
            dt1: new Date().formatDate(),
        }, function (jn) {
            if (jn.rlt) {
                console.log(jn.items)
                //同一个请求遍历不同端的页面
                ORDER.InitCustomerPayedList_APP(jn.items, jn.pageIndex);
                ORDER.InitCustomerPayedList_PC(jn.items, jn.pageIndex);
            }
        });
    },
    //app端用户完成支付的清单遍历
    InitCustomerPayedList_APP: function (items, pageIndex) {
        var wrap = $('ul[ah-id="payed_order_app"]');
        if (items.length > 0) {
            if (pageIndex == 1) wrap.html("");
            $.each(items, function (key, value) {
                var html = '<li>' +
                    '<div class="li-top">' +
                    '<div class="order-main">' +
                    '<span class="order-title">' + value.stationName + '</span>' +
                    '<span class="order-text">医生名称：' + value.doctorName + '</span>' +
                    '</div>' +
                    '<span class="price">' + toPriceStr(value.totalAmount) + '</span>' +
                    '</div>' +
                    '<div class="li-bottom" ah-type="Payed" ah-payId="' + value.payId + '">' +
                    '<span class="order-time">' + value.treatTime + '</span>' +
                    '<span class="order-count">' +
                    '详情 <i class="glyphicon glyphicon-chevron-down"></i>' +
                    '</span>' +
                    '</div>' +
                    '<div class="order-info-wrap" ah-status="false"></div>' +
                    '</li>';
                wrap.append(html);
            });
        }

        wrap.find("#pg_paged_more").remove();
        wrap.append('<li id="pg_paged_more" class="ah-pg-li-more"><a onclick="ORDER.GetCustomerPayedList(' + (pageIndex + 1) + ')">更多...</a></li>');
    },
    //pc端用户完成支付的清单遍历
    InitCustomerPayedList_PC: function (items, pageIndex) {
        var wrap = $('tbody[ah-id="pay-pc-table"]');
        wrap.html("");
        $.each(items, function (key, value) {
            var html = '<tr>' +
                '<td>' + value.stationName + '</td>' +
                '<td>' + value.doctorName + '</td>' +
                '<td>' + value.treatTime + '</td>' +
                '<td style="text-align:right;"><span class="order-price">' + toPriceStr(value.totalAmount) + '</span></td>' +
                '<td><a class="table-order-more" ah-type="Payed" onclick= "ORDER.Order_More_Pc(' + value.payId + ',this)" >详情</a></td>' +
                '</tr>' +
                '<tr class="table-order-info" ah-status="false"><td colspan="5"></td></tr>';
            wrap.append(html);
        });
        wrap.append('<tr><td colspan="5"><a class="pre-page pn-' + pageIndex + '" onclick="ORDER.GetCustomerPayedList(' + (pageIndex - 1) + ')">上一页</a> <a class="next-page" onclick="ORDER.GetCustomerPayedList(' + (pageIndex + 1) + ')">下一页</a> </td></tr>');
    },
    //app端用户需要支付的清单遍历
    InitCustomerNeedPayList_APP: function (items) {
        var wrap = $('ul[ah-id="need_order_app"]');
        if (items.length > 0) {
            wrap.html("");
            $.each(items, function (key, value) {
                var html = '<li>' +
                    '<div class="li-top">' +
                    '<div class="order-main">' +
                    '<span class="order-title">' + value.stationName + '</span>' +
                    '<span class="order-text">医生名称：' + value.doctorName + '</span>' +
                    '</div>' +
                    '<span class="order-pay" onclick="ORDER.OrderPay(' + value.treatId + ',' + value.needPayAmount + ')">支付 ￥' + value.needPayAmount + '</span>' +
                    '</div>' +
                    '<div class="li-bottom" ah-type="NeedPay" ah-treatId="' + value.treatId + '">' +
                    '<span class="order-time">' + value.treatTime + '</span>' +
                    '<span class="order-count">' +
                    '<i class="glyphicon glyphicon-chevron-down"></i>详情' +
                    '</span>' +
                    '</div>' +
                    '<div class="order-info-wrap" ah-status="false"></div>' +
                    '</li>';
                wrap.append(html);
            });
        }
    },
    //pc端用户需要支付的清单遍历
    InitCustomerNeedPayList_PC: function (items) {
        var wrap = $('tbody[ah-id="need-pc-table"]');
        wrap.html("");
        $.each(items, function (key, value) {
            var html = '<tr>' +
                '<td>' + value.stationName + '</td>' +
                '<td>' + value.doctorName + '</td>' +
                '<td>' + value.treatTime + '</td>' +
                '<td><span class="order-price">' + value.needPayAmount + '</span></td>' +
                '<td><a class="order-pay-btn" onclick="ORDER.OrderPay_Pc()">支付</a>' +
                '<a class="table-order-more" ah-type="NeedPay" onclick="ORDER.Order_More_Pc(' + value.treatId + ',this)">详情</a></td>' +
                '</tr>' +
                '<tr class="table-order-info" ah-status="false"><td colspan="5"></td></tr>';
            wrap.append(html);
        });
    },
    //查看订单详情监听
    OnOrderInfoClick: function () {
        //APP 详情监听
        $(".order-app-ul").on("click", ".li-bottom", function (e) {
            var i = $(e.currentTarget).find("i");
            var info = $(e.currentTarget).next();
            if (i.attr("class") == "glyphicon glyphicon-chevron-down") {
                i.attr("class", "glyphicon glyphicon-chevron-up");
                //根据不同类型调用不同接口
                if ($(e.currentTarget).attr("ah-type") == "NeedPay") {
                    ORDER.GetTreatNeedPayInfo($(e.currentTarget).attr("ah-treatId"), info, "app");
                } else {
                    ORDER.GetCustomerPayedDetail($(e.currentTarget).attr("ah-payId"), info, "app");
                }
            } else {
                i.attr("class", "glyphicon glyphicon-chevron-down");
                info.fadeOut();
            }
        });
    },
    //查看已支付订单详情
    GetCustomerPayedDetail: function (payId, info, type) {
        if (info.attr("ah-status") == "false") {
            $.loadJSON("http://chis.jk213.com/Charge/GetCustomerPayedDetail", {
                payId: payId,
            }, function (jn) {
                if (jn.rlt) {
                    info.attr("ah-status", "true");
                    var html = $("<ul class='order-info'></ul>");
                    $.each(jn.items, function (key, value) {
                        html.append("<li><span>" + value.content + "</span><span class='order-info-price'>" + toPriceStr(value.amount) + "</span></li>");
                    });
                    if (type == "pc") {
                        info.find("td").html(html);
                    } else {
                        info.html(html);
                    }
                    info.fadeIn();
                }
            });
        } else {
            info.fadeIn();
        }
    },
    //查看待支付订单详情
    GetTreatNeedPayInfo: function (treatId, info, type) {
        if (info.attr("ah-status") == "false") {
            $.loadJSON("http://chis.jk213.com/Charge/GetTreatNeedPayInfo", {
                treatId: treatId,
            }, function (jn) {
                if (jn.rlt) {
                    info.attr("ah-status", "true");
                    var html = $("<ul class='order-info'></ul>");
                    $.each(jn.items, function (key, value) {
                        html.append("<li><span>" + value.content + "</span><span class='order-info-price'>" + value.amount + "</span></li>");
                    });
                    if (type == "pc") {
                        info.find("td").html(html);
                    } else {
                        info.html(html);
                    }

                    info.fadeIn();

                }
            });
        } else {
            info.fadeIn();
        }
    },
    //调用微信支付（需要先判断是否在微信端上，浏览器响应式也会触发）
    OrderPay: function (treatId, totalAmount) {
        if (!op.isWechat) {
            layer.msg("请到微信支付!");
            return;
        }
        $.loadJSON("/Customer/Home/CreateWXPubPay", {
            treatId: treatId,
            totalAmount: totalAmount,
        }, function (jn) {
            if (jn.rlt) {
                function onBridgeReady() {
                    WeixinJSBridge.invoke(
                        'getBrandWCPayRequest', {
                            "appId": jn.appId,     //公众号名称，由商户传入     
                            "timeStamp": jn.timeStamp,         //时间戳，自1970年以来的秒数     
                            "nonceStr": jn.nonceStr, //随机串     
                            "package": "prepay_id=" + jn.prepay_id,
                            "signType": jn.signType,         //微信签名方式：     
                            "paySign": jn.paySign //微信签名 
                        },
                        function (res) {
                            if (res.err_msg == "get_brand_wcpay_request:ok") {
                                //确认订单是否支付成功
                                ORDER.WXPay_Confirm(jn.out_trade_no);
                            }
                        }
                    );
                }
                if (typeof WeixinJSBridge == "undefined") {
                    if (document.addEventListener) {
                        document.addEventListener('WeixinJSBridgeReady', onBridgeReady, false);
                    } else if (document.attachEvent) {
                        document.attachEvent('WeixinJSBridgeReady', onBridgeReady);
                        document.attachEvent('onWeixinJSBridgeReady', onBridgeReady);
                    }
                } else {
                    onBridgeReady();
                }
            }
            else layer.msg("错误:" + jn.msg);
        });
    },
    //生成PC支付二维码
    OrderPay_Pc: function () {
        layer.msg("生成PC支付二维码");
    },
    //查看PC端订单详情
    Order_More_Pc: function (id, _this) {
        var info = $(_this).parents("tr").next();
        //判断是否处于隐藏状态
        if (info.is(":hidden")) {
            //根据不同类型调用不同接口
            if ($(_this).attr("ah-type") == "NeedPay") {
                ORDER.GetTreatNeedPayInfo(id, info, "pc");
            } else {
                ORDER.GetCustomerPayedDetail(id, info, "pc");
            }
        } else {
            info.fadeOut();
        }
    },
    //确认微信支付是否成功（微信端）
    WXPay_Confirm: function (payOrderId) {
        $.loadJSON("/Customer/Home/CheckWXPubPayStatusByWX", {
            payOrderId: payOrderId
        }, function (jn) {
            if (jn.rlt) {
                layer.msg("支付成功");
                //重新更新列表
                ORDER.GetCustomerNeedPayList();
                ORDER.GetCustomerPayedList();
            } else {
                layer.msg("支付失败，请重试。" + jn.msg);
            }
        });
    },

}