
$(function () {
    /*
	 Fullscreen background
	 
    $.backstretch("/images/web_bg.jpg");
    $('#byValDispaly').hide();//默认的情况下隐藏input
    $('#top-navbar-1').on('shown.bs.collapse', function () {
        $.backstretch("resize");
    });
    $('#top-navbar-1').on('hidden.bs.collapse', function () {
        $.backstretch("resize");
    });*/

    /*
	 Form
	 */
    
    $('.registration-form fieldset:first-child').fadeIn('slow');

    $('.registration-form input[type="text"], .registration-form input[type="password"], .registration-form textarea').on('focus', function () {
        $(this).removeClass('input-error');
    });

    // next step
    $('.registration-form').on("click", ".btn-next", function () {
        var parent_fieldset = $(this).parents('fieldset');
        var next_step = true;
        parent_fieldset.find('input[type="text"]').each(function () {
            if ($(this).val() == "") {
                $(this).addClass('input-error');
                next_step = false;
            }
            else {
                $(this).removeClass('input-error');
            }
        });
        if (next_step) {

            parent_fieldset.fadeOut(400, function () {

                $(this).next().fadeIn();
            });
        }
    });

    //根据用户名和身份证或者手机号搜索
    $('#gh_search').click(function () {
        var d = { cusName: $('#formUserName').val(), searchNumber: $('#formUserIdMobile').val() }
        $.getJSON("/Reservation/Json_GetCustomerList", d, function (jn) {
            if (jn.rlt) {
                var fnum = jn.items.length;
                if (fnum == 0) {
                    $.confirm("询问", "用户信息不完善，是否完善信息?", function () { location.href = "/Reservation/Register"; })

                    //if (confirm("您输入的信息有误，或者您还不是会员，请先注册会员！")) { location.href = "/v20/Reservation/Register" }
                } else if (fnum == 1) {
                    //直接选择门诊和医生
                    $('#userInfo').fadeOut(400, function () {
                        $("#customerid").val(jn.items[0].customerId);
                        $('.department').fadeIn();
                        getStationList(1); //载入门诊信息
                    });
                } else {
                    //列表
                    $('#userInfo').fadeOut(400, function () {
                        $('#userSelect').fadeIn();
                    });
                    //模板与模板数据
                    var html = template('selUserTemplate', jn);
                    //插入生成的数据
                    document.getElementById('userList').innerHTML = html;
                }
            }
        });
    });




    //获取预约信息
    $('.btn-reservation').click(function () {
        var next_step = true;
        //判断信息为空就不能预约
        $('.his_info').find('select[id="departmentSelect"], select[id="DocsSelect"], input[class="form-control"]').each(function () {
            if ($(this).val() == "default" || $(this).val() == "") {
                $(this).addClass('input-error');
                next_step = false;
            }
            else {
                $(this).removeClass('input-error');
            }
        });

        if (next_step) {
            var d = {
                customerID: $('#customerid').val(),
                stationName: $('#form-first-names').val(),
                departmentID: $('#departmentSelect').val(),
                doctorId: $('#DocsSelect').val(),
                ReservationDate: $('#reservation_time').val(),
                reservationSlot: $('#day-time').val()
            };

            $.confirm("确认", "是否真的预约就诊？", function () {
                $.loadJSON("/Reservation/Json_GetReservationInfo", d, function (jn) {
                    if (jn.rlt) {
                        //列表
                        $('.his_info').fadeOut(400, function () {
                            $('#reservation-info').fadeIn();
                        });
                        //模板与模板数据
                        var html = template('selInfoTemplate', jn);
                        //插入生成的数据
                        document.getElementById('reservation-success').innerHTML = html;
                        //定时后跳转
                        finishedSecondsShowAndRedirectRefresh();
                    } else $.alertError(jn.msg);
                })


            });
        }
    })



    function getStationList(pageindex) {
        $.loadJSON("/Reservation/Json_GetStationList", { pageIndexSearch: pageindex }, function (jn) {
            //列表
            //模板与模板数据   
            var html = template('selDepTemplate', jn);
            //插入生成的数据
            document.getElementById('departmemt_list').innerHTML = html;
        });
    }
    function finishedSecondsShowAndRedirectRefresh() {
        //完成后秒数倒计时显示，超时则自动刷新
        var sec = 12;
        var index = setInterval(function () {
            $('#finishedSecondsShow').text(sec--);
            if (sec <= 0) {
                clearInterval(index);
                $('A#btn_finished').trigger("click");
                location.reload(true);
            }
        }, 1000);
    }


    //选择门诊
    $('.registration-form').on("click", ".choose_department", function () {
        var ds = $(this).find('input.customerIdSelect').val();
        $("#customerid").val(ds);
        getStationList(1);
    });

    $('.registration-form').on("click", "#staion_pager .next,#staion_pager .previous", function () {
        var nowpage = parseInt($(this).parent().find(".page-index").text());
        var totalpages = parseInt($(this).parent().find("#page_totalPages").val());
        var pageflag = parseInt($(this).data("flag"));
        var pageindex = nowpage + pageflag;
        if (pageindex < 1) pageindex = 1;
        if (pageindex > totalpages) pageindex = totalpages;
        getStationList(pageindex);
    });
    // previous step
    $('.registration-form .btn-previous').on('click', function () {
        $(this).parents('fieldset').fadeOut(400, function () {
            $(this).prev().fadeIn();
        });
    });

    //回到首页
    $('.registration-form .btn-index').on('click', function () {
        $(this).parents('fieldset').fadeOut(400, function () {
            $('#userInfo').fadeIn();
        });
    });

    //网络医生
    $('.registration-form .btn-doctor').on('click', function () {
        $(this).parents('fieldset').fadeOut(400, function () {
            $('.choose_doctor').fadeIn();
        });
    });

    //预约信息
    $('.registration-form .btn-his').on('click', function () {
        $(this).parents('fieldset').fadeOut(400, function () {
            $('.his_info').fadeIn();
        });
    });

    //选择门诊
    $('.registration-form .btn-department').on('click', function () {
        $(this).parents('fieldset').fadeOut(400, function () {
            $('.department').fadeIn();
        });
    });

    $('#DocsSelect').on("change", loadDoctorWorkInfos);


    // submit
    $('.registration-form').on('submit', function (e) {
        $(this).find('input[type="text"], input[type="password"], textarea').each(function () {
            if ($(this).val() == "") {
                e.preventDefault();
                $(this).addClass('input-error');
            }
            else {
                //首先把预约信息插入到数据同时把注册格式成字符，把格式好的文本数据通过Json_SendMobCode（string mob,string content）的方法
                //把预约信息发送给用；
                //通过get方法,参数StationID,CustomerID，RegisterDate，RegisterSlot，Department，EmployeeID，mob
                $(this).removeClass('input-error');
            }
        });

    });

    initialDateTimePicker();


});


function initialDateTimePicker() {
    //出生日期控件
    $('.form_date').datetimepicker({
        language: 'zh-CN',
        weekStart: 1,
        todayBtn: 1,
        autoclose: 1,
        todayHighlight: 1,
        pickerPosition: "bottom-left",
        startView: 2,
        minView: 2,
        forceParse: 0
    });
}
function setWorkDateUIByDocWorkInfor(jn) {
    $('#time_solt_row').removeMaskUnuse();
    $('#reservation_time').datetimepicker('remove');
    //预约时间控件
    $('#reservation_time').datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        weekStart: 1,
        todayBtn: 1,
        autoclose: 1,
        todayHighlight: 1,
        pickerPosition: "bottom-left",
        startView: 2,
        minView: 2,
        forceParse: 0,
        datesDisabled: jn.notWorkDays,
        //daysOfWeekDisabled: [4, 6],
        startDate: new Date(),// new Date().addDays(1),
        endDate: new Date().addDays(15),
        showMeridian: 1,
        todayBtn: false    //关闭选择今天按钮
    });
}


//onchange获取部门的医生列表
function GetDocsByDepartmemtId() {
    $.getJSON("/Reservation/Json_GetDoctorOfDepartment", { departmentId: $('#departmentSelect').val() }, function (jn) {
        $("#DocsSelect").empty().append("<option value=''>-- 选择医生 --</option>");
        $('#day-time').empty();
        $.each(jn.docList, function (i, item) {
            $("#DocsSelect").append("<option value='" + item.doctorId + "'>" + item.doctorName + "</option>");
        });
        loadDoctorWorkInfos();
    })
}

//获取预约时间的医生情况数据
function changeDoctorTime() {
    clearUIOfSlotInfo();
    var date = $('#reservation_time').val().substr(0, 10);
    var docId = $('#DocsSelect').val();
    $('#day-time').append("<option value=''>- 请选择预约时段 -</option>");
    $.each(pdata.WorkInfos, function (i, m) {
        if (m.isWork && m.doctorId == docId && m.workDate.substr(0, 10) == date) {            
            var txt = m.slotIndex == 1 ? "上午" : m.slotIndex == 2 ? "下午" : m.slotIndex == 3 ? "晚班" : m.slotIndex == 4 ? "深夜班(凌晨)" : "未知班";
            txt += " " + m.slotTimeStart.substr(0, 5) + " - " + m.slotTimeEnd.substr(0, 5);
            $('#day-time').append("<option value='" + m.slotIndex + "'>" + txt + "</option>")
        }
    });
}

//更新时间段选择
function changeDoctorTimeSlot() {
    clearUIOfSelRmk();
    var d = {
        doctorId: $('#DocsSelect').val(),
        date: $('#reservation_time').val(),
        slotNum: $('#day-time').val()
    }
    //从后台获取当日医生的预约情况
    d.slotNum && $.loadJSON("/Reservation/Json_GetDoctorReservationInfo", d, function (jn) {
        if (jn.rlt) {
            if (!jn.isWorkSlot) {
                $.alertWarning("今天医生不上班！");
                return;
            }
            if (jn.isFull) {
                $('#restTip').removeClass("ah-Allowed").addClass("ah-Fobidden").show().css("visibility", "");;
                $('#btn_reservation').attr("disabled", "disabled");
                $('#restResNum').text(jn.restReservatedNum);
                $('#fullMark').text("(已满)");
            } else {
                $('#restTip').removeClass("ah-Fobidden").addClass("ah-Allowed").show().css("visibility", "");;
                $('#btn_reservation').removeAttr("disabled");
                $('#restResNum').text(jn.restReservatedNum);
                $('#fullMark').text("");
            }
        } else {
            $.alertError(jn.msg);
        }
    });
}

//载入医生工作出勤信息
function loadDoctorWorkInfos() {
    clearUIOfDoctorWorkInfo();
    var doctorId = $('#DocsSelect').val();
    var stationId = $('#departmentIdVal').val();
    if (doctorId) {
        var d = {
            doctorId: doctorId,
            stationId: stationId,
            dateFrom: (new Date()).formatDateTime("yyyy-MM-dd"),
            dateTo: (new Date()).addDays(15).formatDateTime("yyyy-MM-dd")
        }
        $.loadJSON("/Reservation/Json_GetDoctorWorkInfos", d, function (jn) {
            if (jn.rlt) {
                pdata.WorkInfos = jn.items;//用于存放数据，然后选择时间段
                //设置医生工作时间的日历            
                setWorkDateUIByDocWorkInfor(jn);

            } else { $.alertError(jn.msg); clearUIOfDoctorWorkInfo(); }
        });
    } else { clearUIOfDoctorWorkInfo(); }
}

//界面联动的初始化=============================================================================
//清理医生信息界面
function clearUIOfDoctorWorkInfo() {
    pdata.WorkInfos = null;
    $('#reservation_time').val('');
    $('#time_solt_row').addMaskUnuse();
    $('#reservation_time').datetimepicker('remove');
    clearUIOfSlotInfo();
}
function clearUIOfSlotInfo() {
    $('#day-time').empty();
    clearUIOfSelRmk();
}
function clearUIOfSelRmk() {
    $('#restResNum').text('');
    $('#fullMark').text('');
    $('#restTip').removeClass("ah-Allowed").removeClass("ah-Fobidden").css("visibility", "hidden");
    $('#btn_reservation').attr("disabled", "disabled");
}
//=============================================================================================



//获取工作站的部门列表。
function departmentIdByQuery(stationId) {
    $("#departmentIdVal").val(stationId);
    $.getJSON("/Reservation/Json_departmemtIdByQuery", { stationId: stationId }, function (jn) {
        if (jn.rlt) {
            $("#form-first-names").val(jn.stationName);
            //加载部门的列表
            $('#departmentSelect').empty().append("<option value='default'>请选择门诊</option>");
            $.each(jn.departmemtsList, function (i, item) {
                $('#departmentSelect').append("<option value='" + item.departmentID + "'>" + item.departmentName + "</option>");
            });
            //清理数据
            pdata.WorkInfos = null;
            $('#DocsSelect').empty();
            clearUIOfDoctorWorkInfo();//清理医生数据
        }
        else {
            $.alertError("信息错误:" + jn.msg);
        }
    });
}

//获取短信验证
function getSmsTime(o) {
    var str = "";
    var wait = 60;
    var phonenum = $("#Telephone").val();
    if (phonenum != null) {

        if ($.trim($('#Telephone').val()).length == 0) {
            str += '手机号没有输入\n';
            $.alertMsg(str);
            $('#Telephone').focus();
        } else {
            if ($.f.isPhoneNo($.trim($('#Telephone').val())) == false) {
                str += '手机号码不正确\n';
                $.alertMsg(str);
                $('#Telephone').focus();
            } else {
                o.setAttribute("disabled", true);
                $.getJSON("/Reservation/Json_SendMobCode", { mob: phonenum }, function (jn) {
                    if (jn.rlt) {
                        var tid = setInterval(function () {
                            o.value = "重新发送(" + wait + ")";
                            wait--;
                            if (wait < 0) {
                                clearInterval(tid);
                                o.removeAttribute("disabled");
                                o.value = "免费获取验证码";
                                wait = 60;
                            }
                        }, 1000);
                    } else {
                        //如果错误
                    }
                });
            }

        }

    }

}

function setBirthdayAndSex(val) {
    $("#byValDispaly").hide();
    if (val) {
        var valStr = val.replace(/(^s*)|(s*$)/g, "");
        var str = "12943";
        var str = str.replace(/(^s*)|(s*$)/g, "");
        if (valStr == str) {
            $('#byValDispaly').hide();
        } else {
            $('#byValDispaly').show();
        }
    }
};

//function verifyCodeisNull() {
//        var ver = $("#verifyCode");
//        var v = ver.val();
//        if (v == "" || v == undefined || v == null) {
//            var tips = "<span for='verifyCode' style='color:#b94a48;display:block;'>请输入验证码</span>";
//            ver.css({ "border": "1px solid #b94a48"});
//            ver.closest('.form-group').append(tips);
//        } else {
//            ver.css({ "border": "1px solid #468847"});
//            ver.closest('.form-group').append().remove();
//        }

//}

