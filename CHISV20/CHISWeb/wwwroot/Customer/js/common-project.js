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
$.msg = function (msg, sets) {
    layer.msg(msg, $.extend(sets, { icon: 1, time: 1000 }));
}
$.form2Json = function (selector) {
    var d = {};
    var t = $(selector).serializeArray();
    $.each(t, function () { 
        d[this.name] = this.value;
    });
    return d;
}
$.loadUrl = function (url,top) {
    if (top) top.window.location.href = url;
    else window.location.href = url;
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


/*template扩展部分*/
template.helper("formatMoney", function (x) {
    return "￥" + x.toFixed(2);
});
template.helper("jsonString", function (jn) {
    return JSON.stringify(jn);
});

function loadTemplateHtml(templateId, jn) {
    if (!jn) return "";
    var render = template.compile($('#' + templateId).html());
    return render(jn);
}