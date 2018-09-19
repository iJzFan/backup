
$(function () {
    $(".ah-drugNurse-register-items .ah-doctor-main .ah-doctor-bj").click(function () {
        var _this = $(this);
        _this.addClass("layui-anim-fadein")
        var depId = _this.attr("data-defdepartmentid");
        var doctorId = _this.attr("data-doctorid");
        var rxdoctorId = _this.attr("data-rxdoctorid");
        $("#DepartmentId").val(depId);
        $("#DoctorId").val(doctorId);
        $('#RxDoctorId').val(rxdoctorId);
      
        $.updataAllLayui();
        setTimeout(function () {
            _this.removeClass("layui-anim-fadein");
        }, 500);
    });

    doctorItemLoad(pagedata.departId);
    //回车监听
    $("#ah-register-phone").keypress(function (e) {
        if (e.which == 13) {
            getCustomersBy();
            $("#register_source_search").blur();
        }
    });
    //手机号码失去焦点监听
    $("#ah-register-phone").blur(function (e) {
        getCustomersBy();
    });
    //重置监听
    $("#Reset").click(function () {
        $("#Customer_Id").val("");
        $("#ah-register-phone").val("");
        $("#Customer_Name").val("");
        $("input[name='sex'][value='1']").attr("checked", true);
        $("#Customer_Birthday").val(new XDate().toString("yyyy-MM-dd"));
        dateChangeRange($("#Customer_Birthday"));
        $.updataAllLayui();
    });
    //快速接诊监听
    $("#fastPatient").click(function () {
        var op = {
            customerID: $("#Customer_Id").val(),
            stationName: pagedata.stationName,
            departmentID: $("#DepartmentId").find("option:selected").val(),
            doctorId: $("#DoctorId").find("option:selected").val(),
            reservationDate: new XDate().toString("yyyy-MM-dd"),
            reservationSlot: 1,
            opId: $("#register-opId").val(),
            opMan: $("#register_source").val(),
            rxDoctorId:$('#RxDoctorId').val()
        }
        if (op.customerID) {
            var depName = $("#DepartmentId").find("option:selected").text();
            var doctorName = $("#DoctorId").find("option:selected").text();
            $.confirm("快速接诊", "是否继续预约-" + doctorName + "医生 (" + depName + ") ?", function () {
                //根据当前时间判断班次
                if (new XDate()[0].getHours() >= 12) {
                    op.reservationSlot = 2;
                }
                $.getJSON("/Reservation/Json_GetReservationInfo", op, function (jn) {
                    if (jn.rlt) {
                        if (jn.rltCode == "REPET_REGIST") {
                            $.err("请注意,您已经预约了,不能重复预约！");
                        } else {
                            if (op.doctorId == pagedata.doctorId) {
                                $.ok("预约成功，跳转中请稍后！");
                                window.location.href = '/Doctor/PatientDetail?customerId=' + jn.customer.customerID + '&registId=' + jn.registerId + '&treatId=null';
                            } else {
                                $.ok("已成功为您预约 - " + doctorName + "医生 (" + depName + ")");
                            }
                        }
                    } else {
                        $.err(jn.msg);
                    }
                });
            })
        } else {
            //如果用户不存在则注册
            fastRegister();
        }


    });
    //选择关联人监听
    $(".ah-doctorIndex-register-main").on("click", ".ah-my-relationships div", function () {
        var _this = $(this);
        activeCustomer({
            customerID: _this.attr("ah-cid"),
            customerName: _this.attr("ah-customerName"),
            gender: _this.attr("ah-gender"),
            birthday: _this.attr("ah-birthday"),
            customerMobile: _this.attr("ah-customerMobile")
        })
    })
    //不选择关联人选择自己监听
    $(".ah-doctorIndex-register-main").on("click", ".ah-doctorIndex-register-user", function () {
        var _this = $(this);
        activeCustomer({
            customerID: _this.attr("ah-cid"),
            customerName: _this.attr("ah-customerName"),
            gender: _this.attr("ah-gender"),
            birthday: _this.attr("ah-birthday"),
            customerMobile: _this.attr("ah-customerMobile")
        })
    })
})
//选择约号人员赋值
function activeCustomer(data) {
    $("#ah-register-phone").val(data.customerMobile);
    $("#Customer_Id").val(data.customerID);
    $("#Customer_Name").val(data.customerName);
    $("input[name='sex']").each(function () {
        var _this = $(this);
        if (_this.val() == parseInt(data.gender)) {
            _this.prop("checked", true);
        } else {
            _this.removeProp("checked");
        }
    });
    $("#Customer_Birthday").val(data.birthday.substring(0, 10));
    dateChangeRange($("#Customer_Birthday"));
    $.updataAllLayui();
}
//快速注册
function fastRegister() {
    var model = {
        customerName: $("#Customer_Name").val(),
        customerMobile: $("#ah-register-phone").val(),
        gender: $("input[name='sex']:checked").val(),
        birthday: $("#Customer_Birthday").val()
    }
    $.postJson("/openapi/HealthorInfo/CustomerQuickRegist", model, function (jn) {
        if (jn.rlt) {
            $("#Customer_Id").val(jn.customerId);
            $("#fastPatient").click();
        } else {
            $.err(jn.msg)
        }
    })
}
function doctorItemLoad(departId) {
    $.get("/openapi/Common/JetDoctorsInDepart?departId=" + departId, function (jn) {
        $("select[ah-select='DoctorIndexItems']").html($.addSelectOption("请选择医生", jn.items));
        if (layui.form) {
            $.updataLayui();
        }
    });
}
//根据基础信息获取用户信息
function getCustomersBy() {
    $.getJSON("/api/Customer/GetCustomersAndRelations", {
        searchText: $("#ah-register-phone").val(),//搜索内容
    }, function (jn) {
        if (jn.length == 1) {
            //判断是否有其他关联用户
            var mr = jn[0].myRelationships;
            var mm = jn[0].customer;
            if (mr.length > 0) {
                var chtml = '<div class="ah-flex ah-register-active ah-doctorIndex-register-user" ah-cid="' + mm.customerID + '"'
                    + 'ah-birthday="' + mm.birthday + '"'
                    + 'ah-customerMobile="' + mm.customerMobile + '"'
                    + 'ah-customerName="' + mm.customerName + '"'
                    + 'ah-gender="' + mm.gender + '"'
                    + '><img src="' + $.getImgPath(pagedata.customerPicPath, mm.customerPhoto) + '" width="48" />'
                    + '<div class="ah-flex ah-flex-column ah-hidden-text">'
                    + '   <div>' + mm.customerName + '(' + $.peopleGender(mm.gender) + ') - ' + mm.age + '</div>'
                    + '   <div class="ah-hidden-text">' + mm.iDcard + '</div>'
                    + '</div></div>';
                var mhtml = "";
                for (var i in mr) {
                    var wrap = $("<div class='ah-my-relationships'></div>");
                    //是否最后一个
                    if (parseInt(i) + 1 == mr.length) {
                        wrap.addClass("ah-last-line");
                    }
                    var uhtml = "<div ah-cid='" + mr[i].customerID + "'" +
                        "ah-birthday='" + mr[i].birthday + "'" +
                        "ah-customerMobile='" + mr[i].customerMobile + "'" +
                        "ah-customerName='" + mr[i].customerName + "'";
                    if (mr[i].genderName == "女") {
                        uhtml += "ah-gender='0'";
                    } else {
                        uhtml += "ah-gender='1'";
                    }
                    uhtml += " >(" + mr[i].relationshipTypeName + ") "
                        + mr[i].customerName + "- " + mr[i].genderName + "- " + mr[i].customerMobile + "</div>"
                    mhtml += wrap.html(uhtml)[0].outerHTML;
                }
                $("div[ah-id='customer-more']").removeClass("ah-hide").html(chtml).append(mhtml);
            } else {
                $("div[ah-id='customer-more']").addClass("ah-hide");
                activeCustomer(mm);
            }

        } else if (jn.length > 1) {
            $("#Customer_Id").val("");
            $.err("此条件包含多个人,请换搜索词条！")
        } else {
            $("#Customer_Id").val("");
            $("#Customer_Name").val("")
            $("#Customer_Birthday").val(new XDate().toString("yyyy-MM-dd"));
            dateChangeRange($("#Customer_Birthday"));
            $("div[ah-id='customer-more']").html("");
        }
    });
}
//获取医生的状态信息
function detectDoctorStatus() {
    var url = pagedata.doctorStatusUrl + "?ids=" + AppIds.join();
    $.post(url, {}, function (jn) {
        if (jn.code == 200) {
            jn.data.push({ "appid": 0, "status": "Offline" });
            $.each(jn.data, function (i, m) {
                setDoctorStatus(m.appid, m.status);
            });
        }
    });

    ////以下用于测试
    //var rlt = new Array();
    //for (var i in AppIds) { rlt.push({ appid: AppIds[i], status: getStatus() }); }
    //for (var i in rlt) { var item = rlt[i]; setDoctorStatus(item.appid, item.status); }
    //function getStatus() {
    //    var status = "Online_Free";
    //    var rlt = parseInt(Math.random() * 10000) % 3;
    //    if (rlt == 0) status = "Online_Free";
    //    if (rlt == 1) status = "Online_Busy";
    //    if (rlt == 2) status = "Offline";
    //    return status;
    //}

}



//设置医生在线状态
function setDoctorStatus(appid, status) {
    // console.log(appid + status);

    var clsname = "", html = ""; var canLine = false;
    if (!status) status = "offline";
    switch (status.toLowerCase()) {
        case "online_free":
            clsname = "ah-online-free";
            canLine = true;
            break;
        case "online_busy":
            clsname = "ah-online-busy";
            html = '<i class="fa fa-video-camera" title="忙线中..."></i>';
            break;
        case "offline":
        default:
            clsname = "ah-online-off";
            html = '<i class="fa fa-ban" title="离线"></i>';
            break;
    }
    $('[eleid=docpicapp_' + appid + ']').removeClass("ah-online-free")
        .removeClass("ah-online-busy")
        .removeClass("ah-online-off")
        .addClass(clsname).html(html);

    var btneles = $('[eleid=vbtn_' + appid + ']');
    btneles.removeProp("disabled").removeProp("title");
    if (!canLine) btneles.prop("disabled", true).prop("title", "连线中，不能呼叫");
}

//查看医生介绍
function DrugStoreNureseRegister_DoctorInfo(doctorId) {
    $.post("/Nurse/DrugStoreNurseRegister_pvDoctorInfo", { doctorId: doctorId }, function (html) {
        $.open({
            type: 1,
            title: "医生详情",
            content: html,
        })
    });
}