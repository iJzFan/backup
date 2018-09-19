/*
 * 自写的插件
 * Author: Rex
 * Date: 2017-2-23
 * 说明：需要引入jquery bootstrap layer 之后再引入此js.
*/
$(function () {
    //首先载入样式
    $.f.addCSSFile("my-widget");
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

            $.getJSON("/api/common/GetAreasId", { areaId: areaId }, function (jn) {
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
        $.getJSON("/api/common/GetAreasId", { areaId: $tagcontrol.val() }, function (jn) {
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
                $.getJSON("/api/common/GetAreas", function (jn) {
                    $.each(jn.items, function (i, m) { $pv0.append("<option value='" + m.areaId + "'>" + m.name + "</option>"); });
                    $pv.find(".pv0").append($pv0);
                    if (loadfunc) { loadfunc(); }
                });
            },
            SetLevel1List: function (parentId, loadfunc) {
                $pv.find(".pv1").empty();
                $pv.find(".pv2").empty();

                parentId && $.getJSON("/api/common/GetAreas", { parentId: parentId }, function (jn) {
                    $pv1.empty().append("<option value=''>--请选择市--</option>");
                    $.each(jn.items, function (i, m) { $pv1.append("<option value='" + m.areaId + "'>" + m.name + "</option>") });
                    $pv.find(".pv1").append($pv1);
                    if (loadfunc) { loadfunc(); }
                });
            },
            SetLevel2List: function (parentId, loadfunc) {
                $pv.find(".pv2").empty();
                parentId && $.getJSON("/api/common/GetAreas", { parentId: parentId }, function (jn) {
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
                    $.getJSON("/api/common/GetAreasId", { areaId: initAreaId }, function (jn) {
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

$.fn.EditLabel = function (settings) {
    var $this = $(this);
    $this.click(function () {
        var $now = $(this);
        var val = $now.text();
        var inputType = ($now.attr("ah-input-type")||"").toLowerCase();
        var inputName = "input[type=text]";
        if (inputType == "textarea") inputName = "textarea";
        var $input = $now.find(inputName);
        if ($input.length == 0) {        
            $input = $("<input class='input-text' type='text' style='width:" + $now.innerWidth() + "px;height:" + ($now.innerHeight()-2) + "px;' value='" + val + "'/>");
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

