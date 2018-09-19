/*! XPage 0.0.1 */
/**
 * Created by Tezml on 2017/6/19
 * author : lixiaofeng
 */
(function ($) {
    $.fn.XPage = function (obj) {
        //局部变量
        var local = {

        }
        //默认配置
        var defaults = {

        }
        var myPage = new myXPage(this, obj, local);
        if (obj && $.isEmptyObject(obj) == false) {
            myPage.options = $.extend(defaults, obj, local);
        } else {
            myPage.options = defaults;
        }
        //调用初始化方法
        myPage.init();
        return myPage;
    };
    //依赖构造函数
    var myXPage = function (ele, obj, local) {
        this.$element = ele;
        this.obj = obj;
        this.local = local;
    };
    myXPage.prototype = {
        init: function () {
            console.log("初始化XPage");
            this.$element.attr("class", "XPage")
            if (this.options.recordTotal > 0) {
                this.newPage();
            }
        },
        newPage: function () {
            var p = this;
            var tmp = "",
                i = 0,
                j = 0,
                a = 0,
                b = 0,
                totalpage = this.options.pageTotal;
            if (this.options.pageIndex > 1) {
                tmp += "<a><</a>"
            } else {
                tmp += "<a class=\"no\"><</a>"
            }
            tmp += "<a>1</a>";
            if (totalpage > this.options.showPageCount + 1) {
                if (this.options.pageIndex <= this.options.showPageCount) {
                    i = 2;
                    j = i + this.options.showPageCount;
                    a = 1;
                } else if (this.options.pageIndex > totalpage - this.options.showPageCount) {
                    i = totalpage - this.options.showPageCount;
                    j = totalpage;
                    b = 1;
                } else {
                    var k = parseInt((this.options.showPageCount - 1) / 2);
                    i = this.options.pageIndex - k;
                    j = this.options.pageIndex + k + 1;
                    a = 1;
                    b = 1;
                    if ((this.options.showPageCount - 1) % 2) {
                        i -= 1
                    }
                }
            }
            else {
                i = 2;
                j = totalpage;
            }
            if (b) {
                tmp += "..."
            }
            for (; i < j; i++) {
                tmp += "<a>" + i + "</a>"
            }
            if (a) {
                tmp += " ... "
            }
            if (totalpage > 1) {
                tmp += "<a>" + totalpage + "</a>"
            }
            if (this.options.pageIndex < totalpage) {
                tmp += "<a>></a>"
            } else {
                tmp += "<a class=\"no\">></a>"
            }
            var pager = this.$element.html(tmp);
            pager.children('a').click(function () {
                var cls = $(this).attr('class');
                if (this.innerHTML == "&lt;") {
                    if (cls != 'no') {
                        p.options.onPageClick(p.options.pageIndex - 1)
                    }
                } else if (this.innerHTML == "&gt;") {
                    if (cls != 'no') {
                        p.options.onPageClick(p.options.pageIndex + 1)
                    }
                } else {
                    if (cls != 'cur') {
                        p.options.onPageClick(parseInt(this.innerHTML))
                    }
                }
            }).each(function () {
                if (p.options.pageIndex == parseInt(this.innerHTML)) {
                    $(this).addClass('cur')
                }
            })  
        },
        
    }
})(jQuery);