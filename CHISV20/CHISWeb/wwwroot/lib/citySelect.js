/*
日期：2017-04-15

prov:省份
city:城市
dist:地区（县）
------------------------------ */
//加载函数
 


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
    $0.unbind("change").on("change", function () { f.setCitys(); });
    $1.unbind("change").on("change", function () { f.setCounties(); });
    $2.unbind("change").on("change", function () {
        option.$val.val($2.val());
    })

    var areaId = option.$val.val();
    if (areaId) {
        //初始化地址
        $.getJSON("/openapi/Utils/JetAreasId", { areaId: areaId }, function (jn) {
            f.setProvince(jn.level0Id, function () {
                f.setCitys(jn.level1Id, function () {
                    f.setCounties(jn.level2Id);
                })
            })
        });
    } else {
        f.setProvince();
        $1.empty().append(f.optItem("--请选择市--"));
        $2.empty().append(f.optItem("--请选择区--"));
    }


}



