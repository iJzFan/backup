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
            if (es[i][js ? 'src' : 'href'].indexOf(name) != -1) return true;
        return false;
    },
    isPhoneNo: function (val) {
        return /^1[34578]\d{9}$/.test(val);
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
            //$("head").append('<link rel="stylesheet" href="/Customer/lib/myWidget/myWidget.css" />');
            var head = document.getElementsByTagName('head')[0];
            var link = document.createElement('link');
            link.href = jsPath + cssfile;
            link.rel = "stylesheet";
            head.appendChild(link);
        }
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
