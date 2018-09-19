$(function () {
    //选择来源监听
    $('ul[ah-id="select-items-ul"] li').on("click", function () {
        var _this = $(this);
        var v = _this.attr("ah-value");
        if (_this.hasClass("select-active")) {
            _this.removeClass("select-active");
        } else {
            _this.addClass("select-active");
        }
    });
    $('#search-bg').on('dblclick', "#drup_list tr", function () {
        //应该是触发layer窗体的确定按钮
        var index = parent.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
        var $c = parent.$('#layui-layer' + index + ' .layui-layer-btn a.layui-layer-btn0');
        $c.trigger("click");
        // parent.layer.close(index); //再执行关闭        
    });

    $('#drup_list').on('click', 'tr a.drugRefresh', function () {
        var $this = $(this);
        var drugId = $this.parents('tr').find("input[name=drugId]").val();
        $.loadJSON("/Doctor/Json_RefreshDrugInfo", { drugId: drugId }, function (jn) {
            if (jn.rlt) { $this.parents('td').text(jn.price); }
            else { $.alertError(jn.msg); }
        });
    });
    setDrugForms();
})

function setDrugForms() {//设置选择来源项目
    var s = ',' + pagedata.SelectForms + ',';
    $('ul[ah-id="select-items-ul"] li').each(function (i, m) {
        var v = $(m).attr('ah-value');
        if (s.indexOf("," + v + ",") >= 0) $(m).addClass("select-active");
        else $(m).removeClass("select-active");
    });
}
function getDrugFroms() {//获取来源项目数组
    var rtn = new Array();
    $('ul[ah-id="select-items-ul"] li.select-active').each(function (i, m) {
        rtn.push($(m).attr("ah-value"));
    });
    return rtn;
}

//搜索药品
var sendIndex;//用于记录时间
$("#search_drupinfos").keyup(function () {
    var searchTxt = $(this).val();
    clearTimeout(sendIndex);//清除上一次的等待
    sendIndex = setTimeout(function () {
        var drugforms = getDrugFroms();

        if (searchTxt.length > 2) {
            if (drugforms.length == 0) { layer.msg("请选择药品来源"); return; }
            $.post("/Doctor/Json_GetDrugInfos", {
                term: searchTxt,
                drugFrom: drugforms,//改为数组
                pageIndex: 1
            }, function (html) {
                $('div[ah-id="selectDrugTable"]').html(html);
            });
        }
    }, 800);
})



//药品选中赋值显示
$('.table-responsive').on("click", "#drup_list tr", function () {
    $('#choosed_medicine').val($(this).find('td[id=drugName]').text());
    $(this).parents('tbody[id=drup_list]').find("tr").removeClass('active_ass');
    $(this).addClass('active_ass');
});

function SelectedDrug(func) {
    var _this1 = $('.active_ass').first();
    var drug = JSON.parse(_this1.find('td[name="JSONDATA"]').text());//获取数据
    var onedrug = {
        rltStatus: "SUCCESS",
        rltMsg: "",
        adviceId: null,
        drugId: _this1.attr("ah-DrugId"),
        drugName: _this1.find('td[id=drugName]').text().trim(),
        drugModel: _this1.find('td[id=drugModel]').text().trim(),
        salePrice: _this1.find('td[id=salePrice]').text().trim(),
        outUnitBigName: _this1.find('td[name=outUnitBigName]').text(),//包装单位
        outUnitSmallName: _this1.find('td[name=outUnitSmallName]').text(),//封装单位
        minUnitName: _this1.find('td[name=dosageUnitName]').text(),//剂量单位
        outUnitBigId: drug.UnitBigId,
        outUnitSmallId: drug.UnitSmallId,
        disageUnitId: drug.DosageUnitId,
        prescribeStyles: _this1.find('td[name="prescribeStyles"]').text(),
        qty: 1
    };
    //如果是健客药品，则开始检测药品是否可以添加
    if (_this1.attr("ah-SourceFrom") == 1) {
        $.getJSON("/Doctor/CheckDrugIsAvaliable", { threePartDrugId: _this1.attr("ah-ThreePartDrugId"), refreshTime: drug.ThreePartDrugRefreshTime }, function (json) {
            if (json.rlt) { if (func) func(onedrug); }
            else { $.alertError(json.msg); }
        });
    } else {
        if (drug.DrugCode.indexOf("AHJK") >= 0) {
            //添加对依赖第三方信息的药品信息刷新
            $.getJSON("/openapi/drug/UpdateDrugInfo", { drugId: drug.DrugId }, function (jn) {
                if (func) func(onedrug);
            });
        } else {
            if (func) func(onedrug);
        }
    }
}
 



