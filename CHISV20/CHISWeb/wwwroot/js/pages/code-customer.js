/**
 * 初始化载入
 */
function initialLoad() {
    //地址
    $.layui3LevelAddress({
        $province: $('#province'),
        $city: $('#citys'),
        $county: $('#county'),
        $val: $('#areaId')
    });
    //图片修改
    $('#EditUserPic').click(function () {
        var handle = layer.open({
            type: 2,
            title: "上传图片",
            area: ['60%', '98%'],
            content: '/tools/UploadPic?posType=customer&fileName=' + $('#Customer_CustomerPic').val()
        });
    });

    $('.ckbox-switch2').switch2();
    //设置初始样式
    $('.status3').status3();
   // $.updataLayui();
}


/* =============== 发送提交请求 start==========================*/
var sendProc = {};
function submitForm(opt) {
    sendProc.sendSuccess = opt.sendSuccess;
    sendProc.sendFailed = opt.sendFailed || function () { $.err("传送数据错误！"); };
    $('#form1').submit();
}
function sendCustomerSuccess(jn) { if (sendProc.sendSuccess) sendProc.sendSuccess(jn); }
function sendCustomerFailed() { if (sendProc.sendFailed) sendProc.sendFailed(jn); }
/* =============== 发送提交请求 end==========================*/





function setLoginName(id) {
    $.post("/Customer/GetDefCustomerLoginName",
        {
            name: $('#Customer_CustomerName').val(),
            gender: $('#Customer_Gender').val(),
            birthday: $('#Customer_Birthday').val()
        }, function (jn) {
            if (jn.rlt) $(id).val(jn.loginName);
            else $.err("获取错误" + jn.msg);
        });

}

function setPicName(fileName) {
    $('#Customer_CustomerPic').val(fileName);
     
    var path = pagedata.cusRoot + fileName;
    
    var val = "url('" + path + "')";
    
    $('#cusImg').css("background-image", val);
}

//检查同名客户，并提示
function checkTheSameCustomer(c, type) {
    var s = $(c).val();
    if (s.length > 6) {
        $.loadJSON("/Api/Common/GetCustomersMasked", { searchtext: s }, function (jn) {

            var bnewCheck = jn && (pagedata.op == "NEW" && jn.length > 0);//新增的时候发现了数据
            //修改的时候发现了多余数据，或者发现了2条及以上修改的数据，或者出现的关键项数据的主键不一致。
            var bmodCheck = jn && (pagedata.op == "MODIFY" && (jn.length > 1 || (jn.length == 1 && jn[0].customerId != pagedata.customerId)));

            if (jn && (bnewCheck || bmodCheck)) {
                var bg = $("<div></div>");
                var h = $("<div class='sameCusBg'></div>");
                bg.append(h);
                h.append("<h5>出现多个用户具有相同信息，继续将会要求验证后清理这些数据信息。</h5>")
                var $tb = $("<table class='table'></table>");
                $tb.append("<tr><th>姓名</th><th>性别</th><th>年龄</th><th>手机</th><th>邮箱</th><th>身份证号</th><th>其他联系号</th><th></th></tr>");
                $.each(jn, function (i, m) {
                    var $tr = $("<tr></tr>");
                    $tr.append("<td>" + m.customerName + "</td>");
                    $tr.append("<td>" + m.gender + "</td>");
                    $tr.append("<td>" + m.age + "</td>");
                    $tr.append("<td>" + m.customerMobile + "</td>");
                    $tr.append("<td>" + m.email + "</td>");
                    $tr.append("<td>" + m.iDcard + "</td>");
                    $tr.append("<td>" + m.telephone + "</td>");
                    $tr.append("<td><a class='clear-cus' data-cusid='{0}' onclick=\"clearCusInfo({0},'{1}','{2}');\">清除</a></td>".format(m.customerId, type, s));
                    $tb.append($tr);//添加一行
                });
                h.append($tb);



                //if (type != "idcard") {
                //    $va = $("<a class='btn btn-primary'>发送验证码</a>");
                //    $va.on("click", function () {
                //        var lk = "", d = {};
                //        if (type == "mobile") { lk = "/api/vcode/SendMobileNewVCode"; d.newMobile = s; }
                //        if (type == "email") { lk = "/api/vcode/SendNewEmailVCode"; d.newEmail = s;}

                //    });
                //    $vc = $("<div><input class='form-control' type='text' name='svcode' /></div>").append($va);
                //    h.append($vc);
                //}


                layer.open({
                    type: 1,
                    area: ['700px', '530px'],
                    title: "发现重复用户",
                    shadeClose: true,
                    // btn: ['清除并继续', '取消'],
                    content: bg.html(),
                    //yes: function (index, layero) {
                    //    //var vcode = $(layero).find("[name=svcode]").val();
                    //    var vcode = "";
                    //    $.loadJSON("/api/Common/ResetOtherCustomerInfo", { mobileOrEmailOrIdCard: s, type: type, vcode: vcode, customerId: $('Customer_CustomerID').val() }, function (jn) {
                    //        if (jn.rlt) $.alertMsg("清除其他成员的数据成功");
                    //        else $.alertError(jn.msg);
                    //    });
                    //}
                });
            }
            else {
                $(c).after("<span class='can-use'>可用</span>");
            }
        });
    }
}
function clearCusInfo(customerId, type, mo) {
    var dd = { customerId: customerId, type: type, txt: mo }
    $.loadJSON("/api/Common/ClearCustomerInfo", dd, function (jn) {
        if (jn.rlt) {
            $.alertMsg("操作成功");
            $("a[data-cusid=" + dd.customerId + "]").parents("tr").first().remove();
        }
        else $.alertError(jn.msg);
    });

}