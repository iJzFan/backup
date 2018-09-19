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
            },
            success: function (data, textStatus) {
                //成功回调方法增强处理  
                fn.success(data, textStatus);
            },
            beforeSend: function (XHR) {
                //提交前回调方法  
                $('body').append("<div id='ajaxInfo' style=''><img style='width: 30px;margin-right: 10px;' src='../../images/loading-tianshi.gif'/>正在加载,请稍后...</div>");
            },
            complete: function (XHR, TS) {
                //请求完成后回调函数 (请求成功或失败之后均调用)。  
                $("#ajaxInfo").remove();;
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





// 询问层
$.confirm = function (title, htmlcontent, funcok, funccancel) {
    var index = layer.confirm(htmlcontent, {
        title: title, skin: 'ah-myskin-blue',
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
//根据屏幕大小，自适应，判定宽度打开弹出层
$.open = function (opt) {
    //判断品目是否是小屏幕
   var vw= document.body.clientWidth; 
   var ism = vw <= 800;//设定视口宽度为800的为移动端
   if (ism) {
       opt.title = false;
       opt.area = ['100%', '100%'];
       closeBtn = 1;
   }
   return layer.open(opt);
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

/*
 * post 数据data，然后打开连接地址
 */
$.postPage = function (link, data, bFrame) {

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

//生成select选择项
$.addSelectOption = function (opt) {
    var html = "";
    if (opt.items.length <= 0) {
        html = "<option value=''>暂无数据</option>";
    } else if (opt.items.length > 0) {
        html = "<option value=''>" + opt.msg + "</option>";
        for (var i = 0; i < opt.items.length; i++) {
            html += "<option ";
            //如果默认只有一个 则自动选上
            if (opt.items.length == 1) {
                html += "selected='selected'";
            }
            html += "value='" + opt.items[i][opt.valKey] + "'>" + opt.items[i][opt.textKey] + "</option>";
        }
    }
    $(opt.dom).html(html);
}

//post json请求封装
$.postJson = function (url, data, success) {
    $.ajax({
        type: 'POST',
        url: url,
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        success: function (jn) {
            success(jn)
        },
    });
}