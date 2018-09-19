/*
 * 自写的插件
 * Author: Rex
 * Date: 2017-2-23
 * 说明：需要引入jquery bootstrap layer 之后再引入此js.
*/
$(function () {
    //首先载入样式
    //  $.f.addCSSFile("my-widget");
});







//定义一个选择区域的jquery插件
$.fn.SelectArea = function (cmd, areaId) {
    if (arguments.length > 0) {
        if (cmd == "setAreaId") {
            var $this = $(this);
            var tagid = $this.attr("tagfor-id");
            var tagtxt = $this.attr("tagfor-text");

            $('#' + tagid).val(areaId);//设置areadId
            var $tagtxtcontrol = $('#' + tagtxt);

            if (!areaId) {
                $('#' + tagid).val('');
                $tagtxtcontrol.text("请先点击选择地址");
                return;
            }

            $.get("/openapi/Utils/JetAreasId", { areaId: areaId }, function (jn) {
                if (jn.level2MergerName) $tagtxtcontrol.text(jn.level2MergerName);
                else $tagtxtcontrol.text("请先点击选择地址");
            });
        }
        return;
    }


    //首先载入样式
    $.f.addCSSFile("my-widget");
    //对地址的文字进行整理
    function initAddressName(addName) {
        return addName.replace("中国,", "").replace(/[,|,]/g, " ");
    }

    var $this = $(this);
    var tagid = $this.attr("tagfor-id");
    var tagtxt = $this.attr("tagfor-text");
    var $tagcontrol = $('#' + tagid);//存放区域Id的Hidden-Input
    var $tagtxtcontrol = $('#' + tagtxt);
    if ($tagcontrol.val()) {
        //初始化地址
        $.get("/openapi/Utils/JetAreasId", { areaId: $tagcontrol.val() }, function (jn) {
            $tagtxtcontrol.text(initAddressName(jn.level2MergerName
            ))
        });
    } else $tagtxtcontrol.text("请先点击选择地址");
    $this.on("click", function () {
        var initAreaId = $tagcontrol.val();// 初始的区域Id
        var layerId = "layer_" + tagid;
        //弹出选择区域的弹出框
        var layerIndex = layer.open({
            id: layerId,
            type: 1,
            icon: 1,
            area: ['30%', '50%'],
            title: '请选择地址区域',
            content: '<div class="areaselect-bg" id="' + layerId + '_bg"></div>',
            btn: ['确定', '取消'],
            yes: function (index, layero) {
                $tagcontrol.val($pv2.val());
                var txt = $pv2.find("option:selected").data("areatxt");
                $tagtxtcontrol.text(initAddressName(txt));
                layer.close(layerIndex);
                $this.find('label.error').remove();//移除错误提示
            },
            cancel: function () {
                //右上角关闭回调
            }
        });
        //设置内容
        $pv = $("<ul class='area-select-ul'><li class='pv0'></li><li class='pv1'></li><li class='pv2'></li></ul>");
        $pv0 = $("<select class='form-control spv0'></select>");
        $pv1 = $("<select class='form-control spv1'></select>");
        $pv2 = $("<select class='form-control spv2'></select>");

        var pvfunc = {
            //设置省的清单
            SetLevel0List: function (loadfunc) {
                $pv.find(".pv0").empty();
                $pv.find(".pv1").empty();
                $pv.find(".pv2").empty();
                $pv0.empty().append("<option value=''>--请选择省份--</option>");
                $.getJSON("/openapi/Utils/JetAreas", function (jn) {
                    $.each(jn.items, function (i, m) { $pv0.append("<option value='" + m.areaId + "'>" + m.name + "</option>"); });
                    $pv.find(".pv0").append($pv0);
                    if (loadfunc) { loadfunc(); }
                });
            },
            SetLevel1List: function (parentId, loadfunc) {
                $pv.find(".pv1").empty();
                $pv.find(".pv2").empty();

                parentId && $.getJSON("/openapi/Utils/JetAreas", { parentId: parentId }, function (jn) {
                    $pv1.empty().append("<option value=''>--请选择市--</option>");
                    $.each(jn.items, function (i, m) { $pv1.append("<option value='" + m.areaId + "'>" + m.name + "</option>") });
                    $pv.find(".pv1").append($pv1);
                    if (loadfunc) { loadfunc(); }
                });
            },
            SetLevel2List: function (parentId, loadfunc) {
                $pv.find(".pv2").empty();
                parentId && $.getJSON("/openapi/Utils/JetAreas", { parentId: parentId }, function (jn) {
                    //载入三级
                    $pv2.empty().append("<option value=''>--请选择镇区--</option>");
                    $.each(jn.items, function (i, m) { $pv2.append("<option value='" + m.areaId + "' data-areatxt='" + m.mergerName + "'>" + m.name + "</option>") });
                    $pv.find(".pv2").append($pv2);
                    if (loadfunc) { loadfunc(); }
                });
            },
            LoadInitAreaId: function (initAreaId) {
                /*如果有初始的区域数据*/
                if (initAreaId) {
                    //载入三级表，然后设置三级数据内容
                    $.getJSON("/openapi/Utils/JetAreasId", { areaId: initAreaId }, function (jn) {
                        $pv0.val(jn.level0Id); pvfunc.SetLevel1List(jn.level0Id, function () {
                            $pv1.val(jn.level1Id); pvfunc.SetLevel2List(jn.level1Id, function () { $pv2.val(jn.level2Id); });
                        });
                    });
                }
            }
        }
        pvfunc.SetLevel0List(function () { pvfunc.LoadInitAreaId(initAreaId) });
        $pv.on("change", "select", function () {
            if ($(this).hasClass("spv0")) { pvfunc.SetLevel1List($(this).val()); }
            if ($(this).hasClass("spv1")) { pvfunc.SetLevel2List($(this).val()); }
        });
        $('#' + layerId + "_bg").empty().append($pv);
    });
}

/*
option:{$province,$city,$county,$val}
*/
$.set3LevelAddress = function (option) {
    var $0 = option.$province;
    var $1 = option.$city;
    var $2 = option.$county;


    var f = {
        optItem: function (txt, val, bsel) {
            if (val == undefined) val = '';
            if (bsel) return "<option value='" + val + "' selected='selected'  >" + txt + "</option>";
            else return "<option value='" + val + "'  >" + txt + "</option>";
        },
        setProvince: function (selval, callback) {
            $.getJSON("/openapi/Utils/JetAreas", {}, function (jn) {
                $0.empty().append(f.optItem("--请选择省级--"));
                $.each(jn.items, function (i, m) {
                    $0.append(f.optItem(m.name, m.areaId));
                });
                if (selval) $0.val(selval);
                if (callback) { callback(); }
            });
        },
        setCitys: function (selval, callback) {
            var parentId = $0.val();
            parentId && $.getJSON("/openapi/Utils/JetAreas", { parentId: parentId }, function (jn) {
                $1.empty().append(f.optItem("--请选择市--"));
                $2.empty().append(f.optItem("--请选择区--"));
                $.each(jn.items, function (i, m) { $1.append(f.optItem(m.name, m.areaId)); });
                if (selval) $1.val(selval);
                if (callback) { callback(); }
            });
        },
        setCounties: function (selval, callback) {
            var parentId = $1.val();
            parentId && $.getJSON("/openapi/Utils/JetAreas", { parentId: parentId }, function (jn) {
                $2.empty().append(f.optItem("--请选择区--"));
                $.each(jn.items, function (i, m) { $2.append(f.optItem(m.name, m.areaId)); });
                if (selval) $2.val(selval);
                if (callback) { callback(); }
            });
        }

    }

    $0.on("change", function () { f.setCitys(); });
    $1.on("change", function () { f.setCounties(); });
    $2.on("change", function () {
        option.$val.val($2.val());
    })

    var areaId = option.$val.val();
    if (areaId) {
        $0.hide(); $1.hide(); $2.hide();
        //初始化地址
        //移除disabled
        var bdisabed = false, breadonly = false;
        if (typeof ($0.attr("disabled")) != "undefined") { bdisabed = true; $0.removeAttr("disabled"); $1.removeAttr("disabled"); $2.removeAttr("disabled"); }
        if (typeof ($0.attr("readonly")) != "undefined") { breadonly = true; }
        $.getJSON("/openapi/Utils/JetAreasId", { areaId: areaId }, function (jn) {
            f.setProvince(jn.level0Id, function () {

                f.setCitys(jn.level1Id, function () {
                    f.setCounties(jn.level2Id);
                })
            })
            if (bdisabed) { $0.attr("disabled", ""); $1.attr("disabled", ""); $2.attr("disabled", ""); }
            if (breadonly) {
                $0.attr("disabled", ""); $1.attr("disabled", ""); $2.attr("disabled", "");
                $0.attr("readonly", ""); $1.attr("readonly", ""); $2.attr("readonly", "");
            }
            $0.show(); $1.show(); $2.show();
        });
    } else {
        f.setProvince();
        $1.empty().append(f.optItem("--请选择市--"));
        $2.empty().append(f.optItem("--请选择区--"));
    }


}





$.fn.EditLabel = function (settings) {
    var $this = $(this);
    $this.click(function () {
        var $now = $(this);
        var val = $now.text();
        var inputType = ($now.attr("ah-input-type") || "").toLowerCase();
        var inputName = "input[type=text]";
        if (inputType == "textarea") inputName = "textarea";
        var $input = $now.find(inputName);
        if ($input.length == 0) {
            $input = $("<input class='input-text' type='text' style='width:" + $now.innerWidth() + "px;height:" + ($now.innerHeight() - 2) + "px;' value='" + val + "'/>");
            if (inputName == "textarea") $input = $("<textarea class='input-area'>" + val + "</textarea>");
            $now.text('').append($input);
            $input.focus().select().blur(function () {
                var v = $(this).val();
                $now.empty().text(v).removeClass("edit-label-now");
                if (v != val) {
                    //向后台发送数据并更新
                    $.loadJSON(settings.url, { key: $now.attr("ah-input-key"), val: v }, function (jn) {
                        if (!jn.rlt) $.alertError(jn.msg);
                    });
                }
            });
            $now.addClass("edit-label-now");
        }

    });
}





/*三态显示*/
$.fn.Status3 = function (callback) {
    if ($(this)[0].nodeName == "SELECT") {
        $(this).wrap("<div class='ah-ynstatus'></div>");
        /*三态显示*/
        $(this).parent('.ah-ynstatus').each(function (i, item) {
            var setv = {
                setByValue: function (v) {
                    if (v == "True") {
                        i.removeClass("determined").removeClass("fa-toggle-off").addClass("fa-toggle-on");
                    } else if (v == "False") {
                        i.removeClass("determined").removeClass("fa-toggle-on").addClass("fa-toggle-off");
                    } else {
                        i.removeClass("fa-toggle-on").addClass("fa-toggle-off").addClass("determined");
                    }
                }
            }
            var v = $(item).find("select").val();
            var i = $(item).find("span>i");
            if (i.size() == 0) {
                var a = $("<span><i class='fa'></i></span>"); $(item).append(a); i = a.find("i");
                i.click(function () {
                    $s = $(this).parents(".ah-ynstatus").first().find("select");
                    if ($s.val() == "True") { $s.val("False"); setv.setByValue("False"); callback("False"); }
                    else if ($s.val() == "False") { $s.val(""); setv.setByValue(""); callback(""); }
                    else { $s.val("True"); setv.setByValue("True"); callback("True"); }
                })
            }
            setv.setByValue(v);

        });
    }
}








/*
* 树形前端插件
 * @param {any} options
 */
function zTreeObj(options) {
    var treeId = options.id;
    var $tree = $("#" + treeId);
    $tree.empty();//清除处理

    options = $.extend({
        dragable: false,
        multiSelect: false,
        checkable: false
    }, options);

    var setting = {
        edit: {
            drag: {
                isMove: true,
                prev: true,
                next: true,
                inner: true
            },
            enable: options.dragable,
            showRemoveBtn: false,
            showRenameBtn: false,
        },

        view: {
            selectedMulti: options.multiSelect
        },
        check: {
            enable: options.checkable
        },
        callback: {
            onClick: zTreeOnClick,  //树节点点击事件
            beforeDrag: beforeDrag, //在拖拽之前，先判断该节点能否被拖拽，能拖拽的才会继续后续操作，否则直接退出拖拽功能
            beforeDrop: beforeDrop, //在添加到目标节点下之前，判断目标节点是否存在（这里限制了对于根节点的添加），若存在则判断目标节点下是否已存在相同的节点
            onDrop: onDrop,
        },
        data: {
            simpleData: {
                enable: true,
                idkey: 'id',
                pIdkey: 'pId',
                rootId: 0
            }
        }
    };

    $.ajax({
        async: false,
        cache: false,
        type: 'POST',
        dataType: 'json',
        url: options.url,
        success: function (data) {
            $.fn.zTree.init($tree, setting, data);
            var treeObj = $.fn.zTree.getZTreeObj(treeId);
            treeObj.expandAll(options.isExpandAll||true);
            if (options.onLoaded) options.onLoaded();
        },
        error: function (msg) {
            $.alertError("请求失败！" + msg);
        }
    });





    //=====================点击工作站查询记录=============================================================
    function zTreeOnClick(event, treeId, treeNode) {
        var zTree = $.fn.zTree.getZTreeObj(treeId);
        var selnodes = zTree.getSelectedNodes();
        var obj = selnodes[0];
        if (options.onClickNode) options.onClickNode(obj);
    };

    //获取工作站树第一个节点ID
    function getFirstNode(zTree) {
        try {
            var nodes = zTree.getNodes();
            if (nodes.length > 0) return nodes[0].id;
            else return -1;
        } catch (ex) { return 0; }
    }

    //获取选择的一个节点
    function getSelectOneNode() {
        var zTree = $.fn.zTree.getZTreeObj(treeId);
        var selnodes = zTree.getSelectedNodes();
        var obj = selnodes[0];
        return obj;
    };


    //================================zTree的拖拽=========================================
    function beforeDrag(treeId, treeNodes) {
        for (var i = 0, l = treeNodes.length; i < l; i++) {
            if (treeNodes[i].drag === false) {
                curDragNodes = null;
                return false;
            } else if (treeNodes[i].parentTId && treeNodes[i].getParentNode().childDrag === false) {
                curDragNodes = null;
                return false;
            }
        }
        curDragNodes = treeNodes;
        return true;
    }
    function beforeDrop(treeId, treeNodes, targetNode, moveType, isCopy) {
        //如果有提交到后台的操作，则会先执行if…else…再执行post等提交操作
        if (targetNode) {
            if (targetNode.children != undefined) {
                //console.log(targetNode.children[0].name)
                //var nodes = targetNode.children;
                var name = treeNodes[0].name;
                for (i = 0; i < targetNode.children.length; i++) {
                    if (targetNode.children[i].name == name) {
                        $.modalAlert("错误：此工作站已经存在！", "error");
                        return false;
                    }
                }
            }
            return true;
        }
    }

    //拖拽成功后，修改后台数据库被拖拽节点的pid
    function onDrop(event, treeId, treeNodes, targetNode, moveType, isCopy) {
        treeNodes && targetNode && $.modalConfirm("是否将此操作同步更新至数据库？", function (result) {
            if (result) $.post('/Code/UpateWorkStationParent', { id: treeNodes[0].id, pid: targetNode.id })
            else $.reload();
        });
    }

    return {
        getSelectOneNode: getSelectOneNode
    };
}






(function ($) {
    $.fn.extend({


        /*--------------------- singleSelector----------------*/
        singleSelector: function (options) {
            var $c = this;
            //初始化
            var def = {
                placeholder: "请输入查询内容",
                ver: 1
            };
            var opt = $.extend(def, options);
            var cc = "<a class='select'>选择</a><div class='search-panel' style='display:none;'><div class='search-bg'><div class='input-group input-group-sm'><input type='text' class='form-control search-text'><span class='input-group-btn'><a class='btn btn-default search-btn'><i class='fa fa-search'></i></a></span></div></div><div class='search-content'><ul></ul></div></div>";
            $c.append(cc);

            $c.find('.search-panel').hide();
            $c.find('.search-panel .search-text').attr('placeholder', opt.placeholder);

            $c.find('.select').click(function () {
                $c.find('.search-panel .search-content ul').empty();
                $c.find('.search-panel .search-text').val('');
                $c.find('.search-panel').show();
            });

            $c.find('.search-btn').click(search);
            $c.find('.search-content ul>li>a').click(selectOneItem);


            $cid = $c.find("input[type=hidden]");
            if (opt.isReadonly) $cid.attr("readonly", "readonly");

            var initalVal = $cid.val();//获取初始值
            var isDisabled = true;//是否禁用
            if (typeof ($cid.attr("disabled")) == "undefined") isDisabled = false;
            var isReadOnly = true;//是否只读
            if (typeof ($cid.attr("readonly")) == "undefined") isReadOnly = false;

            if (initalVal) {
                $.getJSON(opt.searchUrl, { id: initalVal }, function (jn) {
                    var v = (jn == null || jn == "null" || jn == undefined) ? null : jn[0];
                    selectOneItem(v);
                });
            }
            if (isReadOnly || isDisabled) $c.find(".select").remove(); //如果只读则移除选择按钮
            if (isDisabled) $c.addClass("single-selector-disabled");
            if (isReadOnly) $c.addClass("single-selector-readonly");


            function selectOneItem(item) {
                $c.find("input[type=hidden]").val(item[opt.valueName]);
                $c.find(".search-panel").hide();
                if (opt.onSelect) opt.onSelect(item);
            }


            function search() {
                var txt = $c.find('.search-text').val();
                var url = opt.searchUrl;
                $ul = $c.find('.search-content ul').empty();
                txt && $.getJSON(url, { searchtext: txt }, function (jn) {
                    if (jn && jn != 'null') {
                        $.each(jn, function (i, m) {
                            var template = "请初始化选择后的列表模板";
                            if (opt.formatSearchItem) template = opt.formatSearchItem(m);
                            var $template = $(template);
                            $template.click(function () {
                                selectOneItem(m);
                            });
                            $ul.append($template);
                        });
                    }
                });

            }



            $("body").on("click", function (event) {
                var $target = $(event.target).first();
                var isOut = $target.parents(".search-panel").length == 0;
                if (event.target.tagName == "A" && event.target.className == "select") isOut = false;
                if (isOut) $c.find(".search-panel").hide();
            });




        }

    });
})(jQuery);



/*------------------------- switch2 --------------------*/
(function ($) {
    var methods = {
        init: function (options) {
            return this.each(function () {
                var $this = $(this);
                var settings = $this.data('switch2');
                if (typeof (settings) == 'undefined') {
                    //默认参数
                    var defaults = {
                        propertyName: 'value',
                        onSomeEvent: function () { }
                    }
                    settings = $.extend({}, defaults, options);
                    $this.data('switch2', settings);
                } else {
                    settings = $.extend({}, settings, options);
                }
                // 代码在这里运行
                if ($this.on("click", function () {
                    methods.onChange(settings);
                }));
            });
        },
        destroy: function (options) {
            // 在每个元素中执行代码
            return $(this).each(function () {
                var $this = $(this);
                // 执行代码
                // 删除元素对应的数据
                $this.removeData('switch2');
            });
        },
        val: function (options) {
            var v = $(this).eq(0).find("input[type=checkbox]").is(":checked");
            return v;
        },
        onChange: function (options) {
            return $(this).each(function () {
                var $this = $(this);
                if (options.onChange) options.onChange(methods.val(options));
            });
        }
    };


    $.fn.switch2 = function () {
        // 获取我们的方法，遗憾的是，如果我们用function(method){}来实现，这样会毁掉一切的
        var method = arguments[0];
        // 检验方法是否存在
        if (methods[method]) {
            // 如果方法存在，存储起来以便使用
            // 注意：我这样做是为了等下更方便地使用each（）
            method = methods[method];
            // 如果方法不存在，检验对象是否为一个对象（JSON对象）或者method方法没有被传入
            // 我们的方法是作为参数传入的，把它从参数列表中删除，因为调用方法时并不需要它
            arguments = Array.prototype.slice.call(arguments, 1);
        } else if (typeof (method) == 'object' || !method) {
            // 如果我们传入的是一个对象参数，或者根本没有参数，init方法会被调用
            method = methods.init;
        } else {
            // 如果方法不存在或者参数没传入，则报出错误。需要调用的方法没有被正确调用
            $.error('Method ' + method + ' does not exist on jQuery.pluginName');
            return this;
        }
        // 调用我们选中的方法
        // 再一次注意我们是如何将each（）从这里转移到每个单独的方法上的
        return method.apply(this, arguments);
    }

})(window.jQuery);


/*--------------------- Status3 -----------------------*/
(function ($) {
    var status3 = function (element, options) {
        this.element = element;
        this.options = options;
        this.init();         
    };

    status3.prototype = {
        init: function () {
            var that = this;
            var $ele = $(this.element);
            var $sel = $ele.find("select").first();
            var dd = "disabled='disabled'", rr = "readonly='readonly'";
            if (typeof ($sel.attr("disabled")) == "undefined") dd = "";
            if (typeof ($sel.attr("readonly")) == "undefined") rr = "";
            var $h = $("<div class='status3-fm' " + dd + " " + rr + " ></div>");
             
            $ele.on("click", ".status3-fm", function () {
                var act = true;
                if (typeof ($(this).attr("disabled")) != "undefined") act = false;
                if (typeof ($(this).attr("readonly")) != "undefined") act = false;
                if (act) {
                    var $s = $(this).parents(".status3").first().find("select");
                    var v = $s.val();
                    var nv = "";
                    if (v == "") nv = "True";
                    if (v == "True") nv = "False";
                    $s.val(nv);
                    setVal(nv);
                    if (that.options.onChange) that.options.onChange(nv);
                }
            });

            $ele.on("click", "label", function () { 
                $ele.find(".status3-fm").trigger("click");
            });

            $sel.hide();
            $sel.after($h);
            //初始化
            var nv = $sel.val();
            setVal(nv, "未选");

            function setVal(nv) {
                $h.attr("value", nv).attr("title", transTitle(nv));
            }
            function transTitle(v, wx) {
                switch (v) {
                    case "True": return "是";
                    case "False": return "否";
                    default: return wx || "未知，请点选";
                }
            }
        },
        setOptions: function (opts) {
            this.options = $.extend(true, this.options, opts);//重新设置配置
        },
        getOptions: function () {
            return this.options;
        },
        val: function () {   
            var v = $(this.element).find("select").val();
            if (v == "") return undefined;
            if (v == "True") return true;
            if (v == "False") return false;
            return undefined;
        }
    };

    $.fn.status3 = function (options) {
        var args = arguments;
        //不返回jquery对象，而是返回具体的函数 
        if (args[0] == "get") {
            var ui = $._data(this.get(0), "status3"); 
            var fc = args[1];  
            args = Array.prototype.slice.call(args, 2); 
            return ui[fc](args);
        }

        return this.each(function () {
            //此处的this是DOM对象
            var ui = $._data(this, "status3");           
            if (!ui) {
                var opts = $.extend(true, {}, $.fn.status3.defaults, typeof options === "object" ? options : {});
                ui = new status3(this, opts);
                $._data(this, "status3", ui);
            }

            if (typeof options === "string" && typeof ui[options] == "function") {
                args = Array.prototype.slice.call(args, 1);
                ui[options].apply(ui, args);
            }
        });
    }

    $.fn.status3.defaults = {};

})(window.jQuery);


/*--------------------- selectInput -------------------*/
(function ($) {
    var selectInput = function (element, options) {
        this.element = element;
        this.options = options;
        this.init();
    }

    selectInput.prototype = {
        init: function () {
            var that = this;
            var $ele = $(this.element);             
            if ($ele.find(".input-group-btn").length != 0) return;

            $ele.find("input").wrap($('<div class="input-group"></div>'));
            var h = '\
            <div class="input-group-btn">\
                <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown"><span class="caret"></span></button>\
                <ul class="dropdown-menu pull-right"></ul>\
            </div>';
            $ele.find("input").after(h);
            $ele.find("select option").each(function (i, m) {
                var $m = $(m);
                $ele.find("ul").append("<li><a href='javascript:void(0);' value='"+$m.attr("value")+"'>"+$m.text()+"</a></li>")
            });
            $ele.on("click", 'ul li a', function () {       
                $ele.find("input").val($(this).attr("value"));
            });

            var bDisabled = false, bReadonly = false;
            if (typeof ($ele.find("input").attr("disabled")) != "undefined") bDisabled = true;
            if (typeof ($ele.find("input").attr("readonly")) != "undefined") bReadonly = true;

            if (bDisabled) $ele.find("button").attr("disabled", "");
            if (bReadonly) $ele.find("button").attr("disabled", "").attr("readonly", "");

        }
    }
    $.fn.selectInput = function (options) {
        var args = arguments;
        //不返回jquery对象，而是返回具体的函数 
        if (args[0] == "get") {
            var ui = $._data(this.get(0), "selectInput");
            var fc = args[1];
            args = Array.prototype.slice.call(args, 2);
            return ui[fc](args);
        }

        return this.each(function () {
            //此处的this是DOM对象
            var ui = $._data(this, "selectInput");
            if (!ui) {
                var opts = $.extend(true, {}, $.fn.selectInput.defaults, typeof options === "object" ? options : {});
                ui = new selectInput(this, opts);
                $._data(this, "selectInput", ui);
            }

            if (typeof options === "string" && typeof ui[options] == "function") {
                args = Array.prototype.slice.call(args, 1);
                ui[options].apply(ui, args);
            }
        });
    }
    $.fn.selectInput.defaults={};

})(window.jQuery);



/*---------------------通用插件函数 ------------------*/
 
(function ($) {
    $.fn.setSelectValue = function (val) {
                var $this = $(this);
                var bdisabled = false;
                if (typeof $this.attr("disabled") != "undefined") bdisabled = true;
                $this.removeAttr("disabled").val(val).trigger("change");
                bdisabled&&$this.attr("disabled","disabled"); 
    } 

})(window.jQuery);

