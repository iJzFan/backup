
//报表通用
var PUBLIC_FINANCE = {
    views_Switching: function (id1, id2, _this) {
        var view1 = $("#" + id1), view2 = $("#" + id2), view = $(_this);
        if (view1.is(":hidden")) {
            view2.hide(); view1.show(); view.html("列表视图");
        } else {
            view1.hide(); view2.show(); view.html("图表视图");
        }
    },
    //获取开始时间和结束时间
    getTimeRange :function (_this,type) {
        var dtrange = $(_this).parent().find("input[name=dtrange]");
        var dt0 = dtrange.attr("data-val-dt0"), dt1 = dtrange.attr("data-val-dt1");
        if (!dt0 || !dt1) {
            $.err("请选择开始时间或结束时间！");
        } else {
            if (type == "gain") {
                PUBLIC_FINANCE.setFinaceGainTimeRange(dt0, dt1);
            } else if(type =="type") {
                PUBLIC_FINANCE.setFinaceTypeTimeRange(dt0, dt1);
            } else {
                PUBLIC_FINANCE.setFinaceWebTimeRange(dt0, dt1);
            }
            $(_this).parents(".ah-statement-tool").first().find(".ah-statement-active").each(function () {
                $(this).removeClass("ah-statement-active");
            });
        }
    },
    //设置收费统计时间范围和初始化
    setFinaceGainTimeRange: function (timeRange, dt1) {
        TJ_FinaceGain.setTimeRange(timeRange, dt1);
        TJ_FinaceGain.load();
    },
    //设置收费类型时间范围和初始化
    setFinaceTypeTimeRange:function (timeRange, dt1) {
        TJ_FinaceType.setTimeRange(timeRange, dt1);
        TJ_FinaceType.load();
    },
    //设置网上药店时间范围和初始化
    setFinaceWebTimeRange: function (timeRange, dt1) {
        WEB_Finace.setTimeRange(timeRange, dt1);
        WEB_Finace.load();
    },
}
//收费统计
var TJ_FinaceGain = {
    //基础数据存放
    StationId: null,
    TimeRange: "Today",
    vm: null,
    chartOp:null,//保存线形图的数据 用于改变屏幕大小重新画图
    loadVue: function () {
        TJ_FinaceGain.vm = new Vue({
            el: '#_pvVun',
            data: function () {
                return {
                    gain: {
                        items: [],
                        total: [],
                        total:[],
                    }
                }
            }
        });
    },
    setTimeRange: function (timeRange, dt1) {
        //如果是2个参数，则表示dt0,dt1的形式
        //如果是1个参数，则表示timeRange形式
        if (arguments.length == 2 && dt1 != null)
            
            TJ_FinaceGain.TimeRange = "dt0={0};dt1={1}".format(timeRange, dt1);
        else TJ_FinaceGain.TimeRange = timeRange;
    },
    //初始化收费统计数据
    load: function () {
        $.post("/Statistics/LoadFinaceChart_Gain", { timeRange: TJ_FinaceGain.TimeRange, stationId: TJ_FinaceGain.StationId }, function (jn) {
            var xAxisData = [];
            var formedVal = [], herbVal = [], shippingVal = [], otherFeeVal = [], consultationVal = [];
            for (var i = 0; i < jn.items.length; i++) {
                xAxisData.push(jn.items[i].payDateStr);
                formedVal.push(jn.items[i].formedVal);
                herbVal.push(jn.items[i].herbVal);
                shippingVal.push(jn.items[i].shippingVal);
                otherFeeVal.push(jn.items[i].otherFeeVal);
                consultationVal.push(jn.items[i].consultationVal);
            }
            var op = {
                title: "收益统计",//大标题
                titleSmall: "收益线形图",//小标题
                xAxisData: xAxisData,//X
                series: [
                    { name: '中药', type: 'line', data: herbVal },
                    { name: '成药', type: 'line', data: formedVal },
                    { name: '诊金', type: 'line', data: shippingVal },
                    { name: '快递', type: 'line', data: consultationVal },
                    { name: '其他', type: 'line', data: otherFeeVal }
                ],
                legend: ["中药", "成药", "诊金", "快递", "其他"],
                yUnit: "元",//Y轴单位
            }
            TJ_FinaceGain.chartOp = op;
            TJ_FinaceGain.initChart();
            TJ_FinaceGain.vm.gain = jn;
        });
    },
    initChart: function () {
        $("#ah-gainchart").html("").attr("_echarts_instance_", "");
        $.ahECharts(TJ_FinaceGain.chartOp, 'ah-gainchart');
    }
}

//收费类型统计
var TJ_FinaceType = {
    //基础数据存放
    StationId: null,
    TimeRange: "Today",

    setTimeRange: function (timeRange, dt1) {
        //如果是2个参数，则表示dt0,dt1的形式
        //如果是1个参数，则表示timeRange形式
        if (arguments.length == 2 && dt1 != null)
            TJ_FinaceType.TimeRange = "dt0={0};dt1={1}".format(timeRange, dt1);
        else TJ_FinaceType.TimeRange = timeRange;
    },
    //初始化收费类型数据
    load: function () {
        $.post("/Statistics/LoadFinaceChart_Type", { timeRange: TJ_FinaceType.TimeRange, stationId: TJ_FinaceType.StationId }, function (html) {
            $('div[ ah-id="pvLoadFinaceChart_Type"]').html(html);
            TJ_FinaceType.loadPie({ timeRange: TJ_FinaceType.TimeRange, stationId: TJ_FinaceType.StationId });
        });
    },
    //初始化饼图表
    loadPie: function (data) {
        $.post("/Statistics/GetPieData_Finace", data, function (jn) {
            if (jn.rlt) {
                TJ_FinaceType.legendData = jn.legendData;
                TJ_FinaceType.pieData = jn.pieData;
                TJ_FinaceType.seriesName = jn.seriesName;
                TJ_FinaceType.timeRange = data.timeRange;
                TJ_FinaceType.initPie();
            } else $.err(jn.msg);
        });
    },
    //初始化收费类型饼图
    initPie: function () {
        $("#ah-piechart").html("").attr("_echarts_instance_", "");
        $.ahPieChart({
            title: "收费类型统计",//大标题
            titleSmall: "收费类型统计饼图",//小标题
            legendData: TJ_FinaceType.legendData,//图表类型数组
            data: TJ_FinaceType.pieData,//图标数据
            seriesName: TJ_FinaceType.seriesName//鼠标经过时现实的提示文字
        }, "ah-piechart");
    }
}

//网上药店
var WEB_Finace = {
    //基础数据存放
    StationId: null,
    TimeRange: "Today",
    vm: null,
    chartOp: null,//保存线形图的数据 用于改变屏幕大小重新画图
    loadVue: function () {
        WEB_Finace.vm = new Vue({
            el: '#_pvVun',
            data: function () {
                return {
                    web: {
                        items: [],
                        total: [],
                        total: [],
                    }
                }
            }
        });
    },
    setTimeRange: function (timeRange, dt1) {
        //如果是2个参数，则表示dt0,dt1的形式
        //如果是1个参数，则表示timeRange形式
        if (arguments.length == 2 && dt1 != null)

            WEB_Finace.TimeRange = "dt0={0};dt1={1}".format(timeRange, dt1);
        else WEB_Finace.TimeRange = timeRange;
    },
    //初始化收费统计数据
    load: function () {
        $.post("/Statistics/LoadGainChartOfNetWebFinace", { timeRange: WEB_Finace.TimeRange, stationId: WEB_Finace.StationId }, function (jn) {
            var xAxisData = [];
            var totalAmount = [], transFeeAmount = [];
            for (var i = 0; i < jn.items.length; i++) {
                xAxisData.push(jn.items[i].sendDate);
                totalAmount.push(jn.items[i].totalAmount);
                transFeeAmount.push(jn.items[i].transFeeAmount);
            }
            var op = {
                title: "网上药店收益统计",//大标题
                titleSmall: "收益线形图",//小标题
                xAxisData: xAxisData,//X
                series: [
                    { name: '药品费', type: 'line', data: totalAmount },
                    { name: '快递费', type: 'line', data: transFeeAmount },
                ],
                legend: ["药品费", "快递费"],
                yUnit: "元",//Y轴单位
            }
            WEB_Finace.chartOp = op;
            WEB_Finace.initChart();
            WEB_Finace.vm.web = jn;
        });
    },
    initChart: function () {
        $("#ah-webchart").html("").attr("_echarts_instance_", "");
        $.ahECharts(WEB_Finace.chartOp, 'ah-webchart');
    }
}