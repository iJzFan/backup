$(function () {
    //标准日历
    layui.use(['laydate'], function () {
        var laydate = layui.laydate;
        var op = {
            elem: '#ah-doctorIndex-dateIcon'
            , trigger: 'click'
            , calendar: true
        };
        var newDate = new XDate();
        op.max = newDate.toString("yyyy-MM-dd");
        op.min = newDate.getFullYear() - 90 + "-" + (parseInt(newDate.getMonth()) + 1) + "-1";
        op.done = function (value, date) {
            $("#Customer_Birthday").val(value)
            $("#ah-doctorIndex-dateIcon").html("");
            dateChangeRange($("#Customer_Birthday"));
        }
        laydate.render(op);
    });
    //获取待接诊数据
    Json_PatientList(0);
    //获取已接诊数据
    Json_PatientList(2);
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
            opMan: $("#register_source").val()
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
//获取接诊数据
function Json_PatientList(type) {
    $.loadJSON("/Doctor/Json_PatientList", { type: type }, function (fn) {
        if (fn.rlt) {
            var html = "";
            for (var i in fn.items) {
                var pic = "<div class='user-pic' style=\"background:url(" + fn.items[i].userImageUrl + ")\"></div>";
                if (fn.items[i].isVip) {
                    pic += "<i class='ah-img-vip'></i>"
                }
                var info = "<div class='user-name ah-flex ah-flex-column'>" +
                    "<div>" + fn.items[i].customerName + "<span>(" + fn.items[i].gender + "," + fn.items[i].age + ")<span></div>" +
                    "<div style='margin-top:5px'><span class='user-dep'>" + fn.items[i].registDepartmentName + "</span>" + fn.items[i].visitTime + "</div>" +
                    "</div><div class='ah-flex ah-flex-column'>" +
                    "<div><a href='/Doctor/PatientDetail?customerId=" + fn.items[i].customerId + "&registId=" + fn.items[i].registId + "&treatId=" + fn.items[i].treatId + "' onclick='confirmTreat(event," + type + ");'>接诊</a></div>" +
                    //"<div>" + fn.items[i].sourceFrom + "</div>" +
                    "</div>";

                html += "<li>" + pic + info + "</li>";
            }
            if (fn.items.length > 0) {
                if (type == 0) {
                    $("#PatientList").html(html);
                } else if (type == 2) {
                    $("#Patient2List").html(html);
                }
            }
        } else {
            $.err(fn.msg);
        }
    });
}

//已接诊询问
function toPatient(customerID, registerId, treatId) {
    $.confirm("确认", "是否重新接诊？", function () {
        window.location.href = '/Doctor/PatientDetail?customerId=' + customerID + '&registId=' + registerId + '&treatId=' + treatId;
    })
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

function confirmTreat(evt, type) {
    var href = evt.target.href;
    //console.log(type + ":" + href);
    if (type == 2) {
        evt.preventDefault();
        $.confirm("是否接诊", "已经接诊完毕的项目，是否继续接诊?", function () {
            top.location.href = href;
        })
    }
}