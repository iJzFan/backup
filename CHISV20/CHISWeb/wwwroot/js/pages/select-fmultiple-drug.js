var ADDMULTIPLE = {
    sendIndex: 0,//用于记录时间
    addMultipleSelf: null,//用于选药所存储的处方对象
    //设置监听
    setMultipleEvent: function () {
        //搜索事件
        $("#search_submit").click(function () {
            var val = $(this).parent().find('input[id="multiple-search-drupinfos"]').val();
            ADDMULTIPLE.search(this, val);
        });
        //搜索药品(回车)
        $("input[name='multiple-search-drupinfos']").keydown(function (e) {
            var key = e.which; //e.which是按键的值
            if (key == 13) {
                ADDMULTIPLE.search(this);
            }
        });
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
        //双击选中
        $('div[ah-id="selectMultipleDrugTable"]').on('dblclick', "#drup_list tr", function (e) {
            var currentTarget = $(e.currentTarget);
            ADDMULTIPLE.SelectOneDrug({
                drugId: currentTarget.attr("ah-DrugId"),//药品ID
                stockFromId: currentTarget.attr("ah-StockFromId"),//药品库存来源ID
                drugName: currentTarget.attr("ah-DrugName"),//药品名
                drugModel: currentTarget.attr("ah-DrugModel"),//药品规格
                manufacturerOrigin: currentTarget.attr("ah-ManufacturerOrigin"),//生产厂家
            }, "New");
        });
        //选择一个本地药房药品
        $("#setLetter").on("dblclick", ".ah-mcdrug-gp li", function () {
            var _this = $(this);
            ADDMULTIPLE.SelectOneDrug({
                drugId: _this.attr("data-drugid"),//药品ID
                stockFromId: _this.attr("data-stockfromid"),//药品库存来源ID
                drugName: _this.attr("data-drugname"),//药品名
                drugModel: _this.attr("data-drugmodel"),//药品规格
                manufacturerOrigin: "本地药库",//生产厂家
            }, "New");
        })
        //搜索药品
        var sendIndex;//用于记录时间
        $("input[name='multiple-search-drupinfos']").keyup(function () {
            var searchTxt = $(this).val();
            clearTimeout(sendIndex);//清除上一次的等待
            sendIndex = setTimeout(function () {
                ADDMULTIPLE.search($("button[name='search_submit']")[0]);
            }, 800);
        })
    },
    //选中某个药
    SelectOneDrug: function (opt, type) {
        var state = true;
        //判断药品是否已经存在
        $('ul[ah-id="FMultipleSelected"] li').each(function () {
            if ($(this).attr("ah-drugid") == opt.drugId) {
                state = false;
            }
        });
        if (state) {
            $.post("/Doctor/SetMultipleFormedDrug", {
                DrugId: opt.drugId,
                StockFromId: opt.stockFromId,
                DrugName: opt.drugName,
                DrugModel: opt.drugModel,
                ManufacturerOrigin: opt.manufacturerOrigin,
                type: type
            }, function (html) {
                $('ul[ah-id="FMultipleSelected"]').prepend(html);
                if (type == "Unsaved") {

                } else if (type == "New") {
                    $.ok("添加药品成功，请及时保存！");
                    ADDMULTIPLE.UpdataCount();
                }
            });
        } else {
            $.err("该药品已经已经存在！");
        }
    },
    //更新已选药品统计
    UpdataCount: function () {
        var i = 0;
        $('ul[ah-id="FMultipleSelected"] li').each(function () {
            i++;
        });
        $(".ah-phone-selectCart .layui-badge").html(i);
        if (i == 0) {
            $(".ah-phone-selectCart .layui-badge").addClass("ah-hide")
        } else {
            $(".ah-phone-selectCart .layui-badge").removeClass("ah-hide")
        }
    },
    //删除开药界面新选择的药品
    DeleteOneNewDrug: function (_this) {
        $.confirm("确认", "是否删除该药？", function () {
            $(_this).parents("li").first().remove();
            ADDMULTIPLE.UpdataCount();
            $.ok("删除成功，请及时保存！")
        })
    },
    //保存多个药品
    SubmitFormeDrug: function (_this) {
        //判断是否有新添加药品
        if ($('ul[ah-id="FMultipleSelected"] .newTips').length > 0) {
            //获取所有需要保存的药品
            var drugs = new Array();
            $('ul[ah-id="FMultipleSelected"] li').each(function () {
                if ($(this).attr("ah-type") != "Unsaved") {
                    drugs.push({
                        drugId: parseInt($(this).attr("ah-drugid")),
                        StockFromId: $(this).attr("ah-stockfromid")
                    });
                }
            });
            if (drugs.length > 0) {
                $.post("/Doctor/GetSelectedDrugs", {
                    drugs: drugs,
                }, function (jn) {
                    if (jn.length > 0) {
                        var drugIds = [];
                        for (var i = 0; i < jn.length; i++) {
                            drugIds.push(jn[i].drugId)
                        }
                        $.post("/Doctor/AddMultipleFormedDrug", { drugs: drugIds }, function (html) {
                            //添加pv的HTML
                            ADDMULTIPLE.addMultipleSelf.find('ul[rolename="boxitem-drugs"]').append(html);
                            //更新layui
                            $.updataAllLayui();
                        });
                    }
                    ADDMULTIPLE.CloseFormedDrug();//关闭
                });
            } else {
                $.err("添加失败,没有新药品！");
            }
        } else {
            $.err("添加失败,没有新药品！");
        }
    },
    //搜索
    search: function (_this) {
        var wrap = $(_this).parents(".ah-forme-multiple").first();
        var drugforms = ADDMULTIPLE.getDrugFroms(_this);
        var val = $(_this).parent().find('input[name="multiple-search-drupinfos"]').val();
        var isZero = wrap.find("#isIncludeZero").is(":checked");
        if (drugforms.length == 0) { $.err("请选择药品来源"); return; }
        $.post("/Doctor/Json_GetDrugInfos", {
            term: val,
            drugFrom: drugforms,//改为数组
            isIncludeZero:isZero,
            pageIndex: 1
        }, function (html) {
            wrap.find('div[ah-id="selectMultipleDrugTable"]').html(html);
            $.updataScrollbar();
        });
    },
    //切换搜药模式
    selectFormedDrugModel: function (_this, stationId) {
        var wrap = $(_this).parents(".ah-forme-multiple").first();
        wrap.find("div[ah-name='search']").each(function () {
            $(this).toggle();
        });
        wrap.find("div[ah-name='letter']").each(function () {
            $(this).toggle();
        });
        if ($('#setLetter').is(":visible") && $('#setLetterLoading').size() > 0) {
            $.post("/doctor/LoadMyClinicDrugs", { stationId: stationId }, function (html) {
                $('#setLetter').html(html);

            });
        }
    },
    //设置来源
    setDrigFroms: function (FORMEDSELECT) {
        var wrap = $("div[ah-id='ah-multiple-wrap']");
        var cookies = ',' + FORMEDSELECT + ',';
        //设置来源
        wrap.find('ul[ah-id="select-items-ul"] li').each(function (i, m) {
            var v = $(m).attr('ah-value');
            if (cookies.indexOf("," + v + ",") >= 0) $(m).addClass("select-active");
            else $(m).removeClass("select-active");
        });
    },
    //获取已选来源
    getDrugFroms: function () {//获取来源项目数组
        var wrap = $(".ah-forme-multiple").first();
        var rtn = new Array();
        wrap.find('ul[ah-id="select-items-ul"] li.select-active').each(function (i, m) {
            rtn.push($(m).attr("ah-value"));
        });
        return rtn;
    },
    //显示药品层
    AddFormedDrug: function (_this) {
        ADDMULTIPLE.addMultipleSelf = $(_this).parents('section[role="DRUGS"]').first();//保存该处方外层
        var arr = [], old = [];
        ADDMULTIPLE.addMultipleSelf.find("input[name='Details[].DrugId']").each(function () {
            var li = $(this).parent("li").first()
            //如果不是新增的药
            if (li.attr("ah-state") != "new") {
                arr.push($(this).val());
            } else {
                old.push({
                    drugId: li.attr("ah-DrugId"),//药品ID
                    stockFromId: li.attr("ah-StockFromId"),//药品库存来源ID
                    drugName: li.attr("ah-DrugName"),//药品名
                    drugModel: li.attr("ah-DrugModel"),//药品规格
                    manufacturerOrigin: li.attr("ah-ManufacturerOrigin"),//生产厂家
                });
            }
        });
        $.post("/Doctor/SelectMultipleFormedDrug", { drugs: arr }, function (html) {
            var wrap = $("div[ah-id='ah-multiple-wrap']");
            //显示搜药
            wrap.html(html);
            if (old.length > 0) {
                for (var item in old) {
                    //未保存的药
                    ADDMULTIPLE.SelectOneDrug(old[item], "Unsaved");
                }
            }
            wrap.show();
            //滚动条初始化
            $(".scrollbar-dynamic").each(function () {
                $(this).scrollbar();
            });
        });
    },
    //关闭药品层
    CloseFormedDrug: function () {
        $(".ah-forme-multiple").hide();
    },
    //切换已选药品和搜索药品
    SelectViewModel: function (_this) {
        var i = $(_this).find("i").first()
        if ($(".ah-fMultiple-left").is(":hidden")) {
            $(".ah-fMultiple-left").css("display","flex");
            $(".ah-fMultiple-right").addClass("ah-hide");
            i.attr("class", "glyphicon glyphicon-list");
        } else {
            $(".ah-fMultiple-left").css("display", "none");
            $(".ah-fMultiple-right").removeClass("ah-hide");
            i.attr("class", "glyphicon glyphicon-shopping-cart");
        }
    }
};
var DIAGNOSIS = {
    //诊断回调
    DiagnosisCallback: function (id,text,val) {
        $("#"+id).html(text);
        $("#" + id + "_id").val(val);
    },
    //删除诊断后判断是否有选过 避免第一次选中删除了诊断引起后台错误
    DelectDiagnosisCallback: function (id, did) {
        console.log(actionID, did)
        var input = $("#" + id + "_id");
        var text = $("#" + id);
        if (input.val() == did) {
            text.html("");
            input.val("");
        } else {
            text.html(text);
            input.val(val);
        }
    }
}



//更新药品信息
function refreshDrugInfo(drugId, _this) {
    var _this1 = $(_this).parents("tr").first();
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
    if (_this1.attr("ah-SourceFrom") == 1 || drug.DrugCode.indexOf("AHJK") >= 0) {
        //添加对依赖第三方信息的药品信息刷新
        setTimeout(function () {
            $.getJson("/openapi/drug/UpdateDrugInfo", { drugId: drug.DrugId }, function (jn) {
                if (jn.rlt) {
                    $(_this).attr("src", jn.drugPicUrl);
                }
            });
        }, 100);

    }
}

