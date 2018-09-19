$(function () {
    $('#search-bg').on('dblclick', "#drup_list tr", function () {
        //应该是触发layer窗体的确定按钮
        var index = parent.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
        var $c = parent.$('#layui-layer' + index + ' .layui-layer-btn a.layui-layer-btn0');
        $c.trigger("click");
        // parent.layer.close(index); //再执行关闭        
    });
    $('#search_drupinfos').focus().select();//载入选择


})
//鼠标移入移出
function showBigImg(_this) {
    $(_this).next().show();
}
function hideBigImg(_this) {
    $(_this).next().hide();
}
//搜索药品
var sendIndex;//用于记录时间
$("#search_drupinfos").keyup(function () {
    var $this = $(this);
    var searchTxt = $this.val();
    var unitIds = $('#sel_units_group:visible').find('#unitIds').val();
    clearTimeout(sendIndex);//清除上一次的等待
    sendIndex = setTimeout(function () {
        if (searchTxt.length > 2) {
            $.post("/Pharmacy/GetMyStockDrugInfos", {
                searchText: searchTxt,
                unitIds: unitIds,
                pageIndex: 1
            }, function (html) {
                $('div[ah-id="selectDrugTable"]').html(html);
                $this.select();
                //如果只有一条数据并且(搜索的长度>10并且是数字=>判定为barcode)，则自动选择该数据
                if (searchTxt.length > 10 && !isNaN(searchTxt)) {
                    var $tr = $('#drup_list>tr');
                    if ($tr.length == 1) {
                        $tr.click();
                        setTimeout(function () { $tr.find("td").first().dblclick(); }, 200);
                    }
                }
            });
        }
    }, 500);


})



//药品选中赋值显示
$('.table-responsive').on("click", "#drup_list tr", function () {
    $('#choosed_medicine').val($(this).find('td[id=drugName]').text());
    $(this).parents('tbody[id=drup_list]').find("tr").removeClass('active_ass');
    $(this).addClass('active_ass');
});


function getOneDrugJson() {
    var _this1 = $('.active_ass').first();
    var s = _this1.find('td[name=JSONDATA]').text();
    var json = JSON.parse(s);//JSON.stringify(json) 
    return json;
}
function getOneDrug() {
    var m = getOneDrugJson();
    console.log("select-income-drug.js/getOneDrug:");
    console.log(m);
    var _this1 = $('.active_ass').first();
    var d0 = {
        rltStatus: "SUCCESS",
        rltMsg: "",
        adviceId: null,
        drugId: m.DrugId,
        drugName: m.DrugName,
        drugModel: m.DrugModel,
        salePrice: _this1.find('td[id=salePrice]').text().trim(),
        manufacturerOrigin: m.ManufacturerOrigin,// _this1.find('td[id=manufacturerOrigin]').text(),

        outUnitBigName: m.OutUnitBigName,// _this1.find('td[name=outUnitBigName]').text(),//包装单位
        outUnitSmallName: m.OutUnitSmallName,
        minUnitName: m.DosageUnitName,//_this1.find('td[name=dosageUnitName]').text(),//剂量单位
        outUnitBigId: m.UnitBigId,
        outUnitSmallId: m.UnitSmallId,
        disageUnitId: m.DosageUnitId,
        rate: m.OutpatientConvertRate,//转换率
        prescribeStyles: _this1.find('td[name="prescribeStyles"]').text(),
        ValidDays: m.ValidDays,
        incomePriceBig: m.ExpMyLastIncomeBigPrice || getPrice(m.ExpMyLastIncomeSmallPrice, m.OutpatientConvertRate, false),
        incomePriceSmall: m.ExpMyLastIncomeSmallPrice || getPrice(m.ExpMyLastIncomeBigPrice, m.OutpatientConvertRate, true),
    };
    return $.extend(d0, {
        incomeUnitName: d0.outUnitSmallName,
        incomeUnitId: d0.outUnitSmallId,
        incomePrice: d0.incomePriceSmall,
        qty:1
    });

    function getPrice(priceBase, rate, isBigPriceBase) {
        if (isBigPriceBase) {
            if (priceBase && rate) return priceBase / rate;
            else return null;
        }
        else {
            if (priceBase && rate) return priceBase * rate;
            else return null;
        }
    }
}

function SelectedDrug(func) {
    var _this1 = $('.active_ass').first();
    var onedrug = getOneDrug();
    //如果是健客药品，则开始检测药品是否可以添加
    if (_this1.attr("ah-SourceFrom") == 1) {
        $.getJSON("/Doctor/CheckDrugIsAvaliable", { threePartDrugId: _this1.attr("ah-ThreePartDrugId") }, function (json) {
            if (json.rlt) { if (func) func(onedrug); }
            else { $.alertError(json.msg); }
        });
    } else { if (func) func(onedrug); }
}