/*! XAuto 0.0.1 */
/**
 * Created by Tezml on 2017/11/23
 * author : lixiaofeng
 */
(function ($) {
    $.fn.XAuto = function (obj) {
        //局部变量
        var local = {

        }
        //默认配置
        var defaults = {
            
        }
        var myAuto = new myXAuto(this, obj, local);
        if (obj && $.isEmptyObject(obj) == false) {
            myAuto.options = $.extend(defaults, obj, local);
        } else {
            myAuto.options = defaults;
        }
        //调用初始化方法
        myAuto.init();
        return myAuto;
    };
    //依赖构造函数
    var myXAuto = function (ele, obj, local) {
        this.$element = ele;
        this.obj = obj;
        this.local = local;
    };
    myXAuto.prototype = {
        init: function () {
            console.log("初始化XAuto");
            console.log(this);
        },
    }
})(jQuery);