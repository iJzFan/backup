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
    //    if (result) { location.href = "/v20/Reservation/Register" }
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

$.loadJSON = function (url) {
    var data, func;
    if (typeof arguments[1] == "object") { data = arguments[1]; }
    if (typeof arguments[1] == "function") { data = null; func = arguments[1]; }
    if (typeof arguments[2] == "function") { func = arguments[2]; }
    var index = layer.load(2, { shade: false }); //0代表加载的风格，支持0-2 
    $.ajax({
        type: "post",
        url: url,
        data: data,
        dataType: "json",
        success: function (jn) {
            layer.close(index),func(jn),layer.close(index);
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

//邮箱 表单验证规则
jQuery.validator.addMethod("mail", function (value, element) {
    var mail = /^[a-z0-9._%-]+@([a-z0-9-]+\.)+[a-z]{2,4}$/;
    return this.optional(element) || (mail.test(value));
}, "邮箱格式不对");

//电话验证规则
jQuery.validator.addMethod("phone", function (value, element) {
    var phone = /^0\d{2,3}-\d{7,8}$/;
    return this.optional(element) || (phone.test(value));
}, "电话格式如：0371-68787027");

//区号验证规则  
jQuery.validator.addMethod("ac", function (value, element) {
    var ac = /^0\d{2,3}$/;
    return this.optional(element) || (ac.test(value));
}, "区号如：010或0371");

//无区号电话验证规则  
jQuery.validator.addMethod("noactel", function (value, element) {
    var noactel = /^\d{7,8}$/;
    return this.optional(element) || (noactel.test(value));
}, "电话格式如：68787027");

//手机验证规则  
jQuery.validator.addMethod("mobile", function (value, element) {
    var mobile = /^1[3|4|5|7|8]\d{9}$/;
    return this.optional(element) || (mobile.test(value));
}, "手机格式不对");

//邮箱或手机验证规则  
jQuery.validator.addMethod("mm", function (value, element) {
    var mm = /^[a-z0-9._%-]+@([a-z0-9-]+\.)+[a-z]{2,4}$|^1[3|4|5|7|8]\d{9}$/;
    return this.optional(element) || (mm.test(value));
}, "格式不对");

//电话或手机验证规则  
jQuery.validator.addMethod("tm", function (value, element) {
    var tm = /(^1[3|4|5|7|8]\d{9}$)|(^\d{3,4}-\d{7,8}$)|(^\d{7,8}$)|(^\d{3,4}-\d{7,8}-\d{1,4}$)|(^\d{7,8}-\d{1,4}$)/;
    return this.optional(element) || (tm.test(value));
}, "格式不对");

//年龄 表单验证规则
jQuery.validator.addMethod("age", function (value, element) {
    var age = /^(?:[1-9][0-9]?|1[01][0-9]|120)$/;
    return this.optional(element) || (age.test(value));
}, "不能超过120岁");
///// 20-60   /^([2-5]\d)|60$/

//传真
jQuery.validator.addMethod("fax", function (value, element) {
    var fax = /^(\d{3,4})?[-]?\d{7,8}$/;
    return this.optional(element) || (fax.test(value));
}, "传真格式如：0371-68787027");

//验证当前值和目标val的值相等 相等返回为 false
jQuery.validator.addMethod("equalTo2", function (value, element) {
    var returnVal = true;
    var id = $(element).attr("data-rule-equalto2");
    var targetVal = $(id).val();
    if (value === targetVal) {
        returnVal = false;
    }
    return returnVal;
}, "不能和原始密码相同");

//大于指定数
jQuery.validator.addMethod("gt", function (value, element) {
    var returnVal = false;
    var gt = $(element).data("gt");
    if (value > gt && value != "") {
        returnVal = true;
    }
    return returnVal;
}, "不能小于0 或空");

//汉字
jQuery.validator.addMethod("chinese", function (value, element) {
    var chinese = /^[\u4E00-\u9FFF]+$/;
    return this.optional(element) || (chinese.test(value));
}, "格式不对");

//指定数字的整数倍
jQuery.validator.addMethod("times", function (value, element) {
    var returnVal = true;
    var base = $(element).attr('data-rule-times');
    if (value % base != 0) {
        returnVal = false;
    }
    return returnVal;
}, "必须是发布赏金的整数倍");

//身份证
jQuery.validator.addMethod("idCard", function (value, element) {
    var isIDCard1 = /^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$/;//(15位)
    var isIDCard2 = /^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}([0-9]|X)$/;//(18位)

    return this.optional(element) || (isIDCard1.test(value)) || (isIDCard2.test(value));
}, "格式不对");


// 字符验证       
jQuery.validator.addMethod("stringCheck", function (value, element) {
    return this.optional(element) || /^[\u0391-\uFFE5\w]+$/.test(value);
}, "只能包括中文字、英文字母、数字和下划线");
//------------------------------------------------------------
    // 中文字两个字节       
    jQuery.validator.addMethod("byteRangeLength", function (value, element, param) {
        var length = value.length;
        for (var i = 0; i < value.length; i++) {
            if (value.charCodeAt(i) > 127) {
                length++;
            }
        }
        return this.optional(element) || (length >= param[0] && length <= param[1]);
    }, "请确保输入的值在3-15个字节之间(一个中文字算2个字节)");
//------------------------------------------------------------
    // 身份证号码验证       
    jQuery.validator.addMethod("isIdCardNo", function (value, element) {
        return this.optional(element) || isIdCardNo(value);
    }, "请正确输入您的身份证号码");
//------------------------------------------------------------
    // 手机号码验证       
    jQuery.validator.addMethod("isMobile", function (value, element) {
        var length = value.length;
        var mobile = /^[1][3-8]+\\d{9}/;
        return this.optional(element) || (length == 11 && mobile.test(value));
    }, "请正确填写您的手机号码");
//------------------------------------------------------------
    // 电话号码验证       
    jQuery.validator.addMethod("isTel", function (value, element) {
        var tel = /^\d{3,4}-?\d{7,9}$/;    //电话号码格式010-12345678   
        return this.optional(element) || (tel.test(value));
    }, "请正确填写您的电话号码");
//------------------------------------------------------------
    // 联系电话(手机/电话皆可)验证   
    jQuery.validator.addMethod("isPhone", function (value, element) {
        var length = value.length;
        var mobile = /^(((13[0-9]{1})|(15[0-9]{1}))+\d{8})$/;
        var tel = /^\d{3,4}-?\d{7,9}$/;
        return this.optional(element) || (tel.test(value) || mobile.test(value));

    }, "请正确填写您的联系电话");
//------------------------------------------------------------  
    // 邮政编码验证       
    jQuery.validator.addMethod("isZipCode", function (value, element) {

        var tel = /^[0-9]{6}$/;
        return this.optional(element) || (tel.test(value));
    }, "请正确填写您的邮政编码");    