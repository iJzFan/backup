var DISPENSING = {
    //获取搜索条件
    SearchData: function () {
        var w = $('div[ah-id="DISPENSINT"]').first();
        var data = {
            dispensingStatus: w.find('input[name="dispensingStatus"]:checked').val(),
            startDate: w.find('input[ah-id="startDate"]').first().val(),
            endDate: w.find('input[ah-id="endDate"]').first().val(),
            searchText: w.find('input[ah-id="searchText"]').first().val(),
        }
        return data
    },
    //刷新列表
    GetSendList: function () {
        var data = this.SearchData();
        $.post("/Dispensing/GetSendList", {
            searchText: data.searchText,
            dispensingStatus: data.dispensingStatus,
            tstart: data.startDate,
            tend: data.endDate
        }, function (html) {
            $('div[ah-id="DISPENSINT-TABLE"]').html(html);
        });
    },
    //添加备注
    addDispensingRmk: function (_this) {
        var tr = $(_this).parents("tr").first();
        var drugType = tr.attr("ah-type");//药品类型
        var drugId = tr.attr("ah-drugId");//药品ID
        var dbid = tr.attr("ah-dbid");//药品数据库ID
        $.open({
            type: 2,
            title: '未发药备注',
            area: ['500px', 'auto'],
            content: ["/dispensing/DispensingRmk?drugType=" + drugType + "&drugId=" + drugId + "&dbid=" + dbid],
        });

    },
    //提交备注
    SetDispensingStatus: function (drugType, drugId, dbid, Rmk, state) {
        $.ajax({
            url: "/Dispensing/SetDispensingStatus",
            dataType: "json",
            data: {
                drugType: drugType,
                drugId: drugId,
                dbid: dbid,
                Rmk: Rmk,
                state: state,
            },
            success: function (jn) {
                if (jn.rlt) {
                    var tr = $("tr[ah-dbid='" + dbid + "']");
                    var Status;
                    if (state === 2) {
                        tr.attr("class", "ah-drug-despnesing-state-2");
                        Status = "待退";
                    } else {
                        tr.attr("class", "ah-drug-despnesing-state-0");
                        Status = "待发";
                    }
                    tr.find("td[ah-route='Status']").html(Status);
                    tr.find("td[ah-route='Rmk']").html(Rmk);

                } else {
                    layer.msg(jn.msg);
                }
            }
        });
    },
    //中草药全部派发
    SendDrugLocalHerb: function (_this) {
        var section = $(_this).parents('table[ah-id="herb-section"]');//获取父节点
        var treatId = section.attr("ah-treatId");
        var PrescriptionNo = section.attr("ah-PrescriptionNo");
        var drugs = new Array();
        section.find(".ah-drug-despnesing-state-0").each(function () {
            drugs.push(parseInt($(this).attr("ah-drugId")));
        });
        $.post("/Dispensing/SendDrug_LocalHerb", {
            drugs: drugs,
            treatId: treatId,
            prescriptionNo: PrescriptionNo
        }, function (jn) {
            if (jn.rlt) {
                $.ok("发药成功！");
                console.log("123123");
                $.post("/Dispensing/LoadDispensingDetailsOfStore", {
                    sourceFrom: 0,
                    treatId: treatId,
                    supplierId: 0
                }, function (html) {
                    console.log("23232");
                    console.log(html)
                    var content = section.parents(".ah-drug-table").first();
                    content.before(html);
                    content.remove();
                });
            } else {
                layer.msg(jn.msg);
            }
        },"json");
    },
    //成药全部传送
    SendDrugLocalFormed: function(_this) {
        var section = $(_this).parents('table[ah-id="formed-section"]');//获取父节点
        var treatId = section.attr("ah-treatId");
        var supplierId = section.attr("ah-supplierId");
        var PrescriptionNo = section.attr("ah-PrescriptionNo");
        var drugs = new Array();
        section.find(".ah-drug-despnesing-state-0").each(function () {
            drugs.push(parseInt($(this).attr("ah-drugId")));
        });
        $.post("/Dispensing/SendDrug_LocalFormed", {
            drugs: drugs,
            treatId: treatId,
            supplierId: supplierId,
            prescriptionNo: PrescriptionNo
        }, function (jn) {
            if (jn.rlt) {
                $.ok("发药成功！");
                $.post("/Dispensing/LoadDispensingDetailsOfStore", {
                    sourceFrom: 0,//载入本地药房信息
                    treatId: treatId,
                    supplierId: 0
                }, function (html) {
                    console.log(html);
                    var content = section.parents(".ah-drug-table").first();
                    content.before(html);
                    content.remove();
                });
            } else {
                layer.msg(jn.msg);
            }
        }, "json");
    },
    SendDrugWebAll: function (_this) {
        try {
            var section = $(_this).parents('table[ah-id="formed-section"]');//获取父节点
            var treatId = section.attr("ah-treatId");
            var supplierId = section.attr("ah-supplierId");
            var sendRmk = section.find("input[ah-id=SendRmk]").val();//传送的备注
             
            var drugs = new Array();
            section.find(".ah-drug-despnesing-state-0").each(function () {
                drugs.push(parseInt($(this).attr("ah-drugId")));
            });
            $.post("/Dispensing/SendDrug_Web", {
                drugs: drugs,
                treatId: treatId,
                supplierId: supplierId,
                SendRmk: sendRmk
            }, function (jn) {
                if (jn.rlt) {
                    $.ok("发药成功！");
                    $.post("/Dispensing/LoadDispensingDetailsOfStore", {
                        sourceFrom: 1,
                        treatId: treatId,
                        supplierId: 3
                    }, function (html) {
                        console.log(html);
                        var content = section.parents(".ah-drug-table").first();
                        content.before(html);
                        content.remove();
                    });
                } else {
                    layer.msg(jn.msg);
                }
            }, "json");
        }
        catch (ex) { $.err(ex);}
    },
    RefreshDispensingMonitor: function (_this) {        
        $('#sumary').load("/Dispensing/LoadDispensingDetailSumary", { treatId: pagedata.treatId });
    }
}
 

