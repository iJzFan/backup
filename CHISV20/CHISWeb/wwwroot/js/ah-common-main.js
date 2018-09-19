/*
 *    通用变量
 */
var AH_UPDATAPIC_SIZE_WIDTH = 800;//上传图片宽度压缩限制 
var AH_UPDATAPIC_SIZE_HEIGHT = 1280;//上传图片高度压缩限制 

/*
 *    _LayouMain 通用方法
 */
$(function () {
    var isAndroid = navigator.userAgent.indexOf('Android') > -1 || navigator.userAgent.indexOf('Adr') > -1; //android终端
    var androidHeight = $(window).height();//屏幕原本高度
    //如果是安卓系统 则开启自动滚动
    if (isAndroid) {
        $("input").on("focus click", function (event) {
            autoScrollTop(event);
        });
        $("textarea").on("focus click", function (event) {
            autoScrollTop(event);
        });
    }
    $.InitNavTouch();
    $(window).resize(function () { $.InitNavTouch(); });
    $("input[type='checkbox']").each(function () {
        $(this).attr("lay-skin", "primary")
    });
    //点击其他地方隐藏
    $(document).click(function () {
        $(".ah-nav-user-info").hide();
        $(".ah-navmore-new").removeClass("ah-navmore-show");
    });
    //点击显示用户信息
    $(".ah-user-info").click(function (event) {
        event.stopPropagation();
        $(".ah-navmore-new").removeClass("ah-navmore-show");
        depItem = true;
        changeDep();
        var info = $(".ah-nav-user-info");
        if (info.is(":hidden")) {
            info.show();
        } else {
            info.hide();
        }
        return false;
    });

    //点击导航更多PC
    $(".ah-navtouch-more span").click(function (event) {
        ahNavMore(null, event)
    });
    //点击导航更多 平板
    $(".owl-more").click(function (event) {
        ahNavMore(null, event)
    });

    layui.use(['form', 'element', 'laydate'], function () {
        var $ = layui.jquery;
        var element = layui.element; //Tab的切换功能，切换事件监听等，需要依赖element模块
        var laydate = layui.laydate; //执行一个laydate实例
        var form = layui.form; //执行一个form实例

        //定义Tab监听方法
        $.InitTabCallback = function (f, dom) {
            element.on('tab(' + dom + ')', function (data) {
                f(data);
            });
        }
        //定义Checkbox监听方法
        form.on('checkbox', function (data) {
            //判断onchange是否存在
            if (jQuery.isFunction(data.elem.onchange)) {
                data.elem.onchange();//触发onchange
            }
        });
        //定义radio监听方法
        form.on('radio', function (data) {
            //判断onchange是否存在
            if (jQuery.isFunction(data.elem.onchange)) {
                data.elem.onchange();//触发onchange
            }
        });
        form.on('select', function (data) {
            //data.elem 得到select原始DOM对象   data.value 得到被选中的值    data.othis 得到美化后的DOM对象

            //验证的提示显示和隐藏
            $s = $(data.elem);



            if (("1" + $s.val()) != "1") {
                $s.removeClass("input-validation-error"); $s.parent().find(".field-validation-error").hide();
            } else {
                $s.addClass("input-validation-error"); $s.parent().find(".field-validation-error").show();
            }

            //判断onchange是否存在
            if (jQuery.isFunction(data.elem.onchange)) {
                if (data.value != "null" && data.value != "") {
                    data.elem.onchange();//触发原select的onchange
                }
            }
        });
        //直接显示日历（排班）
        lay('.ah-date-show').each(function () {
            //直接嵌套显示
            laydate.render({
                elem: this
                , position: 'static'
                , showBottom: false
                , calendar: true
                , done: function (value, date) {
                    console.log('你选择的日期是：' + value + '<br>获得的对象是' + JSON.stringify(date));
                }
            });
        });
        //点击icon触发日历
        lay('.ah-date-icon').each(function () {
            var _this = $(this);
            var id = $.returnDateID("dataIcon");
            var icon = $("<i class='ah-dateIcon' id='" + id + "' ></i>");
            _this.parent().append(icon);
            var op = {
                elem: this
                , calendar: true
                , eventElem: "#" + id
                , trigger: 'click'
                , showBottom: false
                , done: function (value, date, endDate) {
                    var fid = _this.attr("for-event");
                    $("#" + fid).val(value);
                }
            };
            laydate.render(op);
            _this.keydown(function (e) {
                if (e.keyCode == 13) {
                    return false;
                }
            });
        });
        //标准日历
        lay('.ah-date').each(function () {
            var op = {
                elem: this
                , calendar: true
            };
            if ($(this).attr("ah-max-date")) {
                op.max = $(this).attr("ah-max-date");
                //op.max = parseInt($(this).attr("ah-max-date"));
            }
            if ($(this).attr("ah-min-date")) {
                op.min = $(this).attr("ah-min-date");
                //op.min = parseInt($(this).attr("ah-min-date"));
            }
            if ($(this).attr("ah-date-age")) {
                var newDate = new XDate();
                op.max = newDate.toString("yyyy-MM-dd");
                op.min = newDate.getFullYear() - 90 + "-" + (parseInt(newDate.getMonth()) + 1) + "-1";
            }
            op.done = function (value, date) {
                //判断onchange是否存在
                if (jQuery.isFunction($(op.elem)[0].onchange)) {
                    $(op.elem)[0].onchange();//触发onchange
                }
            }
            laydate.render(op);
        });
        //日历-时分秒
        lay('.ah-date-time').each(function () {
            laydate.render({
                elem: this
            });
        });
        //日期范围
        lay('.ah-date-range').each(function () {
            var that = this;
            var op = {
                elem: this
                , range: '～'
                , format: 'yyyy年MM月dd日'
                , done: function (value, date, endDate) {
                    //  console.log(value); //得到日期生成的值，如：2017-08-18
                    //   console.log(date); //得到日期时间对象：{year: 2017, month: 8, date: 18, hours: 0, minutes: 0, seconds: 0}
                    //  console.log(endDate); //得结束的日期时间对象，开启范围选择（range: true）才会返回。对象成员同上。
                    if (value) {
                        var val_dt0 = date.year + "-" + date.month + "-" + date.date,
                            val_dt1 = endDate.year + "-" + endDate.month + "-" + endDate.date;
                        var val = "dt0={0};dt1={1};".format(val_dt0, val_dt1);
                        $(that).attr("data-val", val).attr("data-val-dt0", val_dt0).attr("data-val-dt1", val_dt1);
                    } else $(that).attr("data-val", '').attr("data-val-dt0", '').attr("data-val-dt1", '');
                }
            };
            if ($(this).attr("ah-format")) { op.format = $(this).attr("ah-format") }
            if ($(this).attr("ah-type")) { op.type = "datetime"; }
            if ($(this).attr("ah-max-date")) op.max = $(this).attr("ah-max-date");
            if ($(this).attr("ah-min-date")) op.min = $(this).attr("ah-min-date");
            laydate.render(op);
        });
    });

    //预约来源点击监听
    $("body[ah-body='wrap']").on("click", ".register_source_item", function () {
        var _this = $(this);
        $(".register_source_item").each(function () {
            $(this).removeClass("layui-this");
        });
        $(_this).addClass("layui-this");
        var id = $("#register_source_search").attr("ah-outInputId");
        $("#" + id).val(_this.attr("ah-opid"));
        $("#register_source").val(_this.attr("ah-opman"));
        isShowRegisterSource(false);
    })
    //预约来源 鼠标监听
    $(document).on("click touchstart", function (e) {
        e = window.event || e; // 兼容IE7
        obj = $(e.srcElement || e.target);
        //用户信息点击其他位置
        if ($(obj).parents(".ah-register-source-wrap").first().length <= 0 && $(obj).attr("id") != "register_source") {
            if (!$(obj).hasClass("ah-register-source-wrap")) {
                $(".ah-register-source-wrap").hide().removeClass("layui-anim-upbit");
            }
        }
    });
    //来源搜索回车监听
    $("body[ah-body='wrap']").on("keypress", "#register_source_search", function (e) {
        if (e.which == 13) {
            registerSource();
            return false;
        }
    });
    //约号来源得到焦点
    $("body[ah-body='wrap']").on("focus", "#register_source", function () {
        isShowRegisterSource(true);
    });
});

//显示隐藏搜索来源
function isShowRegisterSource(_state) {
    var wrap = $("#register_source").parent();
    var dl_wrap = wrap.find(".ah-register-source-wrap").first();
    if (_state) {
        dl_wrap.show().addClass("layui-anim-upbit");
        $("#register_source_search")[0].focus();
    } else {
        dl_wrap.hide().removeClass("layui-anim-upbit");
    }
}
//约号来源搜索
function registerSource(sid) {
    var dl = $("#register_source").parent().find("dl").first();
    $.get("/openapi/Common/JetNotDoctorOfStation", {
        stationId: sid,
        searchText: $("#register_source_search").val(),//搜索内容
        principalshipId: 12999 //只搜索店员
    }, function (jn) {
        var html = "";
        if (jn.doctors.length <= 0) {
            html += "<dd>暂无数据</dd>"
        } else {
            for (var i = 0; i < jn.doctors.length; i++) {
                html += "<dd class='register_source_item' ah-opMan='" + jn.doctors[i].doctorName + "'ah-opId='" + jn.doctors[i].doctorOpId + "'>" + jn.doctors[i].doctorName + " (" + jn.doctors[i].postTitleName + ")</dd>";
            }
        }
        dl.html(html);
    });
}
//安卓输入时自动滚动到目标
function autoScrollTop(event) {
    var _this = $(event.currentTarget);
    //ah-date 是时间控件 不需要移动焦点
    if (!_this.hasClass("ah-date")) {
        setTimeout(function () {
            var t = _this.offset().top;//距离屏幕顶部的高度
            var st = $('body[ah-body="wrap"]').find(".scroll-content").scrollTop();//已经滚动的高度
            $('body[ah-body="wrap"]').find(".scroll-content").first().scrollTop(t + st - 30);//30偏移量可以看见标题
        }, 1200);
    }
}

//拖动选择年龄
function setAgeDay(selector, _this) {
    var age = _this.value;
    var n = new Date();
    var date = new Date(n.getFullYear() - age, n.getMonth(), 1);
    $(selector).val(date.formatDateTime("yyyy-MM-dd"));
    //初始左边的值+步长*range
    var ileft = 14 + ((_this.max - _this.min) / $(_this).width()) * _this.value;
    $(_this).parent().find("i").text(_this.value).css("left", ileft + "px");
}

//导航滚动
$.InitNavTouch = function () {
    var nav = $('.owl-carousel');
    var pcnav = $(".ah-touchNav-items");
    if ($(document).width() <= 450) {
        nav.owlCarousel({
            autoWidth: true,
            loop: false,
            margin: 10
        });
        pcnav.hide();
        nav.show();
    } else {
        pcnav.show();
        nav.hide();
    }

}
//layui tab 监听
$.onTab = function (f, dom) {
    $("div[lay-filter='" + dom + "'] .layui-tab-title li").on("click", function () {
        if (jQuery.isFunction($.InitTabCallback)) {
            $.InitTabCallback(function (data) {
                f(data);
            }, dom);
        } else {
            layer.msg("模块加载中，请稍后！", { time: 1000 });
        }
    });
}
//layui radio 监听
$.onRadio = function (f) {
    layui.use(['form'], function () {
        var form = layui.form; //执行一个form实例
        form.on('radio', function (data) {
            f(data);//得到radio原始DOM对象  data.elem ,被点击的radio的value值 data.value
        });
    });
}
//更新所有layui
$.updataAllLayui = function () {
    if (top.layui.form) {
        top.layui.form.render()
    } else {
        console.log("初始化layui优先级错误！");
    }
}
//更新layui
$.updataLayui = function () {
    layui.form.render();
    $('.ah-date').each(function () {
        var op = {
            elem: this,
        };
        if ($(this).attr("ah-max-date")) {
            op.max = parseInt($(this).attr("ah-max-date"));
        }
        if ($(this).attr("ah-min-date")) {
            op.min = parseInt($(this).attr("ah-min-date"));
        }
        layui.laydate.render(op);
    });
    //日期控件刷新
    $(".ah-date").unbind("focus").bind("focus", function () {
        if ($(this).hasClass("ah-data-can-input")) { }
        else document.activeElement.blur();
    });

}


//layui tab选中
$.layuiTabSelect = function (wrap, layId) {
    layui.element.tabChange(wrap, layId);
}
//生成select选择项
$.addSelectOption = function (msg, items) {
    if (items.length <= 0) {
        return "<option value=''>暂无数据</option>";
    } else {
        var html = "<option value=''>" + msg + "</option>";
        for (var i = 0; i < items.length; i++) {
            html += "<option value='" + items[i].doctorId + "'>" + items[i].doctorName + "</option>";
        }
        return html;
    }
}


$.layui3LevelAddress = function (option) {
    var $0 = option.$province;
    var $1 = option.$city;
    var $2 = option.$county;
    if ($0.length == 0) return;//没有控件则退出

    layui.use(['form'], function () {
        var $ = layui.jquery;
        var form = layui.form; //执行一个form实例
        form.on('select(province)', function (data) {
            fn.setCitys();
        });
        form.on('select(city)', function (data) {
            fn.setCounties();
        })
        form.on('select(county)', function (data) {
            option.$val.val($2.val());
        })
    });

    var fn = {
        getAreaInfo: function (areaId) {
            var dd = CHINAAREAIDS;
            var l0id = 0, l1id = 0, l2id = 0;
            var fd = lumbdaFind(dd, "Id", areaId); var fd1, fd2;
            if (fd != null) {
                l2id = fd.Id;//区
                if (fd.PId > 0) {
                    l1id = fd.PId;//市
                    fd1 = lumbdaFind(dd, "Id", fd.PId);// dd.find(m => m.Id == fd.PId);
                    if (fd1 != null) {
                        if (fd1.PId > 0) l0id = fd1.PId;//省
                    }
                }
            }
            return { level0Id: l0id, level1Id: l1id, level2Id: l2id }
        },
        optItem: function (txt, val, bsel) {
            if (val == undefined) val = '';
            if (bsel) return "<option value='" + val + "' selected='selected'  >" + txt + "</option>";
            else return "<option value='" + val + "'  >" + txt + "</option>";
        },
        setProvince: function (selval, callback) {
            $0.empty().append(fn.optItem("--请选择省级--"));
            $.each(CHINAAREA, function (i, m) {
                $0.append(fn.optItem(m.AreaName, m.AreaId));
            });
            if (selval) $0.val(selval);
            if (callback) { callback(); }
            else layui.form.render("select");
        },
        setCitys: function (selval, callback) {
            $1.empty().append(fn.optItem("--请选择市--"));
            $2.empty().append(fn.optItem("--请选择区--"));
            var parentId = $0.val();
            if (parentId) {
                var items = lumbdaFind(CHINAAREA, "AreaId", $0.val()).Children;//  CHINAAREA.find(m => m.AreaId == $0.val()).Children;
                $.each(items, function (i, m) { $1.append(fn.optItem(m.AreaName, m.AreaId)); });
                if (selval) $1.val(selval);
                if (callback) { callback(); }
            }
            if (!callback) layui.form.render("select");
        },
        setCounties: function (selval, callback) {
            $2.empty().append(fn.optItem("--请选择区--"));
            var parentId = $1.val();
            if (parentId) {
                var items = lumbdaFind(lumbdaFind(CHINAAREA, "AreaId", $0.val()).Children, "AreaId", $1.val()).Children;
                //  var items = CHINAAREA.find(m => m.AreaId == $0.val()).Children.find(m => m.AreaId == $1.val()).Children;
                $.each(items, function (i, m) { $2.append(fn.optItem(m.AreaName, m.AreaId)); });
                if (selval) $2.val(selval);
                if (callback) { callback(); }
            }
            if (!callback) layui.form.render("select")
        }
    }





    var areaId = option.$val.val();
    if (areaId) {
        $0.hide(); $1.hide(); $2.hide();
        //初始化地址
        //移除disabled
        var bdisabed = false, breadonly = false;
        if (typeof ($0.attr("disabled")) != "undefined") { bdisabed = true; $0.removeAttr("disabled"); $1.removeAttr("disabled"); $2.removeAttr("disabled"); }
        if (typeof ($0.attr("readonly")) != "undefined") { breadonly = true; }

        var a = fn.getAreaInfo(areaId);
        fn.setProvince(a.level0Id, function () {
            fn.setCitys(a.level1Id, function () {
                fn.setCounties(a.level2Id, function () { });
            });
        })

        if (bdisabed) { $0.attr("disabled", ""); $1.attr("disabled", ""); $2.attr("disabled", ""); }
        if (breadonly) {
            $0.attr("disabled", ""); $1.attr("disabled", ""); $2.attr("disabled", "");
            $0.attr("readonly", ""); $1.attr("readonly", ""); $2.attr("readonly", "");
        }
    } else {
        fn.setProvince(null, function () { });
        $1.empty().append(fn.optItem("--请选择市--"));
        $2.empty().append(fn.optItem("--请选择区--"));
    }
}




//转化为年月日格式
$.layuiRangeDate = function (date1, data2, id) {
    var dt0 = new XDate(date1).toString("yyyy年MM月dd日");
    var dt1 = new XDate(date2).toString("yyyy年MM月dd日");
    $(id).val(dt0 + " ～ " + dt1);
}
/**
* 切换工作站
*/
function changeStation() {
    var index = $.open({
        skin: "change-station",
        type: 2,
        title: false,
        shadeClose: true,
        shade: 0.99,
        area: ['80%', '80%'],
        content: '/home/ChangeStation' //iframe的url
    });
}
//显示导航更多
function ahNavMore(_this, event) {
    if (_this) {
        $(".ah-navmore-new").removeClass("ah-navmore-show");
    } else {
        event.stopPropagation();
        $(".ah-nav-user-info").hide();
        if ($(".ah-navmore-new").hasClass("ah-navmore-show")) {
            $(".ah-navmore-new").removeClass("ah-navmore-show");
        } else {
            $(".ah-navmore-new").addClass("ah-navmore-show");
        }
        return false;
    }
}
//日历和滑块交互
function dateChangeRange(_this) {
    var newDate = new XDate();
    var age = newDate.getFullYear() - parseInt($(_this).val().substring(0, 4));
    var range = $(_this).parent().parent().find("input[type='range']");
    range.val(age);
    var ileft = 14 + ((range[0].max - range[0].min) / range.width()) * age;
    range.parent().find("i").text(age).css("left", ileft + "px");
}
var depItem = false;
//切换科室
function changeDep() {
    console.log(depItem)
    if (!depItem) {
        $(".select-dep-items").show();
    } else {
        $(".select-dep-items").hide();
    }
    depItem = !depItem;
}


/**
    显示和隐藏工具栏
*/
function ahTools(_this) {
    $('#J_selector').toggle();
}
/*初始化分页
    coune:数据条数，用于判断是否要初始化分页
*/
$.fn.ahPages = function (count, index, total, fn) {
    if (count != 0) {
        $("#pager").pager({
            pagenumber: index,
            pagecount: total,
            buttonClickCallback: function (pageclickednumber) {
                SEARCH.search(pageclickednumber);
            }
        });
    } else {
        $("#pager").html("");
    }
};
/*----------------------------------------------------滚动条相关------------------------------------------------*/
$.updataScrollbar = function () {
    //滚动条初始化
    $(".scrollbar-dynamic").each(function () {
        $(this).scrollbar();
    });
}
/*------------------------------------------------------loading 相关--------------------------------------------*/
$.loading = function (bool, text) {
    var $loadingpage = top.$("#loadingPage");
    var $loadingtext = $loadingpage.find('.loading-content');
    if (bool) {
        $loadingpage.show();
    } else {
        if ($loadingtext.attr('istableloading') == undefined) {
            $loadingpage.hide();
        }
    }
    if (!!text) {
        $loadingtext.html(text);
    } else {
        $loadingtext.html("数据加载中，请稍后…");
    }
    $loadingtext.css("left", (top.$('body').width() - $loadingtext.width()) / 2 - 50);
    $loadingtext.css("top", (top.$('body').height() - $loadingtext.height()) / 2);
}
/*------------------------------------------------------form 相关--------------------------------------------*/
/*验证Form*/
$.fn.formValid = function () {
    return $(this).valid({
        errorPlacement: function (error, element) {
            element.parents('.formValue').addClass('has-error');
            element.parents('.has-error').find('i.error').remove();
            element.parents('.has-error').append('<i class="form-control-feedback fa fa-exclamation-circle error" data-placement="left" data-toggle="tooltip" title="' + error + '"></i>');
            $("[data-toggle='tooltip']").tooltip();
            if (element.parents('.input-group').hasClass('input-group')) {
                element.parents('.has-error').find('i.error').css('right', '33px')
            }
        },
        success: function (element) {
            element.parents('.has-error').find('i.error').remove();
            element.parent().removeClass('has-error');
        }
    });
}
//form 提交
$.submitForm = function (options) {
    var defaults = {
        url: "",
        param: [],
        loading: "正在提交数据...",
        success: null,
        close: true
    };
    var options = $.extend(defaults, options);
    $.loading(true, options.loading);
    window.setTimeout(function () {
        if ($('[name=__RequestVerificationToken]').length > 0) {
            options.param["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
        }
        $.ajax({
            url: options.url,
            data: options.param,
            type: "post",
            dataType: "json",
            success: function (data) {
                //{state:'success',message:''}
                $.loading(false);
                if (data.rlt) data.state = data.rlt ? "success" : "error";
                if (data.msg) data.message = data.msg || "操作成功";

                if (data.state == "success") {
                    options.success(data);
                    $.modalMsg(data.message, data.state);
                    if (options.close == true) {
                        $.modalClose();
                    }
                } else {
                    $.modalAlert(data.message, data.state);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $.loading(false);
                $.modalMsg(errorThrown, "error");
            },
            beforeSend: function () {
                $.loading(true, options.loading);
            },
            complete: function () {
                $.loading(false);
            }
        });
    }, 500);
}
/*------------------------------ 整体框架的弹窗方法 -----------------------------*/
//询问层
$.confirm = function (title, htmlcontent, funcok, funccancel) {
    var index = layer.confirm(htmlcontent, {
        title: title,
        skin: 'ah-myskin-blue',
    }, function () {
        if (funcok) funcok();
        layer.close(index);
    }, function () { if (funccancel) funccancel(); })
}
$.modalAlert = function (content, type) {
    var icon = 0;
    switch (type) {
        case 'success':
            icon = 1;// "fa-check-circle";
            break;
        case 'error':
            icon = 2;// "fa-times-circle";
            break;
        case 'warning':
            icon = 0;// "fa-exclamation-circle";
            break;
        default:
            icon = 0;//"fa-info-circle";
            break;
    }

    top.layer.alert(content, {
        icon: icon,
        title: "系统提示",
        btn: ['确认'],
        btnclass: ['btn btn-primary'],
    });
}
$.alertMsg = function (msg) {
    layer.alert(msg, { title: '提示' });
}
$.alertError = function (msg) {
    layer.alert(msg, { icon: 2, title: '失败错误' });
}
$.alertWarning = function (msg) {
    layer.alert(msg, { icon: 0, title: '警告' });
}
$.alertOK = function (msg) {
    layer.alert(msg, { icon: 1, title: '成功提示' });
}
$.msg = $.msgok = function (msg) {
    layer.msg(msg);
}
$.err = $.error = function (msg) {
    console.log(msg);//如果错误则会记录下这个错误到日志
    layer.msg(msg, {
        icon: "-err", skin: 'ah-layer-err'
        //, time: 500000

    });
}
$.ok = function (msg) {
    layer.msg(msg, { icon: "-ok", skin: 'ah-layer-ok' });
}
//根据屏幕大小，自适应，判定宽度打开弹出层
$.open = function (opt) {
    //判断品目是否是小屏幕
    var vw = document.body.clientWidth;
    var ism = vw < 801;//设定视口宽度为800的为移动端
    var h = $(document).height();
    var w = $(document).width();
    if (!opt.area) {
        //不指定宽高
        if (vw > 0 && vw <= 800) {
            //Pad竖屏
            opt.area = [w + "px", h + "px"]
        } else if (vw > 800 && vw <= 1280) {
            //Pad横屏
            opt.area = [w * 0.9 + "px", h * 0.9 + "px"]
        } else {
            //PC
            opt.area = opt.area || [w * 0.86 + "px", h * 0.96 + "px"]
        }
    } else {
        //不指定宽高
        if (vw > 0 && vw <= 800) {
            //Pad竖屏
            opt.area = ["100%", "100%"]
        }
        //指定宽高
        else opt.area = opt.area || [w * 0.86 + "px", h * 0.96 + "px"];
    }
    opt.skin = opt.skin || 'ah-layer-open';
    opt.resize = false;
    if (!opt.title) {
        opt.title = false
    }
    return top.layer.open(opt);
}
$.openAdd = function (opt) {
    var handle = $.open({
        type: 2, title: opt.title || '添加',
        content: opt.content,//"/Code/CustomerEdit?op=NEWF",
        btn: ['添加', '取消'],
        yes: function (index, layero) {
            var win = top.window[layero.find('iframe')[0]['name']];
            win.submitForm({
                sendSuccess: function (jn) {
                    if (jn.rlt) {
                        opt.sendSuccessTrue && opt.sendSuccessTrue(jn, index);
                    } else { $.err("添加失败" + jn.msg); }
                },
                sendFailed: function () { $.err("传输数据失败！"); }
            });
        }
    });
}

$.openNav = function (opt) {
    var handle = $.open({
        type: 2, title: opt.title || '设置向导',
        content: opt.content,//"/Code/CustomerEdit?op=NEWF",
        btn: null
    });
}
$.thisLayerIndex = function () {
    var index = parent.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
    return index;
}
$.closeThisLayer = function (win) { //关闭本弹出层
    parent.layer.close(parent.layer.getFrameIndex(win.name));
}

//删除数据的框架
$.deleteData = function (opt) {
    opt.alertText = opt.alertText || "是否删除该数据？";
    $.confirm("删除确认", opt.alertText, function () {
        $.post(opt.url, opt.data, function (jn) {
            if (jn.rlt) opt.fnSuccess && opt.fnSuccess(jn);
            else $.err("删除错误:" + jn.msg);
        });
    });
}

$.revalidate = function ($form) {
    //重新做整理前端验证
    $.validator.unobtrusive.parse($form);
}

$.keydownEnter = function (event, func) {
    //检测按键按下后的信息        
    if (event.keyCode == 13 && func) func();
}
$.moreToggle = function ($id, that) {
    var $c = (typeof $id == "string") ? $($id) : $id;
    $c.toggle();

    $i = $(that).find("i.fa");
    if ($i.hasClass("fa-angle-double-down")) $i.removeClass("fa-angle-double-down").addClass("fa-angle-double-up");
    else if ($i.hasClass("fa-angle-double-up")) $i.removeClass("fa-angle-double-up").addClass("fa-angle-double-down");
    else if ($i.hasClass("fa-angle-double-right")) $i.removeClass("fa-angle-double-right").addClass("fa-angle-double-left");
    else if ($i.hasClass("fa-angle-double-left")) $i.removeClass("fa-angle-double-left").addClass("fa-angle-double-right");

    var txt = $(that).text();
    if (txt.indexOf('展开') >= 0) $(that).text(txt.replace('展开', '收起'));
    else if (txt.indexOf('收起') >= 0) $(that).text(txt.replace('收起', '展开'));

}
$.str = function (obj) {
    if (obj == undefined || obj == null) return "";
    else return obj.toString();
}

//线形图
$.ahECharts = function (op, dom) {
    // 基于准备好的dom，初始化echarts实例
    var myChart = echarts.init(document.getElementById(dom));
    // 指定图表的配置项和数据
    option = {
        title: {
            text: op.title,
            subtext: op.titleSmall,
            x: 'center',
        },
        tooltip: {
            trigger: 'axis'
        },
        legend: {
            data: ['最高气温', '最低气温'],
            left: 'center',
            bottom: "0"
        },
        xAxis: {
            type: 'category',
            boundaryGap: false,
            data: op.xAxisData
        },
        yAxis: {
            type: 'value',
            axisLabel: {
                formatter: '{value}' + op.yUnit
            }
        },
        series: op.series,
    };
    // 使用刚指定的配置项和数据显示图表。
    myChart.setOption(option);
}
//饼图
$.ahPieChart = function (op, dom) {
    // 基于准备好的dom，初始化echarts实例
    var myChart = echarts.init(document.getElementById(dom));
    // 指定图表的配置项和数据
    option = {
        title: {
            text: op.title,
            subtext: op.titleSmall,
            x: 'center'
        },
        tooltip: {
            trigger: 'item',
            formatter: "{a} <br/>{b} : {c} ({d}%)"
        },
        legend: {
            left: 'center',
            bottom: "0",
            data: op.legendData
        },
        series: [
            {
                name: op.seriesName,
                type: 'pie',
                radius: '55%',
                center: ['50%', '60%'],
                data: op.data,
                itemStyle: {
                    emphasis: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ]
    };

    // 使用刚指定的配置项和数据显示图表。
    myChart.setOption(option);
    return myChart;
}

//生成时间戳ID
$.returnDateID = function (str) {
    return str + "_" + (new Date()).valueOf();
}
//如果值为空 返回空 不返回null
$.returnNullText = function (val) {
    if (val == null) {
        return "";
    } else {
        return val
    }
}
//图片压缩与上传
$.localUpdataPic = function (_upFile, callBack) {
    _upFile.addEventListener("change", function () {
        if (_upFile.files.length === 0) {
            $.err("请选择图片");
            return;
        }
        var oFile = _upFile.files[0];

        if (!new RegExp("(jpg|jpeg|png)+", "gi").test(oFile.type)) {
            $.err("照片上传：文件类型必须是JPG、JPEG、PNG");
            return;
        }
        var index = layer.load(1, {
            shade: [0.7, '#000'] //0.1透明度的白色背景
        });
        var reader = new FileReader();
        reader.onload = function (e) {
            var base64Img = e.target.result;
            //压缩前预览
            console.log("原始图片SIZE=" + (oFile.size / 1024).toFixed(2) + 'KB');
            //--执行resize。  
            var _ir = ImageResizer({
                resizeMode: "auto"
                , dataSource: base64Img
                , dataSourceType: "base64"
                , maxWidth: AH_UPDATAPIC_SIZE_WIDTH //允许的最大宽度  
                , maxHeight: AH_UPDATAPIC_SIZE_HEIGHT //允许的最大高度。  
                , onTmpImgGenerate: function (img) {

                }
                , success: function (resizeImgBase64, canvas) {
                    console.log("处理过的图片SIZE=" + (resizeImgBase64.length * 0.8 / 1024).toFixed(2) + 'KB');
                    setTimeout(function () {
                        var opt = {
                            image_base64: resizeImgBase64,
                            file_name: oFile.name
                        }
                        //上传bese64
                        $.postJson("/openapi/Picture/UploadBase64", opt, function (jn) {
                            layer.close(index);
                            if (jn.rlt) {
                                //上传成功
                                callBack(jn);
                            } else {
                                $.err(jn.msg);
                            }
                        })
                    }, 100);


                }
                , debug: true
            });

        };
        reader.readAsDataURL(oFile);

    }, false);
}

