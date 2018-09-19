/**
 * 初始化载入
 */
function initialLoad_SelectCustomer() {
    if (pagedata.op == "NEW" || pagedata.op == "NEWF") {
         //选择会员
        $('#sel_customer').singleSelector({
            placeholder: "手机/身份证/邮箱/姓名",
            valueName: 'customerID',
            searchUrl: '/api/common/GetCustomers',
            isReadonly: pagedata.op == "VIEW",
            onSelect: function (jn) {
                loadCustomer(jn);
                //检测是否是医生
                $.post("/api/common/IsDoctor", { customerId: jn.customerID }, function (jsn) {
                    if (jsn.rlt) {
                        if (jsn.isDoctor) {
                            $.msg("该用户已经是医生了！");                       
                            $('#DoctorId').val(jsn.doctorId);
                        }
                        else loadCustomer(jn);

                    } else $.err("错误" + jn.msg);
                });

            },
            formatSearchItem: function (jn) {
                return "<li class='ah-selcus-item'><a><img src='" + pagedata.cusImgRoot + jn.photoUrlDef+"'/> <div class='ah-selcus-txt'><b>" + jn.customerName + "</b> (" + (jn.gender == 1 ? "男" : "女") + ")" + jn.age + "岁 <br /><span>" + jn.customerMobile + "</span></div></a></li>";
            }
        });
    } else { console.log("111");$('#sel_customer').parents(".ah-flex-layout").first().remove();}




    //地址
    $.set3LevelAddress({
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
            area: ['80%', '98%'],
            content: '/tools/UploadPic?posType=doctor&fileName=' + $('#DoctorInfo_DoctorPhotoUrl').val()
        });
    });

    $('.ckbox-switch2').switch2();
    //设置初始样式
    $('.status3').status3();
    // $.updataLayui();

    $("select").each(function () {
        if ($(this).attr("role-initial") == "false") return;
        var b = $(this).parent().hasClass("status3");
        if (b) return;
        if (typeof ($(this).attr("readonly")) != "undefined") { $(this).attr("disabled", "disabled"); } //添加一个disabled

    });
}
function initialLoad_DoctorBase() {

    //地址
    $.set3LevelAddress({
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
            area: ['80%', '98%'],
            content: '/tools/UploadPic?posType=doctor&fileName=' + $('#DoctorInfo_DoctorPhotoUrl').val()
        });
    });

    $('.ckbox-switch2').switch2();
    //设置初始样式
    $('.status3').status3();
    // $.updataLayui();

    $("select").each(function () {
        if ($(this).attr("role-initial") == "false") return;
        var b = $(this).parent().hasClass("status3");
        if (b) return;
        if (typeof ($(this).attr("readonly")) != "undefined") { $(this).attr("disabled", "disabled"); } //添加一个disabled

    });
}

/**
 * 初始化载入
 */
function initialLoad() {
     
    //地址
    //$.set3LevelAddress({
    //    $province: $('#province'),
    //    $city: $('#citys'),
    //    $county: $('#county'),
    //    $val: $('#areaId')
    //});
    //图片修改
    $('#EditUserPic').click(function () {
        var handle = layer.open({
            type: 2,
            title: "上传图片",
            area: ['80%', '98%'],
            content: '/tools/UploadPic?posType=doctor&fileName=' + $('#DoctorInfo_DoctorPhotoUrl').val()
        });
    });

    $('.ckbox-switch2').switch2();
    //设置初始样式
    $('.status3').status3();
    // $.updataLayui();

    $("select").each(function () {
        if ($(this).attr("role-initial") == "false") return;
        var b = $(this).parent().hasClass("status3");
        if (b) return;
        if (typeof ($(this).attr("readonly")) != "undefined") { $(this).attr("disabled", "disabled"); } //添加一个disabled

    });
    $("div[ah-id='ROLEANDDEP']").on("click","a",function () {
        var _this = $(this);
        $.confirm("删除", "是否继续删除？", function () {
            var data = {
                doctorId: pagedata.doctorId,
                stationId: _this.parents(".ah-flex-layout").first().attr("ah-stationId")
            };
            var type = _this.attr("ah-type");
            var url = "/Code/DoctorSet_DeleteStation";
            if (type == "role") {
                data.roleId = _this.attr("ah-id");
                url = "/Code/DoctorSet_DeleteRole";
            } else if (type == "departs") {
                data.departId = _this.attr("ah-id");
                url = "/Code/DoctorSet_DeleteDepart";
            };
            $.post(url, data, function (html) {
                $("div[ah-id='ROLEANDDEP']").html(html);
            });
        });
    });
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





function loadCustomer(jn) {
    console.log(jn);
    $('#sel_customer').find('.customer-name').text(jn.customerName);
    //选中会员代入信息
    $('#customerId').val(jn.customerID);
    $('#DoctorName').text($.str(jn.customerName));
    $('#Birthday').text(formatDate(jn.birthday));
    $('#CustomerMobile').text($.str(jn.customerMobile));
    $('#IDcard').text($.str(jn.iDcard));
    $('#Email').text($.str(jn.email));
    $('#Gender').val(jn.gender); 
    $('#Marriage').val(jn.marriage);
    $('#EduLevel').val(jn.eduLevel);
    $('#cus_addr_pro').text($.str(jn.mergerName));
    $('#cus_addr_detail').text($.str(jn.address));
}

function setPicName(fileName) {
    $('#DoctorInfo_DoctorPhotoUrl').val(fileName);
    var path = pagedata.docImgRoot + fileName;
    var val = "url('" + path + "')";
    $('#cusImg').css("background-image", val);
}

 
//新增用户
function createCustomer() {
    $.openAdd({
        title: '添加会员',
        content: "/Code/CustomerEdit?op=NEWF",
        sendSuccessTrue: function (jn, index) {
            loadCustomer(jn.item);//载入信息
            top.layer.close(index);//关闭窗口
            $.ok("添加会员成功！");
        }
    });
}



