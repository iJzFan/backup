/*
 *   项目所用的公共函数
 *   如果你觉得该函数具有公用价值，则放在此js文件下面
 */


//

//$.datetimepicker.setDefaults({
//    language: 'zh-CN',
//    format: 'yyyy-mm-dd'
//}
//);








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

$.loadJSON = function (url) {
    var data, func;
    if (typeof arguments[1] == "object") { data = arguments[1]; }
    if (typeof arguments[1] == "function") { data = null; func = arguments[1]; }
    if (typeof arguments[2] == "function") { func = arguments[2]; }
    // var index = layer.load(2, { shade: false }); //0代表加载的风格，支持0-2 
    $.ajax({
        type: "post",
        url: url,
        data: data,
        dataType: "json",
        success: function (jn) {
            func(jn);
            // layer.close(index),  layer.close(index);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (XMLHttpRequest.status == 401) {
                top.window.location = "/";
                //top.window.location.reload();
            }
        }
    });
}
$.fn.addMaskUnuse = function () {
    $(this).removeMaskUnuse();
    //添加一个遮罩层
    var $mk = $("<i class='ah-show-unuse'></i>");
    var $sk = $("<s class='clearfix ah-show-fix'></s>");
    $(this).append($sk);
    $mk.css("height", $(this).height());
    $(this).append($mk);
}
$.fn.removeMaskUnuse = function () {
    //移除遮罩层
    $(this).find("i.ah-show-unuse").remove();
    $(this).find("s.ah-show-fix").remove();
}


$.fn.bindSelect = function (options) {
    var defaults = {
        id: "id",
        text: "text",
        search: false,
        initialValue: null,
        url: "",
        param: [],
        change: null,
        placeholder: "请选择父级菜单",
        allowClear: true,
        onLoaded: null //载入成功
    };
    var options = $.extend(defaults, options);
    var that = this;
    var $element = $(this);
    if (options.url != "") {
        $.ajax({
            url: options.url,
            data: options.param,
            dataType: "json",
            async: false,
            success: function (data) {
                $.each(data, function (i) {
                    $element.append($("<option></option>").val(data[i][options.id]).html(data[i][options.text]));
                });
                $element.val(options.initialValue || "");
                $element.select2({
                    minimumResultsForSearch: options.search == true ? 0 : -1,
                    placeholder: options.placeholder,
                    allowClear: options.allowClear
                });
                $element.on("change", function (e) {
                    if (options.change != null) {
                        options.change(data[$(this).find("option:selected").index()]);
                    }
                    $("#select2-" + $element.attr('id') + "-container").html($(this).find("option:selected").text().replace(/　　/g, ''));
                });

                if (options.onLoaded) options.onLoaded.apply(that);
            }
        });
    } else {
        $element.select2({
            minimumResultsForSearch: -1
        });
    }
    return $element;
}




$.request = function (name) {
    var search = location.search.slice(1);
    var arr = search.split("&");
    for (var i = 0; i < arr.length; i++) {
        var ar = arr[i].split("=");
        if (ar[0] == name) {
            if (unescape(ar[1]) == 'undefined') {
                return "";
            } else {
                return unescape(ar[1]);
            }
        }
    }
    return "";
}


$.reload = function () {
    location.reload();
    return false;
}



/*打开一个模式窗体*/
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
$.modalOpen = function (options) {
    var defaults = {
        id: "mw_" + (new Date().getTime()),
        title: '系统窗口',
        width: "100px",
        height: "100px",
        url: '',
        shade: 0.3,
        btn: ['确认', '关闭'],
        btnclass: ['btn btn-primary', 'btn btn-warning'],
        yesThenAutoClose: true,
        isShowRefresh: true,
        yes: null
    };
    var options = $.extend(defaults, options);
    var _width = top.$(window).width() > parseInt(options.width.replace('px', '')) ? options.width : top.$(window).width() + 'px';
    var _height = top.$(window).height() > parseInt(options.height.replace('px', '')) ? options.height : top.$(window).height() + 'px';
    var layerIndex = top.layer.open({
        id: options.id,
        type: 2,
        shade: options.shade,
        title: options.title,
        fix: false,
        //shadeClose:true,
        area: [_width, _height],
        content: options.url,
        btn: options.btn,
        btnclass: options.btnclass,
        yes: function (index, layero) {
            options.yes && options.yes(index, layero, top.window[layero.find('iframe')[0]['name']]);
            options.yesThenAutoClose && top.layer.close(index); //如果设定了yes回调，需进行手工关闭
        }
    });
    /*添加刷新iframe窗体的按钮*/
    if (options.isShowRefresh) {
        var $layer = top.$('.layui-layer[times=' + layerIndex + ']');
        var $refresh = $("<a class='layui-layer-refresh' title='刷新内容'><i class='fa fa-refresh'></i></a>");
        $refresh.click(function () {
            $layer.find("iframe").get(0).contentWindow.location.reload(true);
        });
        $layer.find(".layui-layer-setwin").append($refresh);
    }
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
$.modalMsg = function (content, type) {
    if (type != undefined) {
        var icon = 0;
        if (type == 'success') {
            //icon = "fa-check-circle";
            icon = 1;
        }
        if (type == 'error') {
            //icon = "fa-times-circle";
            icon = 2;
        }
        if (type == 'warning') {
            //icon = "fa-exclamation-circle";
            icon = 0;
        }
        top.layer.msg(content, { icon: icon, time: 1000, shift: 5 });
        top.$(".layui-layer-msg").find('i.' + icon).parents('.layui-layer-msg').addClass('layui-layer-msg-' + type);
    } else {
        top.layer.msg(content);
    }
}
$.closeModalWin = function () {
    top.$('.layui-layer-shade').remove();
    top.$('.layui-layer').remove();
}
/*当前页面的窗体*/
$.currentWindow = function () {
    //var iframeId = top.$(".NFine_iframe:visible").attr("id");
    //return top.frames[iframeId];
    // return top.frames["frame_right"];
    return top.window;
}

$.modalConfirm = function (content, callBack) {
    top.layer.confirm(content, {
        icon: "fa-infor-circle",   //"fa-exclamation-circle",
        title: "系统提示",
        btn: ['确认', '取消'],
        btnclass: ['btn btn-primary', 'btn btn-warning'],
    }, function () {
        $.closeModalWin();
        callBack(true);
    }, function () {
        callBack(false)
    });
}
$.deleteForm = function (options) {
    var defaults = {
        prompt: "注意：您确定要删除该项数据吗？",
        url: "",
        param: [],
        loading: "正在删除数据...",
        success: null,
        close: true
    };
    var options = $.extend(defaults, options);
    if ($('[name=__RequestVerificationToken]').length > 0) {
        options.param["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    }
    $.modalConfirm(options.prompt, function (r) {
        if (r) {
            $.loading(true, options.loading);
            window.setTimeout(function () {
                $.ajax({
                    url: options.url,
                    data: options.param,
                    type: "post",
                    dataType: "json",
                    success: function (data) {
                        if (data.state == "success") {
                            options.success(data);
                            $.modalMsg(data.message, data.state);
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
    });

}



//提交数据
$.submitData = function () {
    /*
     * url       string      传送地址
     * urlpms    string k=v  get传送数据对
     * postData  json/kv     post传送数据
     * jqGridId  string      jqGrid的ID
     */
    var pms = null;
    var url, urlpms, postData, jqGridId;

    var arg = arguments[0];
    if (arg) {
        var a = typeof (arg);
        if (a == "string") url = arg;
        if (a == "object") pms = arg;
    }
    arg = arguments[1];
    if (arg) {
        var a = typeof (arg);
        if (a == "string") urlpms = arg;
        if (a == "object") postData = arg;
    }
    arg = arguments[2];
    if (arg) {
        var a = typeof (arg);
        if (a == "string") jqGridId = arg;
        if (a == "object") postData = arg;
    }
    arg = arguments[3];
    if (arg) {
        var a = typeof (arg);
        if (a == "string") jqGridId = arg;
    }

    if (pms) {
        url = pms.url || '';
        urlpms = pms.urlpms || '';
        postData = pms.postData || {};
        jqGridId = pms.jqGridId || '';
    }

    $.submitForm({
        url: url + "?" + (urlpms || ''),
        param: postData,
        success: function () {
            $.currentWindow().$("#" + jqGridId).trigger("reloadGrid");
        }
    });
}






/*--------- jqgrid 配合layer开发的一般设置样式 ---- START -------------*/
$.fn.dataGrid = function (options) {
    var defaults = {
        datatype: "json",
        autowidth: true,
        rownumbers: true,
        shrinkToFit: true,
        gridview: true
    };
    var options = $.extend(defaults, options);
    var $element = $(this);
    options["onSelectRow"] = function (rowid) {
        var length = $(this).jqGrid("getGridParam", "selrow").length;
        var $operate = $(".operate");
        if (length > 0) {
            $operate.animate({ "left": 0 }, 400);
        } else {
            $operate.animate({ "left": '-400.1%' }, 400);
        }
        $operate.find('.close').click(function () {
            $operate.animate({ "left": '-400.1%' }, 400);
        })
    };
    $element.jqGrid(options);
};
$.fn.jqGridRowValue = function () {
    var $grid = $(this);
    var selectedRowIds = $grid.jqGrid("getGridParam", "selarrrow");
    if (selectedRowIds != "") {
        var json = [];
        var len = selectedRowIds.length;
        for (var i = 0; i < len; i++) {
            var rowData = $grid.jqGrid('getRowData', selectedRowIds[i]);
            json.push(rowData);
        }
        return json;
    } else {
        return $grid.jqGrid('getRowData', $grid.jqGrid('getGridParam', 'selrow'));
    }
}
function CommonEdit(pms) {
    return {
        InitialJqGrid: function (options) {
            var defaults = {
                url: pms.jqurl,
                datatype: "json", //数据来源，本地数据
                mtype: "POST",//提交方式
                autowidth: true,//自动宽
                shrinkToFit: true,//自动适应宽度
                rownumbers: true,//添加左侧行号
                rowNum: 50,//每页显示记录数
                rowList: [50, 100, 200],//用于改变显示行数的下拉列表框的元素数组。
                pager: $('#' + pms.jqGridPager),
                viewrecords: true, //是否在浏览导航栏显示记录总数
                sortable: true,    //是否支持网格排序
            };
            var options = $.extend(defaults, options);
            $("#" + pms.jqGridId).dataGrid(options);
        },

        Add: function (callback, urlpms) {
            $.modalOpen({
                id: pms.id || "Form",
                title: "新增" + pms.title,
                url: pms.url + "?op=NEWF&" + urlpms,
                width: pms.width,
                height: pms.height,
                btn: pms.btn || null,
                btnclass: pms.btnclass,
                yesThenAutoClose: false,
                yes: function (index, layero, win) {
                    callback && callback(index, win);
                }
            });
        },
        Modify: function (urlpms, callback, options) {
            $.modalOpen({
                id: pms.id || "Form",
                title: "修改" + pms.title,
                url: pms.url + "?op=MODIFYF&" + urlpms,
                width: pms.width,
                height: pms.height,
                btn: pms.btn || null,
                btnclass: pms.btnclass,
                yesThenAutoClose: false,
                yes: function (index, layero, win) {
                    callback && callback(index, win);
                }
            });
            var options = $.extend({ bOK: true }, options);
            if (options.bOK == false) $('.layui-layer-btn0').hide();
        },
        Delete: function (urlpms, param) {
            $.deleteForm({
                url: pms.url + "?op=DELETE&" + urlpms,
                param: param,
                prompt: pms.deletePrompt||"是否删除该笔资料",
                success: function () {
                    $.currentWindow().$("#" + pms.jqGridId).trigger("reloadGrid");
                }
            })
        },
        View: function (urlpms) {
            $.modalOpen({
                id: pms.id || "Form",
                title: "查阅" + pms.title,
                url: pms.url + "?op=VIEW&" + urlpms,
                width: pms.width,
                height: pms.height,
                btn: null,
                //btnclass: ['btn btn-hidden', 'btn btn-warning']
            });
            $('.layui-layer-btn0').hide();
        },
        JqGridId: pms.jqGridId,
        JqGridPager: pms.jqGridPager,
        $jqGrid: $("#" + pms.jqGridId),
        $jqGridPager: $('#' + pms.jqGridPager),
        $jqGridReload: function (postData) {
            $("#" + pms.jqGridId).jqGrid('setGridParam', {
                postData: postData
            }).trigger('reloadGrid');
        },
        reload: function (postdata) {
            var $tbName = $("#" + pms.jqGridId);
            $tbName.jqGrid("clearGridData");
            //重新设置jqgrid
            $tbName.jqGrid('setGridParam', {
                //发送数据
                postData: postdata,
                page: 1,
                //该方法是加载完
                loadComplete: function (xhr) {
                    var rowNum = $tbName.jqGrid('getGridParam', 'records');
                    if (!rowNum) {
                        if ($("#norecords").html() == null) {
                            $tbName.parent().append(" <div id='norecords'>没有查询记录！</div> ");
                        }
                        $("#norecords").show();
                    } else {//如果存在记录，则隐藏提示信息。
                        $("#norecords").hide();
                    }
                }
            }).trigger("reloadGrid");//重新载入
        }
    }
}


/*----------jqgrid 配合layer开发的一般设置样式 ----- END -----------*/








/*================================= 窗体框架 ===========================================*/


/*关闭一个模式窗体*/
$.modalClose = function () {
    var index = top.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
    var $IsdialogClose = top.$("#layui-layer" + index).find('.layui-layer-btn').find("#IsdialogClose");
    var IsClose = $IsdialogClose.is(":checked");
    if ($IsdialogClose.length == 0) {
        IsClose = true;
    }
    if (IsClose) {
        top.layer.close(index);
    } else {
        location.reload();
    }
}
$.TiggerOKEvent = function () {
    var index = top.layer.getFrameIndex(window.name); //先得到当前iframe层的索引 
    top.$('#layui-layer' + index).find(".layui-layer-btn .layui-layer-btn0").click();
}



/*获取数据*/
$.fn.formSerialize = function (formdate) {
    var element = $(this);
    if (!!formdate) {
        for (var key in formdate) {
            var $id = element.find('#' + key);
            var value = $.trim(formdate[key]).replace(/&nbsp;/g, '');
            var type = $id.attr('type');
            if ($id.hasClass("select2-hidden-accessible")) {
                type = "select";
            }
            switch (type) {
                case "checkbox":
                    if (value == "true" || value == "True" || value == "Y") {
                        $id.attr("checked", 'checked');
                    } else {
                        $id.removeAttr("checked");
                    }
                    break;
                case "select":
                    $id.val(value).trigger("change");
                    break;
                default:
                    $id.val(value);
                    break;
            }
        };
        return false;
    }
    var postdata = {};
    element.find('input').each(function (r) {
        var $this = $(this);
        var name = $this.attr('name');
        var type = $this.attr('type');
        if (name) {
            switch (type) {
                case "checkbox":
                    postdata[name] = $this.is(":checked") ? "true" : "false";
                    break;
                case "hidden": //因chekbox会自己产生一个type为hidden，ID为空的隐藏标签，所以要去除
                    if ($this.attr('id')) postdata[name] = $this.val();
                    break;
                case "reset": break;
                case "submit": break;
                default:
                    postdata[name] = $this.val();
                    break;
            }
        }
    });
    element.find('select').each(function (r) {
        var $this = $(this);
        var name = $this.attr('name');
        var value = $this.val();
        postdata[name] = value;
    });
    element.find('textarea').each(function (r) {
        var $this = $(this);
        var name = $this.attr('name');
        var value = $this.val();
        postdata[name] = value;
    });

    if ($('[name=__RequestVerificationToken]').length > 0) {
        postdata["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    }
    return postdata;
};
/*将form转Json*/
$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name]) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

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








$.msg = function (msg, sets) {
    top.layer.msg(msg, $.extend(sets, { icon: 1, time: 1000 }));
}
$.msgError = function (msg, sets) {
    top.layer.msg(msg, $.extend(sets, { icon: 2, time: 1500 }));
}


//============================= 自己定义自己的post ====================================
$.postx = function (url) {
    var data, func, datatype = "html";
    if (typeof arguments[1] == "object") { data = arguments[1]; }
    if (typeof arguments[1] == "function") { data = null; func = arguments[1]; }
    if (typeof arguments[2] == "function") { func = arguments[2]; }
    if (typeof arguments[2] == "string") { datatype = arguments[2]; }
    if (typeof arguments[3] == "string") { datatype = arguments[3]; }

    $.ajax({
        type: "post",
        url: url,
        data: data,
        dataType: datatype,
        success: func,
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (XMLHttpRequest.status == 401) window.location.reload();
        }
    });
}
$.fn.loadx = function (url) {
    var data, func, datatype = "html";
    if (typeof arguments[1] == "object") { data = arguments[1]; }
    if (typeof arguments[1] == "function") { data = null; func = arguments[1]; }
    if (typeof arguments[2] == "function") { func = arguments[2]; }
    if (typeof arguments[2] == "string") { datatype = arguments[2]; }
    if (typeof arguments[3] == "string") { datatype = arguments[3]; }

    _this = this;
    $.ajax({
        type: "post",
        url: url,
        data: data,
        dataType: datatype,
        success: function (html) {
            _this.html(html);
            func(html);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (XMLHttpRequest.status == 401) window.location.reload();
            //  alert(textStatus);
            //  
        }
    });
}
$.getJSONx = function (url) {
    var data, func;
    if (typeof arguments[1] == "object") { data = arguments[1]; }
    if (typeof arguments[1] == "function") { data = null; func = arguments[1]; }
    if (typeof arguments[2] == "function") { func = arguments[2]; }
    $.ajax({
        type: "post",
        url: url,
        data: data,
        dataType: "json",
        success: function (jn) {
            func(jn);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (XMLHttpRequest.status == 401) window.location.reload();
            //  alert(textStatus);
            //  
        }
    });
}


/*=========================== 页面层面常用函数的再次封装 ================================*/
$.pg = {
    modalOpen: function (src, title) {
        $.modalOpen({
            url: src,
            title: title,
            width: "80%", height: "80%", btn: null
        });
    },
    modalDialog: function (src, title, yesfunc) {
        $.modalOpen({
            url: src,
            title: title,
            width: "80%", height: "80%",
            yes: yesfunc
        });
    }
}





/*=============================== 扩展对象的函数 =====================================*/
String.prototype.format = function (args) {
    /*两种调用方式
     * 
var template1="我是{0}，今年{1}了";
var template2="我是{name}，今年{age}了";
var result1=template1.format("loogn",22);
var result2=template1.format({name:"loogn",age:22});
//两个结果都是"我是loogn，今年22了" 
     */
    if (arguments.length > 0) {
        var result = this;
        if (arguments.length == 1 && (typeof (args) == "object" && args != null)) {
            for (var key in args) {
                var reg = new RegExp("({" + key + "})", "g");
                result = result.replace(reg, args[key]);
            }
        }
        else {
            for (var i = 0; i < arguments.length; i++) {
                var rv = arguments[i];
                if (rv == undefined || rv == null) {
                    rv = "";
                }

                var reg = new RegExp("({[" + i + "]})", "g");
                result = result.replace(reg, rv);

            }
        }
        return result;
    }
    else {
        return this;
    }
}

Date.prototype.formatDateTime = function (fmt) {
    /** * 对Date的扩展，将 Date 转化为指定格式的String * 月(M)、日(d)、12小时(h)、24小时(H)、分(m)、秒(s)、周(E)、季度(q)
        可以用 1-2 个占位符 * 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字) * eg: * (new
        Date()).pattern("yyyy-MM-dd hh:mm:ss.S")==> 2006-07-02 08:09:04.423      
     * (new Date()).pattern("yyyy-MM-dd E HH:mm:ss") ==> 2009-03-10 二 20:09:04      
     * (new Date()).pattern("yyyy-MM-dd EE hh:mm:ss") ==> 2009-03-10 周二 08:09:04      
     * (new Date()).pattern("yyyy-MM-dd EEE hh:mm:ss") ==> 2009-03-10 星期二 08:09:04      
     * (new Date()).pattern("yyyy-M-d h:m:s.S") ==> 2006-7-2 8:9:4.18      
     */
    var o = {
        "M+": this.getMonth() + 1, //月份         
        "d+": this.getDate(), //日         
        "h+": this.getHours() % 12 == 0 ? 12 : this.getHours() % 12, //小时         
        "H+": this.getHours(), //小时         
        "m+": this.getMinutes(), //分         
        "s+": this.getSeconds(), //秒         
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度         
        "S": this.getMilliseconds() //毫秒         
    };
    var week = {
        "0": "/u65e5",
        "1": "/u4e00",
        "2": "/u4e8c",
        "3": "/u4e09",
        "4": "/u56db",
        "5": "/u4e94",
        "6": "/u516d"
    };
    if (/(y+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    if (/(E+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, ((RegExp.$1.length > 1) ? (RegExp.$1.length > 2 ? "/u661f/u671f" : "/u5468") : "") + week[this.getDay() + ""]);
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(fmt)) {
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        }
    }
    return fmt;
}


$.extend($.jgrid.defaults, {
    loadError: function (xhr, status, error) {
        var txt = "";
        try {
            var $xhr = $(xhr.responseText);
            txt = $xhr.find(".stackerror").text();
        } catch (ex) { }
        $.alertError("载入数据错误:(" + status + ") <br>" + error + "<br>" + txt);
        if (xhr.status == 401) { top.window.reload(true); }
    }
});











