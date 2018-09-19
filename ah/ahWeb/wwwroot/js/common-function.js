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
            //$("head").append('<link rel="stylesheet" href="/v20/lib/my-widget/my-widget.css" />');
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
    if (!fmt) fmt = "yyyy-MM-dd HH:mm:ss";
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
                if(fmt) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
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
Date.prototype.formatDate = function () {
    return this.formatDateTime("yyyy-MM-dd");
}
