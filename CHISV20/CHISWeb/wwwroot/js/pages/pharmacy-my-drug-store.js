/*===================================================入库======== START ================================*/
var IncomeState = true;
var INCOME = {
    //获取入库列表
    GetIncomeLists: function (pageIndex) {
        var searchText = $('#search').find("[name=searchText]").val();
        var dt0 = $('#search').find("[name=dt0]").val();
        var dt1 = $('#search').find("[name=dt1]").val();

        $.post("/Pharmacy/LoadIncomeLists", { searchText: searchText, dt0: dt0, dt1: dt1, pageIndex: pageIndex }, function (html) {
            $('div[ah-id="INCOME-TABLE"]').html(html);
        });
    },
    //选择入库药品 
    SelectDurgIncome: function (drugId) {
        var idx = layer.open({
            type: 2,
            area: ['70%', '80%'],
            title: '添加药品',
            content: "/Pharmacy/SelectIncomeDrug?drugId="+drugId,
            btn: ['确定', '取消'],
            yes: function (index, layero) {
                var iframeWin = window[layero.find('iframe')[0]['name']];
                //执行窗口函数
                iframeWin.SelectedDrug(function (onedrug) {
                    if (onedrug.rltStatus == "SUCCESS") {
                        parent.INCOME.addDrugInfoToTable(autoInput(onedrug));
                        parent.INCOME.isShowNotDataPic();//判断是否有数据
                        parent.INCOME.updataOrdinal();//更新表格序号
                    }
                });
            },
            cancel: function () {
                //右上角关闭回调
            }
        });
        function autoInput(onedrug) {
            //估填信息 重新设置数据
            var dopt = {};
            if ($('#auto_input').is(":checked")) {
                var now = new Date();
                dopt.batNo = formatDate(now, "yyyyMMdd");
                dopt.produceTime = now.addDays(0 - parseInt($('#pcfg_sdt').val()));
                dopt.deadlineTime = now.addDays(onedrug.ValidDays || parseInt($('#pcfg_edt').val()));
                dopt.defInNum = parseInt($('#pcfg_num').val());
            }
            onedrug.incomeNum = dopt.defInNum || 1;
            onedrug.deadlineTime = formatDate(dopt.deadlineTime || "");
            onedrug.produceTime = formatDate(dopt.produceTime || "");
            onedrug.incomePriceBig = onedrug.incomePriceBig || "";
            onedrug.incomePriceSmall = onedrug.incomePriceSmall || "";
            onedrug.batNo = dopt.batNo || "";
            return onedrug;
        }
    },
    //新增入库药品
    addDrugInfoToTable: function (onedrug) {
        console.log(onedrug);
        var tb = $('tbody[ah-id="income-main-table"]');
        //判断该药是否已经添加过
        tb.find("tr").each(function (i, m) {
            if ($(m).attr("ah-did") == onedrug.drugId) {
                //如果是相同药品则累加
                if ($('#chk_addnum').is(":checked")) onedrug.incomeNum += parseInt($(m).find("input[ah-id='Num']").val());
                $(m).remove();
                return;
            }
        });

        //整理数据
        onedrug = $.extend(onedrug, {
            //生产商
            manufacturerOrigin: onedrug.manufacturerOrigin || str(onedrug.drugManufacture) + str(onedrug.drugOrigPlace),
            //进货价格
            incomePrice: (onedrug.incomePrice == "null" || !onedrug.incomePrice) ? "" : onedrug.incomePrice,
        });

        var html = "<tr ah-state='add' ah-did='" + onedrug.drugId + "'>" +
            "<td></td>" +
            "<td>" + onedrug.drugId + "</td>" +
            "<td>" + onedrug.drugName + "</td>" +
            "<td>" + onedrug.drugModel + "</td>" +
            "<td>" + onedrug.manufacturerOrigin + "</td>" +
            "<td><input ah-id='BatNo' value='" + onedrug.batNo + "' class='ah-table-input' type='text'/></td>" +
            "<td><input ah-id='ProduceTime' value='" + onedrug.produceTime.substring(0, 10) + "' placeholder='yyyy-mm-dd' ah-type='date' class='ah-table-input ah-date' style='text-align:left;' type='text'/></td>" +
            "<td><input ah-id='DeadlineTime' value='" + onedrug.deadlineTime.substring(0, 10) + "' placeholder='yyyy-mm-dd' ah-type='date' class='ah-table-input ah-date' style='text-align:left;' type='text'/></td>" +
            "<td><input ah-id='IncomePrice' value='" + $.f.toPrice(onedrug.incomePrice, 3) + "' class='ah-table-input' type='text'/></td>" +
            "<td><input ah-id='Num' onkeydown='return $.f.onlyNumbersEvt(event);' maxlength='5' onafterpaste='onlyNumOnafterpaste(this)' type='text' value='" + onedrug.incomeNum + "'class='ah-table-input'/></td>" +
            "<td style='text-align: center;'><div class='ah-tool-wrap' id='unitId'></div></td >" +
            "<td style='text-align: center;'><a ah-income-active='del'>删除</a></td>" +
            "</tr>"

        var $html = $(html);
        //追加入库单位的单选按钮
        // 是否选择大单位
        var isSelBig = onedrug.incomeUnitId == onedrug.outUnitBigId && onedrug.incomeUnitId != onedrug.outUnitSmallId;
        //生成大小单位的Radio选择按钮
        var unitShtml = "<input ah-id='UnitSmallId' data-pval='" + onedrug.incomePriceSmall + "' value='" + onedrug.outUnitSmallId + "' type='radio' name='outUnit" + onedrug.drugId +  "'"+ (isSelBig?"":"checked='checked'")+ " title='" + onedrug.outUnitSmallName + "' />";
        var unitBhtml = "<input ah-id='UnitBiglId' data-pval='" + onedrug.incomePriceBig + "' value='" + onedrug.outUnitBigId + "' type='radio' name='outUnit" + onedrug.drugId + "'" + (isSelBig ? "checked='checked'":"") +" title='" + onedrug.outUnitBigName + "'  />";
        //插入单位按钮
        if (onedrug.outUnitSmallId != onedrug.outUnitBigId) {
            $html.find("#unitId").append(unitShtml + unitBhtml);
        } else {
            $html.find("#unitId").append(unitShtml);
        }
        //设置单位按钮点击后的事件，给入库价格赋值
        $.onRadio(function (d) {
            var $c = $(d.elem);
            var val = $c.attr("data-pval"); if (val) val = $.f.toPrice(val, 3);
            $c.parents("tr").first().find("input[ah-id=IncomePrice]").val(val);
        });
 
        //选中入库数量
        $html.find("[ah-id='Num']").unbind("focus").on("focus", function () {
            $(this).select();
        });
        tb.append($html);//添加
        $.updataAllLayui();//加载样式
        //选中数量 定时后才能选中
        setTimeout(function () { $html.find("[ah-id='Num']").focus().select(); }, 200);

        layer.closeAll();
        lay('.ah-date').each(function () {
            var op = {
                elem: this
                , calendar: true
            };
            if ($(this).attr("ah-max-date")) {
                op.max = $(this).attr("ah-max-date");
                //op.max = parseInt($(this).attr("ah-max-date"));
            }
            if ($(this).attr("ah-min-date")) {
                op.min = $(this).attr("ah-min-date");
                //op.min = parseInt($(this).attr("ah-min-date"));
            }

            layui.laydate.render(op);
        });

    },
    //删除入库药品
    delIncomeDrug: function (_this) {
        //询问框
        var index = $.confirm('删除提示','是否删除该药品？', function () {
            $(_this).parents("tr").first().remove();
            layer.close(index);
            parent.INCOME.isShowNotDataPic();//判断是否有数据
            parent.INCOME.updataOrdinal();//更新表格序号
        });
    },
    //是否显示暂无数据
    isShowNotDataPic: function () {
        if ($("tr[ah-state='add']").length > 0) {
            $('table[ah-table="INCOME"]').removeClass("hidden");
            $(".income-not-data").addClass("hidden");
        } else {
            $('table[ah-table="INCOME"]').addClass("hidden");
            $(".income-not-data").removeClass("hidden");
        }
    },
    //更新序号
    updataOrdinal: function () {
        var tb = $('tbody[ah-id="income-main-table"]');
        //判断该药是否已经添加过
        tb.find("tr").each(function (i, m) {
            $(m).find('td').first().html(i + 1);
        });
    },
    //保存入库药品
    SaveDurgIncome: function () {
        var arr = new Array();
        var tb = $('tbody[ah-id="income-main-table"]');
        var ymd = /^(\d{4})-(0\d{1}|1[0-2])-(0\d{1}|[12]\d{1}|3[01])$/;//日期校验
        var pr = /^\d+(\.\d{1,3})?$/;//价格校验

        IncomeState = true;
        //判断该药是否已经添加过
        tb.find("tr").each(function (i, m) {
            console.log(i);
            var BatNo = $(m).find("input[ah-id='BatNo']").first().val();//批号
            var ProduceTime = $(m).find("input[ah-id='ProduceTime']").first().val();//生产时间
            var DeadlineTime = $(m).find("input[ah-id='DeadlineTime']").first().val();//过期时间
            var IncomePrice = $(m).find("input[ah-id='IncomePrice']").first().val();//入库价格
            var Num = $(m).find("input[ah-id='Num']").first().val();//入库数量

            //批号校验
            INCOME.changeDataStyle($(m).find("input[ah-id='BatNo']").first().first(), BatNo);

            //生产日期校验
            INCOME.changeDataStyle($(m).find("input[ah-id='ProduceTime']").first(), ymd.test(ProduceTime));

            //过期日期校验
            INCOME.changeDataStyle($(m).find("input[ah-id='DeadlineTime']").first(), ymd.test(DeadlineTime));

            //入库价格校验
            INCOME.changeDataStyle($(m).find("input[ah-id='IncomePrice']").first(), pr.test(IncomePrice) || IncomePrice == "");

            //入库数量校验
            INCOME.changeDataStyle($(m).find("input[ah-id='Num']").first(), pr.test(Num) && Num > 0);

            var _drugId = $(m).attr("ah-did");
            arr.push({
                DrugId: $(m).attr("ah-did"),//药品ID
                BatNo: BatNo,
                ProduceTime: ProduceTime,
                DeadlineTime: DeadlineTime,
                IncomePrice: IncomePrice,
                Num: Num,
                InUnitId: $(m).find("input[name='outUnit" + _drugId + "']:checked").val(),//入库单位
            });

        });
        if (IncomeState) {
            $.post("/Pharmacy/SaveIncome", { items: arr }, function (jn) {

                if (jn.rlt) {
                    window.location.href = jn.CallBackUrl;
                } else {
                    layer.alert(jn.message, {
                        skin: 'layui-layer-lan'
                        , closeBtn: 0
                        , anim: 1 //动画类型
                    });
                }
            });
        } else {
            layer.alert('数据有误，请重新确认药品数据！', {
                skin: 'layui-layer-lan'
                , closeBtn: 0
                , anim: 1 //动画类型
            });
        }

    },
    //删除入库流水表数据
    delTurnoverDurg: function (stockInId, _this) {
        //询问框
        $.confirm("询问", "是否删除该药品？", function () {
            $.post("/Pharmacy/DeleteIncomeItem", { stockInId: stockInId }, function (jn) {
                if (jn.rlt) {
                    $(_this).parents("tr").first().remove()
                } else {
                    layer.alert(jn.message, {
                        skin: 'layui-layer-lan'
                        , closeBtn: 0
                        , anim: 1 //动画类型
                    });
                }
            });
        });
    },
    //数据校验样式
    changeDataStyle: function (obj, state) {
        if (state) {
            obj.css("border", "none").css("border-bottom", "1px dashed");
        } else {
            obj.css("border", "1px solid");
            IncomeState = false;
        }
    },
    //导出入库记录
    ExportIncomeList: function () {
        var searchText = $('#search').find("[name=searchText]").val();
        var dt0 = $('#search').find("[name=dt0]").val();
        var dt1 = $('#search').find("[name=dt1]").val();
        var el = document.createElement("a");
        document.body.appendChild(el);
        el.href = '/Pharmacy/ExportStockMonitorList?searchText=' + searchText + "&dt0=" + dt0 + "&dt1=" + dt1; //url 是你得到的连接
        el.target = '_blank'; //指定在新窗口打开
        el.click();
        document.body.removeChild(el);
    },
    //批量入库 
    ImportExcelFile: function () {
        var handle = top.layer.open({
            type: 2,
            title: '选择上传的文件',
            shadeClose: true,
            shade: 0.8,
            area: ["500px", "300px"],
            content: '/Pharmacy/UpdateExcelFile'
        });
    },
    //批量载入返回的数据
    ImportExcelLoad: function (jn) {
        for (var i = 0; i < jn.length; i++) {
            var jna = jn[i];
            console.log(jna);
            this.addDrugInfoToTable(jna);
        }
        this.isShowNotDataPic();//判断是否有数据
        this.updataOrdinal();//更新表格序号
        layer.msg("导入数据成功！");
    }

};
/*===================================================入库======== End ================================*/

/*===================================================库存======== START ================================*/
var STOCK = {
    //获取药房库存列表
    GetStockMontorLists: function (pageIndex) {
        var searchText = $('input[ah-id="searchText"]').val();
        var stockNumStatus = $('input[name="stockNumStatus"]:checked').val();
        var drugStockTypeId = $('#drugStockTypeId').val();
        $.get("/Pharmacy/LoadStockMonitorLists", { searchText: searchText, drugStockTypeId: drugStockTypeId, stockNumStatus: stockNumStatus, pageIndex: pageIndex }, function (html) {
            $('div[ah-id="INCOME-TABLE"]').html(html);
        });
    },
    //导出库存列表
    ExportStockMonitorList: function () {
        var searchText = $('input[ah-id="searchText"]').val();
        var stockNumStatus = $('input[name="stockNumStatus"]:checked').val();
        var el = document.createElement("a");
        document.body.appendChild(el);
        el.href = '/Pharmacy/ExportStockMonitorList?searchText=' + searchText + "&stockNumStatus=" + stockNumStatus; //url 是你得到的连接
        el.target = '_blank'; //指定在新窗口打开
        el.click();
        document.body.removeChild(el);
    },
    //编辑药房药品
    EditStockMonitorDetail: function (_this, drugStockMonitorId, newPrice, num, safeNum) {
        var tr = $(_this).parents("tr");
        var idx = $.open({          
            type: 2,
            area:["560px","80%"],
            title: '库存数据维护',
            content: "/Pharmacy/EditStockMonitorDetail?drugStockMonitorId=" + drugStockMonitorId + "&&price=" + newPrice + "&&num=" + num + "&&safeNum=" + safeNum,
            btn: ['确定', '取消'],
            yes: function (index, layero) {
                var iframeWin = window[layero.find('iframe')[0]['name']];
                //执行窗口函数
                iframeWin.SelectedDrug(function (onedrug) {
                    //console.log(onedrug)
                    $.post("/Pharmacy/ChangeDrugStockData", {
                        drugStockMonitorId: onedrug.DrugStockMonitorId,
                        newPrice: onedrug.StockSalePrice,
                        num: onedrug.DrugStockNum,
                        safeNum: onedrug.StockLineNum,
                        rmk: onedrug.Rmk
                    }, function (jn) {
                        if (jn.rlt) {
                            tr.find("span[ah-id='StockSalePrice']").html("￥" + onedrug.StockSalePrice);
                            tr.find("span[ah-id='DrugStockNum']").html(onedrug.DrugStockNum);
                            tr.find("td[ah-id='StockLineNum']").html(onedrug.StockLineNum);
                            $(_this).attr("onclick", "STOCK.EditStockMonitorDetail(this,'" + onedrug.DrugStockMonitorId + "'," + onedrug.StockSalePrice + "," + onedrug.DrugStockNum + "," + onedrug.StockLineNum + ")");
                            layer.closeAll();
                        }
                        layer.msg(jn.message);
                    });
                });
            },
            cancel: function () {
                //右上角关闭回调

            }
        });
    },
    //删除监控记录
    DeleteItem: function (_this, drugStockMonitorId) {
        $.deleteData({
            url: "/Pharmacy/DeleteDrugStockMonitorById",
            data: { drugStockMonitorId: drugStockMonitorId },
            fnSuccess: function () {
                $.ok("删除成功！")
                $(_this).parents("tr").first().remove();
            }
        });
    }

};
/*===================================================库存======== End ================================*/

/*===================================================库存======== START ================================*/
var MENU = {
    //初始化我的申请药品清单
    InitMyDrugMenuList: function (pageIndex) {
        var searchText = $('input[ah-id="searchText"]').val();
        var dt0 = $('#search').find("[name=dt0]").val();
        var dt1 = $('#search').find("[name=dt1]").val();
        var status = $('input[name="activeStatus"]:checked').val();
        if (status == "null") status = null;
        $.post("/Pharmacy/MyDrugMenuList", { searchText: searchText, dt0: dt0, dt1: dt1, status: status, pageIndex: pageIndex }, function (html) {
            $('div[ah-id="MENU-TABLE"]').html(html);
        });
    },
};
/*===================================================库存======== End ================================*/