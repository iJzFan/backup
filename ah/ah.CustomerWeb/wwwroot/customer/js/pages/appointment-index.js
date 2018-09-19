
var op = {
    stationId: null,//用于存储工作站的ID
    pageIndex: 1,//当前页
    pageSize: 3,//页条数
    isScrollAjax: true,//避免下拉重复加载
    customerId: null,//预约的用户ID
    departmentId: null,//科室ID
    departmentType: null,//科室类型   
    doctorId: null,//医生ID 
    date: null,//预约时间
    slotNum: null,//预约班次
    getViewType: function () {
        if ($(window).width() > 800) {
            return "pc";
        } else {
            return "app"
        }
    }, //显示方式
    getTopDiv: function () { return $(".ah-appointmert-wrap") },//响应式顶层div
    getInputData: function () {//获取输入的值
        var $topc = op.getTopDiv();
        var sex = $topc.find('input:radio[name="sex"]:checked').val();
        var uname = $topc.find('input[name=userName]').val();
        var ubirthday = $topc.find('input[name=birthday]').val();
        var account = op.getAccountValue();
        var isok = !!(account && ubirthday && uname && (sex != null || sex != ""));
        return { customerName: uname, birthday: ubirthday, gender: sex, account: account, isOK: isok };
    },
    clearInputData: function () {
        op.customerId = null;
        var $topc = op.getTopDiv();
        $topc.find('input[Name=userName]').val('');
        $topc.find('input[name=birthday]').val('');
        $topc.find('input:radio[name=sex]').removeAttr("checked");
        $('input[name="sex"]').iCheck('uncheck');
        op.setSubmitEnable(false);
        op.setResvationInfo(0);
    },
    setResvationInfo: function (status, num) {// 设置预约状态信息
        var $c = op.getTopDiv().find(".resv-status");
        $c.find(".can-resv").hide();
        $c.find(".not-resv").hide();
        if (status == 1) {
            $c.find(".can-resv").css("visibility", "visible").show().find("b").text(num); op.setSubmitEnable(true);
            if (op.isQuickSet && !op.customerId) op.setSubmitEnable(false);//如果是快速设置，则设置为不可用
        }
        else if (status == 2) { $c.find(".not-resv").show(); op.setSubmitEnable(false); }
        else { $c.find(".can-resv").show().css("visibility", "hidden"); op.setSubmitEnable(false); }
    },
    setSubmitEnable: function (isEnabled) {
        op.getTopDiv().find(".sub-btn").prop("disabled", !isEnabled);//设置为禁用/可用
    },
    getAccountValue: function () {
        return op.getTopDiv().find('input[data-role=account]').val();
    },
    GetSearchMore: function (pageIndex, val) {
        var pageSize = 10;
        if ($(window).width()>800) {
            pageSize = 3;
        }
        
        if (!pageIndex) {
            pageIndex = 1;
        }
        //loading层            
        var index = (op.getViewType() == "app") && layer.load(1, { shade: [0.7, '#000'] /*0.1透明度的白色背景*/ });
        var opt = {
            pageIndex: pageIndex, pageSize: pageSize,
            searchText: val,
            Lat: null,
            Lng: null
        };
        if (op.lat != 0 || op.lng != 0) {
            opt.Lat = op.lat;
            opt.Lng = op.lng;
        }
        $.get("/Customer/Appointment/StationInfos", opt, function (html) {
            $(".loading-tips").hide();
            layer.close(index)
            $(".index-search-items").html(html);
            if ($(window).width() > 800) {
                $(".pc-search-wrap").show();
            }
        });
    }
}
var STEP = {
    InitStep1: function () {
        $(".index-search-items ").on("click", ".main", function () {
            var _this = $(this);
            window.location.href = "/Customer/Appointment/IndexStep2?stationName=" + _this.attr("ah-sname") + "&stationId=" + _this.attr("ah-sid");
        })
        $(".index-search-items ").on("click", ".toFollow", function (event) {
            var _this = $(this);
            event.stopPropagation();
            var stationId = _this.attr("ah-data");
            var state = true;
            if (_this.attr("ah-state") == "false") {
                state = false;
            }
            $.isFollow(stationId, null, state, function () {
                if (state) {
                    _this.attr("ah-state","false")
                    _this.removeClass("fa-star-o").addClass("fa-star");
                } else {
                    _this.attr("ah-state", "true")
                    _this.removeClass("fa-star").addClass("fa-star-o");
                }
            })
        });
        $(".index-search-items ").on("click", ".toMap", function (event) {
            var _this = $(this);
            event.stopPropagation();
            if ($(window).width() > 800) {
                var index = layer.open({
                    type: 1,
                    title: "位置",
                    shade: 0.5,
                    area: ['1234px', '630px'],
                    content: '<div id="container"></div>'
                });
            } else {
                var index = layer.open({
                    type: 1,
                    title: false,
                    closeBtn: 0,
                    shade: 0.5,
                    area: ['100%', '100%'],
                    content: '<div id="container"></div><a class="map-return">返回</a>'
                });
            }

            $(".map-return").click(function () {
                layer.close(index);
            });
            console.log(_this.attr("lo"), _this.attr("la"))
            var center = new qq.maps.LatLng(_this.attr("lo"), _this.attr("la"));
            var map = new qq.maps.Map(document.getElementById('container'), {
                center: center,
                zoom: 13
            });
            var infoWin = new qq.maps.InfoWindow({
                map: map
            });
            infoWin.open();
            //tips  自定义内容
            infoWin.setContent('<div style="width:200px;">' + _this.attr("name") + '<br>' + _this.attr("add") + '</div>');
            infoWin.setPosition(center);
        })
    },
    InitStep2: function () {

        //icheck 复选框初始化
        $('input[name="sex"]').iCheck({
            checkboxClass: 'icheckbox_square-green',
            radioClass: 'iradio_flat-green',
        });

        //出生年月日历初始化
        $($("input[name='birthday']")[0]).fdatepicker({
            format: 'yyyy-mm-dd',
            startDate: '1900-1-1',
            endDate: new Date()
        });
        $($("input[name='birthday']")[1]).calendar({
            minDate: '1900-1-1',
            maxDate: new Date()
        });


        //pc端日历初始化
        $('#appointment-date').fdatepicker({
            format: 'yyyy-mm-dd',
            startDate: new Date().addDays(-1),
            endDate: new Date().addDays(19),
            todayBtn: true, todayHighlight: true
            // daysOfWeekDisabled:[1,2,3]
        }).on('changeDate', function (ev) {
            var nowTemp = new Date(ev.date);
            var m = nowTemp.getMonth() + 1;
            op.date = nowTemp.getFullYear() + "-" + m + "-" + nowTemp.getDate();
            op.slotNum = null;
            newSlotNum();
        });
        //移动端日历初始化
        $('#appointment-date-app').calendar({
            minDate: new Date().addDays(-1),
            maxDate: new Date().addDays(19) /*预约到20天内*/,
            onChange: function (p, values, displayValues) {
                op.date = values[0];
                op.slotNum = null;
                newSlotNum();
            },
        });
        //如果自动填写了手机，则初始载入
        op.getAccountValue() && $('#btn_' + op.getViewType() + '_getcust').click();
        $('[name=userName],[name=birthday],[name=sex]').blur(checkInputs);//检测输入

    }
}
var APPOINTMENT = {
    //心理问卷
    PsyQuestionnaire: function () {
        if (!op.customerId || op.customerId == null) {
            layer.msg("请补充预约人员信息！");
            $("select[name='departmentsSelect']").val("");
            $("select[name='doctorSelect']").val("");
            return 0;
        }
        var config = {
            type: 2,
            shade: 0.5,
            area: ['100%', '100%'],
            title: false,
            closeBtn: 0,
            content: "/Customer/Appointment/PsyQuestionnaire?customerId=" + op.customerId + "&doctorId=" + op.doctorId
        }
        if (op.getViewType() == "pc") {
            config.area = ['1018px', '720px'];
            config.title = "心理调研";
            config.closeBtn = 1;
        }
        //iframe层
        var index = layer.open(config);
    },
    //心理问卷回调
    PsyQuestionnaire_CallBack: function (propName, propValue) {
        op.propName = propName;
        op.propValue = propValue;
    }
}


//步骤
function pc_step(sId, num, stationName) {
    op.stationId = sId;
    op.stationName = stationName;
    if (num == 2) {
        op.getAccountValue() && $('#btn_' + op.getViewType() + '_getcust').click();
    } else {
        $(".pc-search-wrap").show();
        $(".index-search-items").show("fast");
        $(".index-appointment-info").hide();
        if (op.oldWidth <= 800) {
            $(".app-search-wrap").show();
        }
        if (op.isQuickSet) {
            op.isQuickSet = false;
            getStationList(op.pageIndex, op.pageSize, op.getViewType());
        }
        op.doctorId = null;
        op.stationId = null;
        op.departmentId = null;
    }
}



//快速注册
function quickReg(_this) {
    //快速注册后返回之后，重新载入信息
    var d = op.getInputData();
    var account = d.account;
    if (!account) $.msg("请输入您的注册手机号/邮箱");
    var bCan = $(_this).is(":visible") && d.isOK;//是否可进行注册操作
    if (!(d.isOK)) { $.err("必填信息填写不完整。"); return; }
    bCan && $.getJSON("/api/customer/SendRegVCode", { regAccount: account }, function (jn) {
        if (jn.rlt) {
            layer.prompt({ title: '请输入验证码', formType: 1 }, function (pass, index) {
                layer.close(index);
                var d = op.getInputData();
                d.vcode = pass;
                if (!d.isOK) layer.msg("请填写必要的信息");
                d.isOK && $.getJSON("/api/customer/Json_CustomerQuickRegist", d, function (jn) {
                    if (jn.rlt) $('#btn_' + op.getViewType() + '_getcust').click();
                    else layer.msg("注册失败" + jn.msg);
                });
            });
        } else layer.msg(jn.msg);
    });
}

//补充信息
function customerInfoByKey(e,_doctorId) {
    var $that = $(e);
    var $topdiv = $that.parents(".wrap-main");//当前最高父级


    op.clearInputData();

    var key = $(e).prev().val();
    $(".can-hide").each(function () {
        $(this).css("display", "none");
    });
    $.ajax({
        async: false,
        type: "post",
        url: "/Api/Customer/Json_CustomerInfoByKey",
        data: { key: key, bMask: true, bNameMask: true },
        dataType: "json",
        success: function (jn) {
            var w = $(e).parent().next();
            w.html("").hide();//清除旧数据
            if (jn.rlt) {
                var wrap = $("<div class='appointment-user-items'></div>");
                var l = $("<label class='flex-label'></label>");
                var r = $('<input type="radio" value="' + jn.customer.customerId + '" name="user-items" />');
                var s = $("<span class='u-item'><span class='uname'>{0}</span><s>{1}</s><i>{2}</i><i>{3}</i></span>".format(
                    jn.customer.customerName,
                    jn.customer.genderName,
                    jn.customer.age,
                    jn.customer.idCardNumber));
                l.append(r).append(s);
                w.append(wrap.append(l)).show();
                $('input[name="user-items"]').iCheck({
                    checkboxClass: 'icheckbox_square-green',
                    radioClass: 'iradio_flat-green',
                });

                var uck = {
                    checked: function (event) {
                        if (!op.isQuickSet) {
                            if (!_doctorId) {
                                //清空数据
                                $("select[name='departmentsSelect']").val("");
                                $("select[name='doctorSelect']").val("");
                            }
                        }
                        op.customerId = parseInt(event.target.value);
                        $(".can-hide").each(function () {
                            $(this).css("display", "none");
                        });

                        checkInputs();
                    },
                    checkOneCustomer: function () {
                        //如果找到一条数据，则自动选择该条数据
                        var fd = op.getTopDiv().find('input[name="user-items"]');
                        if (fd.length == 1) {
                            fd.first().iCheck('check');
                        }
                    }
                }

                $('input[name="user-items"]').on('ifChecked', function (event) { uck.checked(event); });
                uck.checkOneCustomer();

                checkInputs();
            } else {
                layer.msg("暂无该用户信息，请补充！");
                $(".can-hide").each(function () {
                    $(this).css("display", "flex");
                });
            }

        }
    });
}

function inputSets(that, bBlur) {
    var s = $(that).val();
    op.setSubmitEnable(false);
    if (bBlur) { $('#btn_' + op.getViewType() + '_getcust').click(); }
}
//获取科室
function getDepartsOfStation(id, departId, doctorId) {
    $.ajax({
        async: false,
        type: "get",
        url: CONFIG.ApiRoot+ "/openapi/Common/JetDepartmentsOfTreatStation",
        data: { stationId: id },
        dataType: "json",
        success: function (jn) {
            if (jn.rlt) {
                if (jn.items.length > 0) {
                    $("select[name='departmentsSelect']").each(function () {
                        var _this = $(this);
                        _this.html("");
                        _this.append("<option value=''>请选择科室</option>")
                        for (var i = 0; i < jn.items.length; i++) {
                            var option = "<option value='" + jn.items[i].departmentID + "' type='" + jn.items[i].spetialDepartTypeVal + "'>" + jn.items[i].departmentName + "</option>"
                            _this.append(option);
                        }
                        if (departId != 0) {
                            _this.val(parseInt(departId));
                            op.departmentId = parseInt(departId);
                            getDoctorOfDepartment(parseInt(departId), parseInt(doctorId));
                        }
                        _this.unbind();//去除所有事件监听 避免重复触发
                        _this.change(function () {
                            op.departmentId = $(this).val();
                            op.departmentType = $(this).find("option:selected").attr("type");
                            $("#appointment-date").val("");
                            $("#appointment-date-app").val("");
                            op.date = null;
                            op.doctorId = null;
                            op.isQuickSet = false;//只要改变了值，则快速预约失效
                            //查询科室的医生
                            getDoctorOfDepartment($(this).val(),0);
                        });
                        //快速设置科室
                        if (op.isQuickSet) {
                            _this.val(op.departmentId);
                            getDoctorOfDepartment(op.departmentId,0);
                        }
                    });
                } else {
                    pc_step(op.stationId, 1);
                    layer.msg("该工作站暂无可用科室，请更换其他工作站！");
                }
            } else {
                layer.msg("查询科室失败，请重试！");
            }
        }
    });
}

//显示所有的医生
function showAllDoctor() {
    $.ajax({
        async: false,
        type: "post",
        url: "/Api/Doctor/Json_GetDoctorOfStation",
        data: { stationId: op.stationId },
        dataType: "json",
        success: function (jn) {
            Json_GetDoctor(jn.docList, "info");
        }
    });
}

//根据科室ID查询医生 弹出医生的详细信息
function getDoctorOfDepartment(id,doctorId) {
    $.ajax({
        async: false,
        type: "post",
        url: "/Api/Doctor/Json_GetDoctorOfDepartment",
        data: { departmentId: id },
        dataType: "json",
        success: function (jn) {
            if (jn.rlt) {
                if (jn.docList.length > 0) {
                    $("select[name='doctorSelect']").each(function () {
                        var _this = $(this);
                        _this.html("");
                        _this.append("<option value=''>请选择医生</option>")
                        for (var i = 0; i < jn.docList.length; i++) {
                            _this.append("<option value='" + jn.docList[i].doctorId + "'>" + jn.docList[i].doctorName + "</option>")
                        }
                        if (doctorId != 0) {
                            _this.val(parseInt(doctorId));
                            op.doctorId = parseInt(doctorId);
                        } else {
                            if (!op.isQuickSet) Json_GetDoctor(jn.docList, "select");
                        }
                        _this.unbind();//去除所有事件监听 避免重复触发
                        _this.change(function () {
                            op.doctorId = $(this).val();
                            $("#appointment-date").val(""); $("#appointment-date-app").val(""); op.date = null;
                            $('[name=slotNum]').val(''); op.slotNum = null;
                        });
                        //快速设置选择的医生
                        if (op.isQuickSet) {
                            _this.val(op.initDoctorId);
                            op.doctorId = op.initDoctorId;
                            $("#appointment-date").val(op.initDate);
                            $("#appointment-date-app").val(op.initDate);
                            op.date = op.initDate;
                            $('[name=slotNum]').val(op.initSlot); op.slotNum = op.initSlot;
                            slotChange(op.initSlot);
                        }
                    });
                } else {
                    $("select[name='doctorSelect']").each(function () {
                        $(this).html("");
                        $(this).append("<option value=''>无可用医生</option>");
                    });
                    layer.msg("该科室暂无可用医生，请更换其他科室！");
                }
            } else {
                layer.msg("查询医生列表失败，请重试！");
            }
        }
    });
}

//医生展示
function Json_GetDoctor(json, type) {
    if (op.getViewType() == "pc") {
        var h = $("<ul id='doctorList' class='Appointment-Select-Doctor'></ul>");
    } else {
        var h = $("<ul id='doctorList' class='Appointment-Select-Doctor-app'></ul>");
    }
    $.each(json, function (i, m) {
        var html = "<li ah-val='{0}'><img src='{1}' /><div><span class='doctorName'>{2}<span>({3})</span></span><span class='rmk'>{4}</span></div ></li > ".format(m.doctorId, img(m.photoUrlDef), m.doctorName, m.postTitleName, rmk(m.doctorSkillRmk));
        h.append(html);
    });
    if (type == "info") {
        h = h.addClass("Appointment-Select-Doctor-stop");
        h = "<div class='Appointment-Select-Doctor-tips'>该列表仅用于浏览，如需选择医生 请选择对应的科室</div>" + h[0].outerHTML;
    } else {
        h = h[0].outerHTML;
    }
    //使用公共自适应方法打开层
    var index = $.open({
        type: 1, shade: 0.5, title: "医生列表",
        area: ['1018px', '620px'],
        content: h
    });
    if (type == "info") {
        $('#doctorList').on("click", "li", function (e) {
            layer.close(index);
            layer.msg("该列表仅用于浏览，如需选择医生 请选择对应的科室");
        });
    } else {
        $('#doctorList').on("click", "li", function (e) {
            var doctorId = $(e.currentTarget).attr("ah-val");//选择到的医生Id
            layer.close(index);
            $("select[name='doctorSelect']").val(doctorId);
            op.doctorId = doctorId;//设置op的数据
            switch (op.departmentType) {
                case "PSYCH": {
                    APPOINTMENT.PsyQuestionnaire(); break;
                }
            }
        });
    }


    function img(u) {
        if (!u) return "";
        console.log(pagedata)
        if (u.toLowerCase().indexOf("http") >= 0) return u;
        else return pagedata.doctorImgRoot + u;
    }
    function rmk(u) {
        if (!u) {
            return "暂无介绍"
        } else {
            if (u.length > 60) {
                u = u.substring(0, 60) + " ...";
            }
            return u
        }
    }
}

//初始化班次
function newSlotNum() { $("select[name='slotNum']").each(function () { $(this).val(""); }); }

//提交预约
function appointmentSubmit() {
    if (op.customerId && op.departmentId && op.doctorId && op.date && op.slotNum) {
        $.post("/Customer/Appointment/GetReservationInfo",
            { customerId: op.customerId, departmentId: op.departmentId, doctorId: op.doctorId, reservationDate: op.date, reservationSlot: op.slotNum, propName: op.propName, propValue: op.propValue }, function (html) {
                $(".index-appointment-info").html(html);
            });
    } else op.setSubmitEnable(false);
}

//检测输入值
function checkInputs() {
    var v0 = op.getTopDiv().find(".can-resv").is(":visible");
    var v = v0 && op.getTopDiv().find(".can-resv b").is(":visible");//只有现实了可预约的情况，才能让提交预约按钮可用
    console.log("预约内容显示:" + v0 + "|" + v);
    if ($('[name=userName]:visible').length > 0) {
        var dd = op.getInputData();
        if (dd.userName && dd.birthday && dd.gender) op.setSubmitEnable(v);
        else op.setSubmitEnable(false);
    }
    // op.setResvationInfo(0);
    if (op.customerId && op.departmentId && op.doctorId && op.date && op.slotNum) {
        op.setSubmitEnable(v);
        if (v) op.getTopDiv().find(".can-resv").css("visibility", '');
    }
    else {
        op.setSubmitEnable(false);
        op.setResvationInfo();
    }
}

//班次修改
function slotChange(val) {
    op.slotNum = null;
    op.slotNum = val;
    checkInputs();
    getDoctorReservationInfo();
    //医生排期
    function getDoctorReservationInfo() {
        op.setResvationInfo(0);
        op.slotNum && $.getJSON("/Api/Doctor/Json_GetDoctorReservationInfo",
            { doctorId: op.doctorId, departId: op.departmentId, date: op.date, slotNum: op.slotNum },
            function (jn) {
                if (jn.rlt) {
                    if (jn.isFull) op.setResvationInfo(2);
                    else {
                        op.setResvationInfo(1, jn.restReservatedNum);
                        checkInputs();
                    }
                } else { layer.msg("查询医生班次失败，请重试！"); }
            });
    }
}



