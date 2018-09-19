/*
 *   项目所用的公共函数
 *   如果你觉得该函数具有公用价值，则放在此js文件下面
 */
/*======================== 重写Jquery ==================== START　======================*/
(function ($) {
    //首先备份下jquery的ajax方法  
    var _ajax = $.ajax;

    //重写jquery的ajax方法  
    $.ajax = function (opt) {
        opt = $.extend({ bAccess: true }, opt);
        //备份opt中error和success方法  
        var fn = {
            error: function (XMLHttpRequest, textStatus, errorThrown) { },
            success: function (data, textStatus) { }
        }
        if (opt.error) {
            fn.error = opt.error;
        }
        if (opt.success) {
            fn.success = opt.success;
        }

        //扩展增强处理  
        var _opt = $.extend(opt, {
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                //错误方法增强处理  
                fn.error(XMLHttpRequest, textStatus, errorThrown);
                if (opt.bAccess) {
                    if (XMLHttpRequest.status == 401) { top.window.location = "/"; }
                }
                if (XMLHttpRequest.status == 0) $.err("网络不通啊");
                $(".ajax-info").remove();
            },
            success: function (data, textStatus) {
                //成功回调方法增强处理  
                fn.success(data, textStatus);
            },
            beforeSend: function (XHR) {
                //提交前回调方法  
                if (opt.bSilence) return;
                //$('body').append("<div class='ajax-info'><img style='width: 30px;margin-right: 10px;' src='../../images/loading-tianshi2.gif'/>加载...</div>");
                $('body').append("<div class='ajax-info'><i class='fa fa-spinner fa-spin'></i>加载...</div>");
            },
            complete: function (XHR, TS) {
                //请求完成后回调函数 (请求成功或失败之后均调用)。  
                $(".ajax-info").remove();

            }
        });
        return _ajax(_opt);
    };




})(jQuery);

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
            // layer.close(index), layer.close(index);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (XMLHttpRequest.status == 401) {
                top.window.location = "/";
                //top.window.location.reload();
            }
        }
    });
}


/*======================== 重写Jquery ==================== END ======================*/

//

//$.datetimepicker.setDefaults({
//    language: 'zh-CN',
//    format: 'yyyy-mm-dd'
//}
//);








$.confirm = function (title, htmlcontent, funcok, funccancel) {
    /// <author> Rex 2017-3-1 </author>
    /// <summary>公共Confirm确认弹窗</summary>     
    /// <param name="title" type="String">弹窗的标题</param>    
    /// <param name="htmlcontent" type="String">弹窗提示内容Html或者Text格式</param>   
    /// <param name="funcok" type="Function">确定返回函数</param>   
    /// <param name="funccancel" type="Function">取消返回函数</param> 

    //bootbox.setDefaults("locale", "zh_CN");  //弹窗设置中文             
    //bootbox.confirm("没有该用户，是否跳到注册", function (result) {
    //    if (result) { location.href = "/Customer/Reservation/Register" }
    //});

    var index = layer.confirm(htmlcontent, {
        title: title, icon: 3
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

layer.msgok = function (msg) {
    layer.msg(msg);
}

$.msg = function (msg) {
    layer.msg(msg);
}
$.err = function (msg) {
    layer.msg(msg, { icon: 2, skin: 'ah-layer-err' });
}
$.ok = function (msg) {
    layer.msg(msg, { icon: 1, skin: 'ah-layer-ok' });
}

//根据屏幕大小，自适应，判定宽度打开弹出层
$.open = function (opt) {
    //判断品目是否是小屏幕
    var vw = document.body.clientWidth;
    var ism = vw < 961;//设定视口宽度为960的为移动端
    if (ism) {
        opt.title = false;
        opt.area = ['100%', '100%'];
        closeBtn = 1;
    }
    return layer.open(opt);
}



$.form2Json = function (selector) {
    var d = {};
    var t = $(selector).serializeArray();
    $.each(t, function (i, m) {
        if (typeof d[this.name] === "undefined") d[this.name] = this.value;//禁止重复写入数值，因为checkbox会有2个数值
    });
    return d;
}
$.loadUrl = function (url, top) {
    if (top) top.window.location.href = url;
    else window.location.href = url;
}

$.setReadOnly = function () {
    $('input[type=text],input[type=number],input[type=checkbox],input[type=password],input[type=email],input[type=hidden],select,textarea').attr("readonly", "readonly");
}


$.getImgPath = function (root, name) {

    if (name && name.length > 0 && name.toLowerCase().indexOf("http") >= 0) { return name; }
    else return root + name;
}

$.peopleGender = function (gender) {
    if (gender == "0") return "女";
    else if (gender == "1") return "男";
    else return "其他"
}
//规范数据的name为规范集合排序
$.SortFormArray = function ($cs) {
    var num = $cs.length;
    var index = 0;
    var reg = /\[\d*\]/;
    $cs.each(function (i, item) {
        $(item).find(":input").each(function (index, m) {
            $m = $(m);
            if (reg.test($m.attr("name"))) {
                var newName = $m.attr("name").replace(reg, "[" + i + "]");
                $m.attr("name", newName);
            }
        });
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


/*template扩展部分*/
template.helper("formatMoney", function (x) {
    return ("￥" + x.toFixed(2)).trim();
});
template.helper("jsonString", function (jn) {
    return JSON.stringify(jn);
});
template.helper("imgUrl", function (root, name) {
    return $.getImgPath(root, name);
});




function loadTemplateHtml(templateId, jn) {
    if (!jn) return "";
    var render = template.compile($('#' + templateId).html());
    var rtn = "" + render(jn);
    return rtn.trim();
}






//扩展验证规则
$.extend($.validator.messages, {
    required: "必填",
    remote: "请修正该字段",
    email: "电子邮件格式不正确",
    url: "网址格式不正确",
    date: "日期格式不正确",
    dateISO: "请输入合法的日期 (ISO).",
    number: "请输入数字",
    digits: "只能输入整数",
    creditcard: "请输入合法的信用卡号",
    equalTo: "请再次输入相同的值",
    accept: "请输入拥有合法后缀名的字符",
    maxlength: $.validator.format("请输入一个 长度最多是 {0} 的字符"),
    minlength: $.validator.format("请输入一个 长度最少是 {0} 的字符"),
    rangelength: $.validator.format("请输入 一个长度介于 {0} 和 {1} 之间的字符"),
    range: $.validator.format("请输入一个介于 {0} 和 {1} 之间的值"),
    max: $.validator.format("请输入一个最大为{0} 的值"),
    min: $.validator.format("请输入一个最小为{0} 的值")
});
//手机验证规则  
jQuery.validator.addMethod("mobile", function (value, element) {
    var mobile = /^1[3|4|5|7|8]\d{9}$/;
    return this.optional(element) || (mobile.test(value));
}, "手机格式不对");

/*通用打印函数*/
function Print(preProcFunc, actionName, args) {
    if (preProcFunc) {
        preProcFunc(printproc);
    } else { printproc() }

    function printproc() {
        var url = "/Print/" + actionName + "?1=1&" + args;
        top.$.open({
            type: 2,
            title: '打印预览',
            area: ['80%', '90%'],
            content: url
        });
    }

}

$.getJson = function (url, data, func) {
    $.ajax({
        bSilence:true,
        url: url,
        data:data,
        datatype: "json",
        type: 'get',
        success: func
    });
}