/*! XTree 0.0.1 */
/**
 * Created by Tezml on 2017/11/3
 * author : lixiaofeng
 */
(function ($) {
    $.fn.XTree = function (obj) {
        //局部变量
        var local = {
            arr:[],//已经添加过的
        }
        //默认配置
        var defaults = {
            
        }
        var myTree = new myXTree(this, obj, local);
        if (obj && $.isEmptyObject(obj) == false) {
            myTree.options = $.extend(defaults, obj, local);
        } else {
            myTree.options = defaults;
        }
        //调用初始化方法
        myTree.init();
        return myTree;
    };
    //依赖构造函数
    var myXTree = function (ele, obj, local) {
        this.$element = ele;
        this.obj = obj;
        this.local = local;
    };
    myXTree.prototype = {
        init: function () {
            console.log("初始化XTree");
            console.log(this);
            //追加HTML和style
            this.loadTreeWrap(this);
        },
        loadTreeWrap: function (_this) {
            //追加外层样式
            this.$element.addClass("ah-my-tree");
            //保存文本框对象 方便后续赋值
            this.local.event = this.$element.find("#ah-tree-event").first();
            //追加弹出HTML
            this.loadTreeHtml();
            //tree监听
            this.loadTreeEvent();
        },
        loadTreeHtml: function () {
            this.local.treeWrap = $("<ul class='ah-mytree-wrap'></ul>");
            //初始化tree数据
            this.addTreeHtml();
            this.$element.parents("body").first().append(this.local.treeWrap);
        },
        loadTreeEvent: function () {
            var _this = this;
            //鼠标点击其他位置
            $(document).click(function (e) {
                e = window.event || e; // 兼容IE7
                obj = $(e.srcElement || e.target);
                if ($(obj).parents(".ah-mytree-wrap").first().length <= 0) {
                    if ($(obj).attr("id") != _this.local.event.attr("id")) {
                        _this.local.treeWrap.hide();
                    }
                }
            }); 
            //监听文本框焦点 显示树
            _this.local.event.click(function () {
                var win = document.querySelector('#ah-tree-event').getBoundingClientRect();
                _this.local.treeWrap.css("top", win.bottom + 10);
                if (_this.options.float == "right") {
                    _this.local.treeWrap.css("left", win.left-320+win.width);
                } else {
                    _this.local.treeWrap.css("left", win.left);
                }
                _this.local.treeWrap.show();
                _this.local.treeWrap.addClass("layui-anim layui-anim-upbit");
                _this.local.event.blur();
            });
            //选择节点后回调
            $(".ah-mytree-wrap").on("click","a",function () {
                var a = $(this);
                var li = a.parents("li").first()
                var data = {
                    id: li.attr("id"),
                    pid: li.attr("pid"),
                    value: a.html(),
                }
                if (data.id) {
                    _this.options.selectNode(data);
                    _this.local.treeWrap.hide();
                }
            });
            //展开监听
            $(".ah-mytree-wrap").on("click", "span[ah-type='symbol']", function () {
                var s = $(this),state="close"
                if (s.hasClass("ah-symbol-close")) {
                    s.removeClass("ah-symbol-close").addClass("ah-symbol-open");
                    s.next().removeClass("ah-flie-close").addClass("ah-flie-open");
                    state = "open";
                } else {
                    s.removeClass("ah-symbol-open").addClass("ah-symbol-close");
                    s.next().removeClass("ah-flie-open").addClass("ah-flie-close");
                    state = "close";
                }
                var li = s.parents("li").first();
                var ul = li.find("ul");
                if (ul.length > 0) {
                    $.each(ul, function (i, m) {
                        if (state == "close") {
                            $(m).hide();
                        } else {
                            $(m).show();
                        }
                    })
                } else {
                    //加载tree子节点数据
                    _this.addTreeHtml(s, li.attr("id"));
                }
            });
        },
        addTreeHtml: function (event,id) {
            var _this = this;
            var url = _this.options.url.format(_this.options.id);
            if (event) {
                url = _this.options.moreUrl.format(id);
            }
            $.post(url, function (jn) {
                //按照需求 ， 初始化时接口带bWithRoot=true 是父节点和子节点都有返回
                var field = _this.options.field;
                if (jn.rlt) {
                    var html = "";
                    var pItem = [];
                    //整理数据
                    $.each(jn[_this.options.jnArray], function (i, item) {
                        if(item){
                            //查询是否有子节点 然后放到childNode
                            $.each(jn[_this.options.jnArray], function (i2, item2) {
                                if (item2) {
                                    if (item2.pId == item.id) {
                                        if (item.childNode == undefined) item.childNode = [];
                                        item.childNode.push(item2);
                                        delete jn[_this.options.jnArray][i2];
                                    }
                                }
                            });
                        }
                    });
                    $.each(jn[_this.options.jnArray], function (i, item) {
                        if(item){
                            html += "<li id=" + item[field.id] + " pid=" + item[field.pid] + ">";
                            //无子节点
                            if (!item.isParent) {
                                html += "<span class='ah-mytree-icon ah-mytreicon-notmore'></span>";
                            } else {
                                if (event) {
                                    html += "<span class='ah-symbol-icon ah-symbol-close' ah-type='symbol'></span>";
                                    html += "<span class='ah-mytree-icon ah-flie-close'></span>";
                                } else {
                                    html += "<span class='ah-symbol-icon ah-symbol-open' ah-type='symbol'></span>";
                                    html += "<span class='ah-mytree-icon ah-flie-open'></span>";
                                }
                               
                            }
                            html += "<a href='javascript:void(0)' >" + item[field.value] + "</a>";
                            //有子节点
                            if (item.childNode) {
                                html += "<ul class='ah-tree-more-ul' style='display: block;'>";
                                $.each(item.childNode, function (i2, item2) {
                                    if (item2[field.pid] == item[field.id]) {
                                        html += "<li id=" + item2[field.id] + " pid=" + item2[field.pid] + ">";
                                        //无子节点
                                        if (!item2.isParent) {
                                            html += "<span class='ah-mytree-icon ah-mytreicon-notmore'></span>";
                                        } else {
                                            html += "<span class='ah-symbol-icon ah-symbol-close' ah-type='symbol'></span>";
                                            html += "<span class='ah-mytree-icon ah-flie-close'></span>";
                                        }
                                        html += "<a href='javascript:void(0)' >" + item2[field.value] + "</a>";
                                        html += "</li>";
                                    }
                                });
                                html += "</ul>";
                            }
                            html += "</li>";
                        }
                    });
                    if (event) {
                        //添加子节点
                        _this.addMoreTreeHtml(html,event);
                    } else {
                        //添加1级节点
                        _this.local.treeWrap.append(html);
                    }
                } else $.err(jn.msg);
            });           
        },
        addMoreTreeHtml: function (html, event) {
            event.parents("li").first().append("<ul class='ah-tree-more-ul'>{0}</ul>".format(html));
        }
    }
})(jQuery);