/*! XSidebar 0.0.1 */
/**
 * Created by Tezml on 2017/6/2
 * author : lixiaofeng
 */
(function ($) {
    $.fn.XSidebar = function (obj) {
        //局部变量
        var local = {
            selectRow:null,
        }
        //默认配置
        var defaults = {

        }
        var mySidebar = new myXSidebar(this, obj, local);
        if (obj && $.isEmptyObject(obj) == false) {
            mySidebar.options = $.extend(defaults, obj, local);
        } else {
            mySidebar.options = defaults;
        }
        //调用初始化方法
        mySidebar.init();
        return mySidebar;
    };
    //依赖构造函数
    var myXSidebar = function (ele, obj, local) {
        this.$element = ele;
        this.obj = obj;
        this.local = local;
    };
    myXSidebar.prototype = {
        init: function () {
            this.options.onLoaded();
            console.log("初始化XSidebar");
            console.log(this);
            //是否需要操作栏
            if (this.options.action) {
                this.addActionHtml(this.options.action);
            }
            //调用添加数据函数
            this.addHtmlData(this.options.items);
        },
        //添加HTML数据
        addHtmlData: function (data) {
            var s = this;
            data = data[s.options.dataParameter];
            //清除旧数据
            s.$element.html("");
            for (var i = 0; i < data.length; i++) {
                var d = data[i];
                var domWrap = $("<div class='XS-li'></div>");
                if (d[s.options.defaultsShow.key] == s.options.defaultsShow.value) {
                    //如果是默认选中 则高亮  并且把对应的对象存入到插件中 用于删除和编辑
                    domWrap.addClass("XS-li-active");
                    s.local.selectRow = d;
                }
                var c = $("<input type='checkbox'/>").hide();
                domWrap.append(c);
                var dom = $("<div class='XS-li-content'>" + d[s.options.nodeTitle] + "<span>[" + d[s.options.nodeRemark] + "]</span></div>");
                domWrap.append(dom);
                s.$element.append(domWrap);
                //点击
                dom.on("click", { index: i }, function (event) {
                    s.$element.find(".XS-li").each(function () {
                        $(this).removeClass("XS-li-active");
                    });
                    $(event.currentTarget).parent().addClass("XS-li-active");
                    var index = event.data.index;
                    s.local.selectRow = data[index];
                    s.options.onClick(data[index]);
                });
            }
        },
        //刷新数据 parameter：参数
        refresh: function (parameter) {
            var s = this;
            $.loadJSON(s.options.url, parameter, function (jn) {
                s.addHtmlData(jn);
            });
        },
    }
})(jQuery);