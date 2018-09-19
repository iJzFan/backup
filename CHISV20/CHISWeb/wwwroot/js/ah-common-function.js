/*
 *   项目所用的公共函数
 *   如果你觉得该函数具有公用价值，则放在此js文件下面
 */

$.f = {
    isInclude: function (name) {
        /// <author> Rex 2017-3-1 </author>
        /// <summary>判断是否包含某js或者css文件</summary>   
        /// <param name="name" type="String">判断的文件</param>        
        /// <returns type="Boolean">返回有-true 无-false</returns> 
        var js = /js$/i.test(name);
        var es = document.getElementsByTagName(js ? 'script' : 'link');
        for (var i = 0; i < es.length; i++)
            if (es[i][js ? 'src' : 'href'].indexOf(name)!= -1) return true;
        return false;
    },
    isPhoneNo: function (val) {
        return /^(((13[0-9]{1})|(15[0-9]{1})|(17[0-9]{1})|(18[0-9]{1}))+\d{8})$/.test(val);
    },
    addCSSFile: function (filename) {

        var js = document.scripts;
        var jsPath;
        for (var i = js.length; i > 0; i--) {
            if (js[i - 1].src.indexOf(filename + ".js") > -1) {
                jsPath = js[i - 1].src.substring(0, js[i - 1].src.lastIndexOf("/") + 1);
            }
        }
        var cssfile = filename + ".css";
        if (!$.f.isInclude(cssfile)) {
            //$("head").append('<link rel="stylesheet" href="/Customer/lib/my-widget/myWidget.css" />');
            var head = document.getElementsByTagName('head')[0];
            var link = document.createElement('link');
            link.href = jsPath + cssfile;
            link.rel = "stylesheet";
            head.appendChild(link);
        }
    },
    toDtStr: function (dt, fmt) {
        try {
            var datetime = dt;
            if (typeof dt == "string") datetime = new Date(dt.replace(/-/g, "/").replace("T", ' '))
            return datetime.formatDateTime(fmt);
        } catch(ex){ return ""; }
    }, 
    onlyNumbersEvt: function (evt) {
        //只能输入数字
        evt = evt || window.event;
        var keynum;
        if (window.event) // IE        
            keynum = evt.keyCode
        else if (evt.which) // Netscape/Firefox/Opera        
            keynum = evt.which
         
        var c = String.fromCharCode(keynum); 
       // console.log("f.onlyNumberEvt:" +keynum);
        //小键盘
        switch (keynum) {
            case 32:
                var val = evt.target.value;
                setTimeout(function () {evt.target.value=val }, 200);     //空格屏蔽       
                return false;//空格
            case 48:
            case 96: c = "0"; break;
            case 49:
            case 97: c = "1"; break;
            case 50:
            case 98: c = "2"; break;
            case 51:
            case 99: c = "3"; break;
            case 52:
            case 100: c = "4"; break;
            case 53:
            case 101: c = "5"; break;
            case 54:
            case 102: c = "6"; break;
            case 55:
            case 103: c = "7"; break;
            case 56:
            case 104: c = "8"; break;
            case 57:
            case 105: c = "9"; break;
            case 8://退格
            case 9://tab
            case 46://删除
            case 110:
            case 190: return true;//小数点
            default:                return false;
        }
        return /[\d]/.test(c);
    },
    onlyNumAfterPaste: function (_this) {
        var v = _this.value;
        alert(v);
    },
    /*Url合并或者增加*/
    urlAppend: function (url, append) {
        if (url.indexOf('?') > 0) return url + "&" + append;
        else return url + "?" + append;
    },
    toPrice: function (num, n) {
        //转换为价格
        if (!num) return "";
        if (n == undefined) n = 2;
        return parseFloat(num).toFixed(n);
    },
    ver:1
}


Date.prototype.formatDateTime = function (fmt) { return formatDateTime(this, fmt); }
Date.prototype.formatDate = function (fmt) { return formatDate(this, fmt); }

Date.prototype.addDays = function (days) {
    /// <author> Rex 2017-3-8 </author>
    /// <summary>给日期增加天数</summary>   
    /// <returns type="Date">继续返回时间对象</returns> 
    var a = this.valueOf() + days * 24 * 60 * 60 * 1000
    return new Date(a);
}
 
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

//格式化日期
function formatDate(dt, fmt) { 
    if (!fmt) fmt = "yyyy-MM-dd";    
    return formatDateTime(dt,fmt);
}
function formatDateTime(dt, fmt) {

    if (dt === "" || dt == null || dt === undefined) return "";
    if (typeof dt == "string") dt = new Date(dt.replace("/-/g", "/"));

    /** * 对Date的扩展，将 Date 转化为指定格式的String * 月(M)、日(d)、12小时(h)、24小时(H)、分(m)、秒(s)、周(E)、季度(q)
       可以用 1-2 个占位符 * 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字) * eg: * (new
       Date()).pattern("yyyy-MM-dd hh:mm:ss.S")==> 2006-07-02 08:09:04.423      
    * (new Date()).pattern("yyyy-MM-dd E HH:mm:ss") ==> 2009-03-10 二 20:09:04      
    * (new Date()).pattern("yyyy-MM-dd EE hh:mm:ss") ==> 2009-03-10 周二 08:09:04      
    * (new Date()).pattern("yyyy-MM-dd EEE hh:mm:ss") ==> 2009-03-10 星期二 08:09:04      
    * (new Date()).pattern("yyyy-M-d h:m:s.S") ==> 2006-7-2 8:9:4.18      
    */
    var _this = dt;
    try { _this.getDate(); } catch (e) { return ""; }
    if (!fmt) fmt = "yyyy-MM-dd HH:mm:ss";
    var o = {
        "M+": _this.getMonth() + 1, //月份         
        "d+": _this.getDate(), //日         
        "h+": _this.getHours() % 12 == 0 ? 12 : _this.getHours() % 12, //小时         
        "H+": _this.getHours(), //小时         
        "m+": _this.getMinutes(), //分         
        "s+": _this.getSeconds(), //秒         
        "q+": Math.floor((_this.getMonth() + 3) / 3), //季度         
        "S": _this.getMilliseconds() //毫秒         
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
        fmt = fmt.replace(RegExp.$1, (_this.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    if (/(E+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, ((RegExp.$1.length > 1) ? (RegExp.$1.length > 2 ? "/u661f/u671f" : "/u5468") : "") + week[_this.getDay() + ""]);
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(fmt)) {
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        }
    }
    return fmt; 
}


function str(obj) {
    if (obj == null || obj == undefined || obj == "") return "";
    else return obj.toString();
}

 