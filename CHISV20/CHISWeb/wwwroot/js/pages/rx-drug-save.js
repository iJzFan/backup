var RXDRUGSAVE = {
    //初始化扫一扫填信息的二维码
    InitSupplementInfoCode: function () {
        $("#rx-userinfo-code").qrcode({
            text: pagedata.qrUrl,
            width: 150,
            height: 150,
        });
    },
    //添加一个处方药格子
    addTrDrug: function (_this) {
        $(this).after();
    },
    //添加一个处方药
    addRxDrugSaveDrug: function () {

        var opt = {
            rxSaveId: $("#RxSaveId").val(),
            model: {
                DrugId: $("#drugId").val(),
                DrugName: $("#drugName").val(),
                DrugModel: $("#drugMolde").val(),
                DrugManufacture: $("#drugManufacturerOrigin").val(),
                DrugPiNo: $("#drugBatNo").val(),
                DrugDeadTime: $("#drugDeadlineTime").val(),
                DrugQty: $("#drugNum").val(),
                DrugUnitName: $("#dosageUnitName").val()
            }
        };
        console.log(opt);
        if (opt.model.DrugPiNo == "" || opt.model.DrugDeadTime == "" || opt.model.DrugQty == "") {
            $.err("请检查药品信息！");
            return false;
        } else {
            //检查日期格式
            var reg = /^(\d{4})-(0\d{1}|1[0-2])-(0\d{1}|[12]\d{1}|3[01])$/;  
            var regExp = new RegExp(reg);
            if (!regExp.test(opt.model.DrugDeadTime)) {
                $.err("日期格式不正确，正确格式为：2018-01-01");
　　            return;
            }
            if (new XDate() > new XDate(opt.model.DrugDeadTime)) {
                $.err("有效期不能小于今天！");
                return;
            }
            if (!(new RegExp("^[0-9]*$").test(opt.model.DrugQty))) {
                $.err("数量只能为数字！");
                return;
            } else {
                if (opt.model.DrugQty <= 0) {
                    $.err("数量必须大于0！");
                    return;
                }
            }
            
        }
        //判断药品是否重复或者为空
        if (opt.model.DrugId!="") {
            var ul = $("ul[ah-id='RxDrugSaveBase_Drug']");
            var status = true;
            ul.find("li").each(function () {
                if (opt.model.DrugId == $(this).attr("ah-drugid")) {
                    status = false;
                    $.err("不能添加重复的药品，如需修改请先删除!");
                }
            });
            if (status) {
                $.post("/Nurse/RxDrugSaveAddDrug", opt, function (html) {
                    ul.append(html)
                });
            }
        } else {
            $.err("请选择药品!");
        }
        
    },
    //搜索药品
    GetDrugsBasic: function (val, pageIndex) {
        var pageSize = 10;
        if (!pageIndex) {
            pageIndex = 1;
        }
        if (val) {
            $.get("/openapi/Drug/GetDrugsBasic", { searchText: val, pageSize: pageSize, pageIndex: pageIndex}, function (jn) {
                console.log(jn)
                if (jn.length == 1) {
                    $("#drugId").val(jn[0].drugId);
                    $("#dosageUnitName").val(jn[0].dosageUnitName);
                    $("#drugName").val(jn[0].drugName);
                    $("#drugMolde").val(jn[0].drugModel);
                    $("#drugManufacturerOrigin").val(jn[0].manufacturerOrigin);
                    $("#drugBatNo").val("");
                    $("#drugDeadlineTime").val("");
                    $("#drugNum").val(1);
                } else if (jn.length > 1) {
                    var ul = $("<ul class='ah-flex-auto'></ul>");
                    for (var i = 0; i < jn.length; i++) {
                        var d = jn[i];
                        var html = "<li ah-drugId='" + d.drugId + "' ah-drugManufacturerOrigin='" + $.returnNullText(d.manufacturerOrigin) +"' ah-drugMolde='" + d.drugModel+"' ah-drugName='"+d.drugName+"' ah-dosageUnitName='" + d.dosageUnitName+"' >";
                        html += "<img src='" + d.drugImgUrl + "'/>";
                        html += "<div class='ah-over-hidden '><div>" + d.drugName + "</div><div class='drugModel'>" + d.drugModel;
                        html += " (<span style='color: #ff6a00; font - size:14px; font - weight:bold;'>" + d.dosageUnitName + "</span>)</div>";
                        html += "<div class='manufacturerOrigin'>" + $.returnNullText(d.manufacturerOrigin) + $.returnNullText(d.originPlace) + "</div></div>";
                        html += "</li>";
                        ul.append(html)
                    }
                    var pageWrap = $("<div class='ah-flex ah-rx-selectPage'></div>");
                    if (pageIndex != 1) {
                        var Prev = $("<a class='ah-btn-blue' onclick='RXDRUGSAVE.GetDrugsBasic(\"" + val +"\"," + (parseInt(pageIndex) - 1) + ")'>上一页</a>");
                        pageWrap.append(Prev);
                    }
                    var Next = $("<a class='ah-btn-blue' onclick='RXDRUGSAVE.GetDrugsBasic(\"" + val + "\"," + (parseInt(pageIndex) + 1) + ")'>下一页</a>");
                    pageWrap.append(Next);
                    $(".ah-rx-selectDrug").css("right", "0").find("div[ah-id='selectDrug-wrap']").html(ul).append(pageWrap);

                } else {
                    $.err("无更多数据！");
                }
            })
        } else {
            $.err("请输入药品名称 按空格分割[品名 商标/厂商]");
        }
    },
    //新增处方记录提交前验证
    RxOnsubmit: function () {
        var pic = $("#rxPic .ah-RxPic");
        if (pic.length > 0) {
            var picUrl = [];
            pic.each(function () {
                picUrl.push($(this).find("img").attr("src"));
            });
            //整理图片数据
            for (var i = 1; i < 4; i++) {
                if (picUrl[i - 1] != "undefined") {
                    $("#RxPicUrl" + i).val(picUrl[i - 1]);
                } else {
                    $("#RxPicUrl" + i).val("");
                }
            }
        }
        //判断有没有选中审核人
        if ($("#CheckDrugMan").val() != "") {
            //判断有没有选择药品
            if ($("ul[ah-id='RxDrugSaveBase_Drug'] li").length > 0) {
                return true;
            } else {
                $.err("最少添加一种药品！");
                return false;
            }
        } else {
            $.err("请选择审核人！");
            return false;
        }
    },
    //根据手机和身份证查找信息
    GetCustomersAndRelations: function (val) {
        if (val == "" || val == null) {
            $.err("请填写手机或许身份证号码!");
            return false;
        }
        $.getJSON("/api/Customer/GetCustomersAndRelations", {
            searchText: val,//搜索内容
        }, function (jn) {
            if (jn.length == 1) {
                var c = jn[0].customer;
                $("#CustomerId").val(c.customerID);
                $("#CustomerName").val(c.customerName);
                $("#CustomerMobile").val(c.customerMobile);
                $("#CustomerIdCode").val(c.iDcard);
            } else if(jn.length==0) {
                $("#CustomerId").val(0);
                $("#CustomerName").val("");
                $("#CustomerMobile").val("");
                $("#CustomerIdCode").val("");
            }
        });
    },

    loadDetail: function (rxId) {
        $.get('/Nusre/GetCompletedRxUser', { rxSaveId: rxId }, function (html) {
            $('#ah-linkbg').html(html);
        });
    }
}