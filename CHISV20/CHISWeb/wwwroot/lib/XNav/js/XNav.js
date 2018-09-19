/*! XNav 0.0.1 */
/**
 * Created by Tezml on 2017/9/28
 * author : lixiaofeng
 */
(function ($) {
    $.fn.XNav = function (obj) {
        //局部变量
        var local = {

        }
        //默认配置
        var defaults = {

        }
        var myNav = new myXNav(this, obj, local);
        if (obj && $.isEmptyObject(obj) == false) {
            myNav.options = $.extend(defaults, obj, local);
        } else {
            myNav.options = defaults;
        }
        //调用初始化方法
        myNav.init();
        return myNav;
    };
    //依赖构造函数
    var myXNav = function (ele, obj, local) {
        this.$element = ele;
        this.obj = obj;
        this.local = local;
    };
    myXNav.prototype = {
        init: function () {
            var nav = this;
            //添加鼠标监听
            nav.isClickNax(nav);
            $(this.$element).find("a[href='javascript:void(0);']").each(function () {
                var _this = $(this);
                _this.on("click", function () {
                    $(nav.$element).find("ul").each(function () {
                        $(this).removeClass("ah-open");
                    });
                    nav.allAarent(_this);
                    _this.parent().first().find("ul").first().addClass("ah-open");
                })
            });
            //整体窗口大小改变
            $(window).resize(function () {
                $(nav.$element).find("ul").each(function () {
                    $(this).removeClass("ah-open");
                });
                $(".ah-nav-items").removeClass("ah-nav-response");
            });
        },
        //对当前展开的菜单父级添加CLASS
        allAarent: function (_this) {
            var nav = this;
            var ul = _this.parents("ul").first();
            if (ul.attr("ah-nav-level") != "1") {
                ul.addClass("ah-open");
                nav.allAarent(ul);
            }
        },
        //判断鼠标是否点击了导航区域
        isClickNax: function (nav) {
            $(document).click(function (e) {
                e = window.event || e;
                var obj = e.srcElement || e.target;
                if ($(obj).parents(".ah-nav-items").length == 0 ) {
                    $(nav.$element).find("ul").each(function () {
                        $(this).removeClass("ah-open");
                    });
                }
            });
        }
    }
})(jQuery);