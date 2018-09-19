/*西药处方的相关操作类*/
function FormedDrugPrescription(treatId, onSave) {
    var THIS = this;
    var $content = null;

    //增加一个空白的处方
    this.AddBlankPrescription = function () {
        var data = {};
        return this.CreatePrescription(data);
    }

    //创建一个处方单 或者初始化本处方单，包括绑定相关的事件
    //data{prescriptionNo:1234,items:[{},{}]}
    this.CreatePrescription = function (data) {
        //载入处方框架
        $content = $(loadTemplateHtml('Temp_Formed_CF', data));
        $content.on('click', '[evtid="ADDDRUG"]', function () { openSelectThenAddOneDrug(this); }); //增加处方
        $content.on('click', '[evtid="DELETECF"]', function () { DeleteCF(this); });//删除处方
        $content.on('click', '[evtid="SAVECF"]', function () { SavePrescription(this); });//保存处方
        $content.on('click', '[evtid="SAVEONEGROUP"]', function () { SetDrugsAsOneGroup(this); });//保存为同一组

        //载入处方药品清单
        data.drugs && AddDrugs(data.drugs);
        $('#top_box').append($content);
        return THIS;
    }
    this.Reload = function (data) {
        //重新载入处方信息
        $content.find('input[name="prescriptionNo"]').val(data.prescriptionNo);
        //重新载入药品
        $content.find("ul[rolename=boxitem-drugs]").empty();
        data.drugs && AddDrugs(data.drugs);
    }










    //添加所有药品
    function AddDrugs(drugs) {
        var groupnum = null;
        $.each(drugs, function (i, item) {
            if (item.groupNum != groupnum) {
                addline();
                groupnum = item.groupNum;
            }
            AddOneDrug(item);
        });
        if (groupnum != null) addline();

        function addline() {
            $content.find("ul[rolename=boxitem-drugs]").append("<li class='device-space'></li>"); //添加分割线
        }
    }

    //添加一个药品 返回当前药品条的jquery对象
    function AddOneDrug(drug, dataver) {

        var onedrug = {};
        if (dataver == 1) onedrug = drug;
        else onedrug = {
            adviceId: drug.adviceID,
            drugId: drug.drugID,
            drugName: drug.drugName,
            drugModel: drug.drugModel,
            salePrice: drug.price,
            outUnitbName: drug.unitPackageName,//包装单位
            outUnitName: drug.unitFormedName,//封装单位
            minUnitName: drug.minUnitName,//剂量单位
            outUnitbId: drug.unitBigId,
            outUnitsId: drug.unitSmallId,
            disageUnitId: drug.dosageUnitId,

            prescribeStyle: drug.prescribeStyle,//出药方式
            qty: drug.qty,  //数量
            amount: drug.amount,
            amountStr: drug.amountStr,//格式化好的总价字符串
            unitId: drug.unitID,
            unitName: drug.unitName,

            givenWhereTypeId: drug.givenWhereTypeId,
            givenTimeTypeId: drug.givenTimeTypeId,
            givenTakeTypeId: drug.givenTakeTypeId,
            givenDosage: drug.givenDosage,
            givenDays: drug.givenDays,
            givenRemark: drug.givenRemark,
            advice: drug.advice,
            groupNum: drug.groupNum,

            chargeStatus: drug.chargeStatus,//收费状态
            chargeStatusName: drug.chargeStatusName,
            status: drug.status,//用药状态
            useStatusName: drug.useStatusName,

            v: 1.0
        };

        var $li = $(loadTemplateHtml('Temp_Formed_CF_drugitem', onedrug));
        $content.find("ul[rolename=boxitem-drugs]").append($li);

        initialOpenType($li, onedrug);
        //设置数据
        $li.find('[name=GivenWhereTypeId]').val(onedrug.givenWhereTypeId);
        $li.find('[name=GivenTimeTypeId]').val(onedrug.givenTimeTypeId);
        $li.find('[name=GivenTakeTypeId]').val(onedrug.givenTakeTypeId);
        $li.find('[name=GivenDosage]').val(onedrug.givenDosage);
        $li.find('[name=GivenDays]').val(onedrug.givenDays);
        $li.find('[name=GivenRemark]').val(onedrug.givenRemark);
        $li.find('[name=Advice]').val(onedrug.advice);

        return $li;
    }

    function initialOpenType($li, onedrug) {
        //按照剂量 封装 包装去发药 PACKAGE：包装    FORMED;封装   DOSAGE：剂量
        //分为设置界面 和选中数据
        var f = {
            setUI: function (dd) {
                $li.find("input[name=PrescribeStyle]").val(dd.prescribeStyle);//设置开药方式数据
                $li.find('i[evtid="DRUGOPENTYPE"]').css("visibility", 'hidden');//设置所有的隐藏
                $li.find("span[name=single-dose]").css("visibility", 'hidden');//设置隐藏计量部分

                if (hasOpenDrugType(dd.b, 'DOSAGE')) { //按计量
                    var c = $li.find('i[evtid="DRUGOPENTYPE"][evtarg="DOSAGE"]');
                    c.css('visibility', 'visible');
                    var dosagediv = $li.find("span[name=single-dose]");
                    dosagediv.show(); dosagediv.find('name="dosageName"').text(dd.minUnitName);
                    dosagediv.find('name=GivenDosage').val(dd.dosageUnitId);
                }
                if (hasOpenDrugType(dd.b, 'FORMED')) { //按封装
                    var c = $li.find('i[evtid="DRUGOPENTYPE"][evtarg="FORMED"]');
                    c.css('visibility', 'visible');
                }

                if (hasOpenDrugType(dd.b, 'PACKAGE')) { //按包装
                    var c = $li.find('i[evtid="DRUGOPENTYPE"][evtarg="PACKAGE"]');
                    c.css('visibility', 'visible');

                }
                //如果数据库仲的数据有发药方式，则根据发药方式设置选择的方式
                if (dd.prescribeStyle) {
                    var c1 = $li.find('i[evtid="DRUGOPENTYPE"][evtarg="' + dd.prescribeStyle + '"]');
                    c1.parent().find("i").removeAttr('selected');
                    c1.attr('selected', 'selected');
                }

                //设置发药单位
                var $cc = $li.find("#total-dose");
                $cc.find('[name="UnitID"]').val(dd.outUnitsId);
                $cc.find('span[name=UnitName]').text(dd.outUnitName)
            },

            selectOneOpenType: function (_this) {
                var selType = $(_this).attr('evtarg');
                if (!selType) return;

                if (selType == 'DOSAGE') {
                    $(_this).parents('li').find('span[name=single-dose]').css('visibility', 'visible');
                }
                if (selType == 'FORMED' || selType == 'PACKAGE') {
                    $(_this).parents('li').find('span[name=single-dose]').css('visibility', 'hidden');
                }

                $(_this).parent().find("i").removeAttr('selected');
                $(_this).attr('selected', 'selected');

                //设置发药方式
                $li.find("input[name=PrescribeStyle]").val(selType);

                //设置发药单位
                var $cc = $li.find("#total-dose");
                $cc.find('[name="UnitID"]').val($(_this).data("unitid"));
                $cc.find('span[name=UnitName]').text($(_this).data("unitname"))
            }
        }

        var dt = {
            b: onedrug.prescribeStyles || onedrug.prescribeStyle,
            minUnitName: onedrug.minUnitName,
            dosageUnitId: onedrug.dosageUnitId,
            outUnitName: onedrug.outUnitName,
            outUnitsId: onedrug.outUnitsId,
            prescribeStyle: onedrug.prescribeStyle
        };
        f.setUI(dt);
        //绑定事件
        $li.on('click', '[evtid="DRUGOPENTYPE"]', function () { f.selectOneOpenType(this); });
        //设置初选
        if (dt.b) {
            var pstyles = dt.b.split(',');
            if (pstyles.length > 0) $li.find('[evtid="DRUGOPENTYPE"][evtarg="' + pstyles[0] + '"]').trigger('click');
        }
    }


    //增加单个药（弹出选择层）
    function openSelectThenAddOneDrug(_this) {
        var type = $(_this).parents("[rolename=boxitem]").attr("drugType");//处方类别
        if (type == "FORMED") {
            var idx = layer.open({
                type: 2,
                area: ['70%', '80%'],
                title: '添加药品',
                content: "/Doctor/selectFormedDrug",
                btn: ['确定', '取消'],
                yes: function (index, layero) {
                    //selectItem(this);
                    var iframeWin = window[layero.find('iframe')[0]['name']];
                    //执行窗口函数
                    iframeWin.SelectedDrug(function (onedrug) {
                        if (onedrug.rltStatus == "SUCCESS") {
                            layer.close(index);
                            selectItem(onedrug);
                        }
                    });
                },
                cancel: function () {
                    //右上角关闭回调
                }
            });

            function selectItem(onedrug) {
                var onedrug = $.extend(onedrug, {
                    adviceId: null,
                    qty: 1
                });
                //加入的li
                var $addli = AddOneDrug(onedrug, 1);
            }
        }
    };
    //删除处方
    function DeleteCF(_this) {
        var f = {
            prescriptionNo: $(_this).parents("[rolename=boxitem]").first().find('[name="prescriptionNo"]').val(),
            deleteFm: function () { $(_this).parents('div[rolename=boxitem]').remove(); }
        }
        if (!f.prescriptionNo) f.deleteFm();
        f.prescriptionNo && $.confirm("确认", "是否删除该处方？", function () {
            $.loadJSON("/Doctor/DeletePrescription", { treatId: treatId, prescriptionNo: f.prescriptionNo }, function (jn) {
                if (jn.rlt) f.deleteFm();
                else $.alertError("删除处方出现错误：" + jn.msg);
            });
        })
    }


    //保存本处方
    function SavePrescription(_this) {
        $drugs = $(_this).parents("[rolename=boxitem]").find("ul[rolename=boxitem-drugs]>li[rolename=boxitem-drugs-item]");
        if ($drugs.length > 0) {
            var drugArray = new Array();
            $.each($drugs, function (i, m) {
                //获取数据
                var d = {
                    adviceId: $(m).find('[name="adviceId"]').val(),
                    drugId: $(m).find('[name="drugId"]').val(),
                    GivenWhereTypeId: $(m).find('[name="GivenWhereTypeId"]').val(),
                    GivenTimeTypeId: $(m).find('[name="GivenTimeTypeId"]').val(),
                    GivenTakeTypeId: $(m).find('[name="GivenTakeTypeId"]').val(),
                    GivenDosage: $(m).find('[name="GivenDosage"]').val(),
                    GivenDays: $(m).find('[name="GivenDays"]').val(),
                    GivenRemark: $(m).find('[name="GivenRemark"]').val(),
                    Advice: $(m).find('[name="Advice"]').val(),
                    Qty: $(m).find('[name="Qty"]').val(),//发药数量
                    UnitID: $(m).find('[name="UnitID"]').val(),//发药单位
                    PrescribeStyle: $(m).find('[name=PrescribeStyle]').val(),//发药方式选择
                }
                if (!d.drugId) {

                    return false;
                }
                drugArray.push(d);
            });
            var prescriptionNo = $(_this).parents("[rolename=boxitem]").first().find('[name="prescriptionNo"]').val();
            //  alert(prescriptionNo);
            $.loadJSON("/Doctor/SaveDrugs", { drugs: drugArray, treatId: pagedata.treatId, prescriptionNo: prescriptionNo }, function (jn) {
                if (jn.rlt) {
                    $.msg("保存成功")
                    //刷新替换这个处方位置
                    THIS.Reload(jn);
                    if (onSave) onSave();
                } else { $.alertError("保存处方失败:" + jn.msg); }
            });
        }
    }

    //设置为同组药品
    function SetDrugsAsOneGroup(_this) {
        //检测保存后再设置为同组
        var adviceIds = new Array();
        var checkeditems = $(_this).parents('[rolename="boxitem"]').find("ul[rolename=boxitem-drugs]>li .chkdrug-item:checked");
        var bsend = checkeditems.length > 0;
        $.each(checkeditems, function (i, item) {
            var i = $(item).val();
            if (!i) { $.alertMsg("请先保存后设为同一组药"); bsend = false; return false; }
            else adviceIds.push(i);
        });
        //获取到设置同组的AdviceId
        var prescriptionNo = $(_this).parents("[rolename=boxitem]").first().find('[name="prescriptionNo"]').val();
        bsend && prescriptionNo && $.loadJSON("/Doctor/SetDrugsAsOneGroup", { treatId: pagedata.treatId, adviceIds: adviceIds, prescriptionNo: prescriptionNo }, function (jn) {
            //设置为同组后的操作
            $.msg("设置成功");
            THIS.Reload(jn);
        });
    }



    //获取开药的类别
    function hasOpenDrugType(pkstring, type) {
        if (!pkstring) return false;
        return (pkstring.indexOf(type) >= 0);
    }
}

//该部分初始化所有开出的药品信息
function InitialTreatAdivces(treatId, onSave) {
    $.loadJSON("/Doctor/GetTreatAdvices", { treatId: treatId }, function (jn) {
        $.each(jn.items, function (i, item) {

            new FormedDrugPrescription(treatId, onSave).CreatePrescription(item);
        });
    });
}


function modifyCustomerInfo(cusid) {
    var handle = $.open({
        type: 2, title: "完善患者信息", btn: ['确定', '取消'],
        content: '/Code/CustomerEdit?op=MODIFYF&recid=' + cusid,
        yes: function (index, layero) {
            var win = top.window[layero.find('iframe')[0]['name']];
            win.submitForm({
                sendSuccess: function (jn) {
                    if (jn.rlt) { $.ok("修改成功！"); top.layer.close(handle); }
                    else $.err("错误！" + jn.msg);
                }
            });
        }
    });
}



















/*===================================================成药处方操作部分======== START ================================*/
var FORMED = {

    AddOneFormedDrug: function (_this) {
        var $top = $(_this).parents("[rolename=boxitem]");
        var type = $top.attr("drugType");//处方类别
        if (type == "FORMED") {
            var idx = $.open({
                type: 2,
                title: '添加药品',
                content: "/Doctor/selectFormedDrug",
                btn: ['确定', '取消'],
                yes: function (index, layero) {
                    //selectItem(this);
                    var iframeWin = window[layero.find('iframe')[0]['name']];
                    //执行窗口函数
                    iframeWin.SelectedDrug(function (onedrug) {
                        if (onedrug.rltStatus == "SUCCESS") {
                            layer.close(index);

                            //  selectItem(onedrug);
                            //添加一个药品
                            $.post("/Doctor/AddOneFormedDrug", { drugId: onedrug.drugId }, function (html) {
                                $top.find('ul[rolename="boxitem-drugs"]').append(html);
                                $.updataAllLayui();
                            });
                        }
                    });
                },
                cancel: function () {
                    //右上角关闭回调
                }
            });
        }
    },
    //保存处方
    SaveFormedPrescription: function (_this) {
        var form = FORMED.getForm(_this);
        $.SortFormArray(form.find('[rolename="boxitem-drugs-item"]'));//规整集合
        var t = form.serialize();
        $.post("/Doctor/SaveFormedPrescription", t, function (jn) {
            if (jn.rlt) {
                $.post(jn.SuccessThenCallUrl, function (html) {
                    $.ok("保存成功！");
                    form.before(html);
                    form.remove();
                    RefreshSumaryFee();//刷新费用摘要
                    $.updataAllLayui();//更新layui
                });
            } else {
                $.err("保存失败，请重试！");
            }
        });
    },
    //获取最高form层级
    getForm: function (_this) {
        var section = $(_this);
        if ($(_this).attr("role") != "DRUGS") {
            section = $(_this).parents("section[role=DRUGS]").first();
        }

        if (section.parent().get(0).tagName != "FORM") {
            var form = $('<form  method="post"></form>');
            section.wrap(form);
        }
        form = section.parent();
        return form;
    },
    //设置为同组药
    SetFormedDrugSameGroup: function (_this) {
        //检测保存后再设置为同组
        var form = FORMED.getForm(_this);
        var $top = form.find('[rolename="boxitem"]').first();//最高级的控件
        var adviceIds = new Array();
        var checkeditems = $top.find("ul[rolename=boxitem-drugs]>li .chkdrug-item:checked");
        var bsend = checkeditems.length > 0;
        $.each(checkeditems, function (i, item) {
            var i = $(item).val();
            if (!i || i == "0") { $.alertMsg("请先保存后设为同一组药"); bsend = false; return false; }
            else adviceIds.push(i);
        });

        //获取到设置同组的AdviceId
        var prescriptionNo = $top.find('[d-name=prescriptionNo]').val();



        bsend && prescriptionNo && $.loadJSON("/Doctor/SetDrugsAsOneGroup", { treatId: pagedata.treatId, adviceIds: adviceIds, prescriptionNo: prescriptionNo }, function (jn) {
            //设置为同组后的操作
            if (jn.rlt) {
                $.msg("设置成功");
                $.post(jn.SuccessThenCallUrl, function (html) {
                    form.before(html);
                    form.remove();
                    $.updataAllLayui();
                });
            }
        });
    },
    DeleteFormedPrescription: function (_this) {
        //检测保存后再设置为同组
        var form = FORMED.getForm(_this);
        var $top = form.find('[rolename="boxitem"]').first();//最高级的控件

        var f = {
            prescriptionNo: $top.find('[d-name=prescriptionNo]').val(),
            deleteFm: function () { form.remove(); }
        }
        if (!f.prescriptionNo) f.deleteFm();
        f.prescriptionNo && $.confirm("确认", "是否删除该处方？", function () {
            $.loadJSON("/Doctor/DeleteFomedPrescription", { treatId: pagedata.treatId, prescriptionNo: f.prescriptionNo }, function (jn) {
                if (jn.rlt) f.deleteFm();
                else $.alertError("删除处方出现错误：" + jn.msg);
            });
        })

    },
    //计算总价
    calcAmount: function ($top) {

        var amount = 0;
        //  $('#FormedMain_Amount').val(amount);
    },

    //删除单个成药
    DeleteFormedDrug: function (_this) {
        var li = $(_this).parents("li[rolename = boxitem-drugs-item]").first();
        //获取药品ID
        var adviceFormedId = li.find("input#AdviceFormedId").val();

        $.confirm("确认", "是否删除该药？", function () {
            if (!adviceFormedId || adviceFormedId == '0') li.remove();
            else {
                $.loadJSON("/Doctor/DeleteOneAdvice", { treatId: pagedata.treatId, adviceFormedId: adviceFormedId }, function (jn) {
                    if (jn.rlt) li.remove();
                    else $.alertError("删除处方出现错误：" + jn.msg);
                });
            }
        })
    },
    formedShowMore: function (_this) {
        var _this = $(_this);
        var more = _this.parents("li").first().find(".ah-formed-more-info");
        var icon = _this.find("i").first();
        if (more.is(":hidden")) {
            more.css("display", "inherit");
            icon.attr("class", "glyphicon glyphicon-menu-down")
        } else {
            more.css("display", "none");
            icon.attr("class", "glyphicon glyphicon-menu-right")
        }
    },
    ShowDrugDetail: function (drugid, _this) {

        var url = '/MedicalLib/DrugDetail?drugid=' + drugid;
        $.open({
            type: 2,
            title: false,
            content: url,
            area: ["280px", "500px"]
        })

    },
    ver: 0
}










/*===================================================成药处方操作部分======== END ================================*/







/*===================================================中药处方操作部分======== START ================================*/
var HERB = {

    getForm: function (_this) {
        var section = $(_this).parents("section[role=HERBS]").first();
        if (section.parent().get(0).tagName != "FORM") {
            var form = $('<form  method="post"></form>');
            section.wrap(form);
        }
        return section.parent();
    },
    //删除中药处方
    DelHerbPrescription: function (_this) {
        $.confirm("确认", "是否删除该处方？", function () {
            var form = HERB.getForm(_this);
            var section = $(_this).parents("section[role=HERBS]").first();
            var prescriptionNo = section.find("input[name='Main.prescriptionNo']").val();
            if (prescriptionNo == "00000000-0000-0000-0000-000000000000" || !prescriptionNo) {
                form.remove();
            } else {
                $.loadJSON("/Doctor/DeleteHerbAdvice", { prescriptionNo: prescriptionNo }, function (jn) {
                    if (jn.rlt) {
                        $.ok("删除成功！");
                        section.remove();
                    } else {
                        $.err("删除失败，请重试！");
                    }
                });
            }
        });
    },
    //保存中药处方
    SaveHerbPrescription: function (_this) {
        var form = HERB.getForm(_this);
        $.SortFormArray(form.find('[rolename="ZDRUG"]'));//规整集合
        var t = form.serialize();
        $.post("/Doctor/SaveHerbAdvice", t, function (jn) {
            if (jn.rlt) {
                $.post(jn.SuccessThenCallUrl, function (html) {
                    $.ok("保存成功！");
                    form.before(html);
                    form.remove();
                    RefreshSumaryFee();//刷新费用摘要
                    $.updataAllLayui()
                });
            } else {
                $.err("保存失败，请重试！");
            }
        });
    },
    /*中药搜索和代码自动补全*/
    AddHerbal: function (_this) {
        var tData = [];
        var $this = $(_this);
        $("input[myrole=zDrugSearchList]").autocomplete({
            source: function (request, response) {
                $.post("/Doctor/Json_GetHerbsInfos", {
                    maxRows: 20,
                    drugFrom: ["0-0"],//本地中药库
                    term: request.term
                }, function (data) {
                    response(data.items);
                });
            },
            mustMatch: false,
            minLength: 2,
            focus: function (event, ui) {
                $this.val(ui.item.label);
                $this.prev("input[type='hidden']").val(ui.item.value);
                return false;
            },
            select: function (event, ui) {
                selectHerb(ui.item);
                return false;
            }
        });
        function selectHerb(jn) {
            if (jn != null) {
                $.post("/Doctor/SingHerbalInfor", { drugId: jn.value }, function (html) {
                    var _pDiv = $this.parents("[rolename=ZDRUG]");
                    var $html = $(html.trim());
                    _pDiv.before($html);
                    _pDiv.find('#zDrugName').val('');//清空数字
                    $html.find('input[tag=Herb_Num]').val('1').focus().select();
                    $(_this).val('');
                });
            } else $.alertError("没有选择药品!");
        };
    },
    /*计算中药的小计函数 失去焦点*/
    SingelHerbalSubtotalBlur: function (_o) {
        var $top = $(_o).parents("[rolename=ZDRUG]").first();
        var $o = $(_o);
        var m = parseFloat($top.find("input[id='SinglePrice']").val());
        var n = parseFloat($(_o).val());
        if (!n) n = 0;
        var tol = parseFloat((m * n)).toFixed(3);
        var s = $($o.parents(".herb-flex-data").find("small[id='subtoal']"));
        s.empty();
        s.text(tol);
    },
    /*删除单个中药*/
    deleteSingleZDrug: function (_this) {
        $.confirm("确认", "是否删除该药？", function () {
            $div = $(_this).parents("[rolename=ZDRUG]");
            $div.remove();
        });
    }, onlyNum: function (_this) {
        if (_this.value.length == 1) {
            _this.value = _this.value.replace(/[^1-9]/g, '')
        } else {
            _this.value = _this.value.replace(/\D/g, '')
        }
    },
    onlyNumOnafterpaste: function (_this) {
        if (_this.value.length == 1) {
            _this.value = _this.value.replace(/[^1-9]/g, '0')
        } else {
            _this.value = _this.value.replace(/\D/g, '')
        }
    },



    ver: 0
}



/*===================================================中药处方操作部分======== END ================================*/





/*===================================================其他收费======== START ================================*/

var OTHERFEE = {
    addExtraFees: function (_this) {
        $.open({
            type: 2,
            title: "新增其他收费",
            area: ["600px", "360px"],//内容过少 可以固定大小
            content: "/Doctor/AddExtraFees",
            btn: ['确定', '取消'],
            yes: function (index, layero) {
                var body = layer.getChildFrame('body', index);
                var section = body.find("section[ah-id=addOtherFeesForm]").first();
                var data = {
                    TreatFeeTypeId: section.find("[ah-id=TreatFeeTypeId]").val(),//费用类型
                    Qty: section.find("[ah-id=Qty]").val(),//数量
                    TreatFeeOriginalPrice: section.find("[ah-id=TreatFeeOriginalPrice]").val(),//原价格
                    TreatFeePrice: section.find("[ah-id=TreatFeePrice]").val(),//价格
                    Amount: section.find("[ah-id=Amount]").val(),//小计
                    FeeRemark: section.find("[ah-id=FeeRemark]").val(),//费用说明
                }
                $.loadJSON("/Doctor/SaveExtraFee", { extraFee: data, treatId: pagedata.treatId }, function (jn) {
                    if (jn.rlt) {
                        $.post(jn.SuccessThenCallUrl, function (html) {
                            $.ok("保存成功！");
                            $(_this).parents("[ah-id=pvExtraFeesForm]").html(html);
                            RefreshSumaryFee();//刷新费用摘要
                            layer.close(index);
                        });
                    } else {
                        $.err("保存失败，请重试！" + jn.msg);
                    }
                });
            }
        })
    },
    addOtherFeesForm: function (_this) {
        var section = $(_this).parents("section[ah-id=addOtherFeesForm]").first();


    },
    changeTreatFeeType: function (_this) {
        var val = $(_this).val();
        if (val == "" || val == null) return;
        $.ajax({
            url: "/Doctor/LoadExtraFeeInfo",
            dataType: "json",
            data: {
                extraFeeTypeId: val,
                treatId: pagedata.treatId
            },
            success: function (jn) {
                var section = $(_this).parents("section[ah-id=addOtherFeesForm]").first();
                section.find("[ah-id=Qty]").val(jn.item.qty);
                section.find("[ah-id=TreatFeeOriginalPrice]").val(jn.item.treatFeeOriginalPrice);//原价格
                section.find("[ah-id=TreatFeePrice]").val(jn.item.treatFeePrice);//价格
                section.find("[ah-id=Amount]").val(jn.item.amount);//小计
                section.find("[ah-id=FeeRemark]").val(jn.item.feeRemark);//费用说明 
            }
        });
    },
    //删除
    deleteExtraFeeItem: function (extraFeeId, _this) {
        $.confirm("确认", "是否删除？", function () {
            $.loadJSON("/Doctor/RemoveExtraFee", { extraFeeId: extraFeeId }, function (jn) {
                if (jn.rlt) {
                    $.ok("删除成功！");
                    $(_this).parent().parent().remove();
                    RefreshSumaryFee();//刷新费用摘要
                } else {
                    $.err("删除失败，请重试！" + jn.msg);
                }
            });
        });
    }
}

/*===================================================其他收费======== END ================================*/

//保存接诊信息
function SaveTreatData(callback) {
    $.loadJSON("/Doctor/TempSaveTreat", { viewdata: $.form2Json('#treat_data'), treatId: pagedata.treatId },
        function (jn) {
            if (jn.rlt) {
                callback();
            } else $.alertError(jn.msg);
        });
}







/** *******************************成药js ST****************************************************************************** */
function AddFormedPrescription() {
    $.post("/Doctor/NewFormedPrescription", { treatId: pagedata.treatId }, function (html) {
        var $html = $(html.trim());
        $('#top_box').prepend($html);
    });
}

/** *******************************成药js EN****************************************************************************** */

/** *******************************中药js ST****************************************************************************** */
//增加中药处方
function AddHerbPrescription() {
    $.post("/Doctor/NewHerbPrescription", { treatId: pagedata.treatId }, function (html) {
        $('#top_box_herbs').prepend(html);
        $.updataAllLayui();
    });
};
/** *******************************中药js EN****************************************************************************** */


$(function () {
    $('a[target=modalOpen]').click(function (event) {
        event.preventDefault();
        var w = $(this).data("winwidth");
        var h = $(this).data("winheight");
        var href = $(this).attr("href");
        pagedata.currentModalWin = top.layer.open({
            type: 2,
            title: '人员信息修改',
            shadeClose: true,
            shade: 0.8,
            area: [w, h],
            btn: ["确定", "取消"],
            yes: function (index, layero) {
                var win = top.window[layero.find('iframe')[0]['name']];
                win.submitForm({ success: function () { top.layer.close(index); } });
            },
            content: href //iframe的url
        });
    });
    /*初始化编辑控件*/
    $('.ah-patient-input-wrap input').blur(function () {
        var key = $(this).attr("ah-input-key");
        var v = $(this).val();
        //向后台发送数据并更新
        $.loadJSON("/Doctor/UpdateInputData?treatId=" + pagedata.treatId, { key: key, val: v }, function (jn) {
            if (!jn.rlt) $.alertError(jn.msg);
        });
    });

    /*药品输入信息*/
    $('#top_box').on("focus", "[rolename=boxitem-drugs-item] input[type=text]", function () {
        $(this).select();
    });

    /*主次诊断*/
    $("div[myrole=diagnosisinput]").click(function () {
        $.get("/Doctor/GetDiagnosis", { actionID: $(this).attr("id") }, function (html) {
            var opt = {
                type: 1,
                area: ['740px', '465px'],
                title: '诊断搜索',
                content: html,
            }
            if ($(document).height() <= 800) {
                opt.area = ['740px', '400px'];
            }
            $.open(opt);
        });
    });

    //当诊断失去焦点的时候需要检查诊断数据的一致性
    $('input[myrole="diagnosisinput"]').blur(function () {
        var $that = $(this);
        var $id = $that.prev('input[type=hidden]').first();
        var name = $that.val().trim();
        if (!$id.val()) { $that.val(''); return; }
        $.getJSON("/openapi/treat/GetDiagnosisById", { zdid: $id.val() }, function (jn) {
            //if (jn.itemName != name) {
            //    $.msg("请从下拉诊断条目内选择");
            //    $that.val('');
            //    $id.val('');
            //}
        });
    });

    $('a[ahd-role="RefreshFeeSumary"]').click(function () {
        $('#fee_sumary').load("/doctor/LoadFeeSumary", { treatId: pagedata.treatId });
        $.post("/doctor/Json_PrescriptionNum", { treatId: pagedata.treatId }, function (jn) {
            if (jn.rlt) {
                $('#formed_pre_num').text(jn.formedNum).removeClass("ah-num-mk-0").addClass("ah-num-mk-" + (jn.formedNum > 0 ? 1 : 0));
                $('#herb_pre_num').text(jn.herbNum).removeClass("ah-num-mk-0").addClass("ah-num-mk-" + (jn.herbNum > 0 ? 1 : 0));
            }
            else $.err("错误：" + jn.msg);
        });
    });

    $(".ah-data-label i").click(function () {
        var tips = $(this).parent().parent().find(".tips");
        if (tips.is(":hidden")) {
            tips.css("display", "inline-block");
            $(this).attr("class", " glyphicon glyphicon-circle-arrow-up");
        } else {
            tips.css("display", "none");
            $(this).attr("class", "glyphicon glyphicon-info-sign");
        }
    });

    
    /*药品剂量的输入计算监控*/
    $('ul[rolename="boxitem-drugs"] #GivenDays,ul[rolename="boxitem-drugs"] #GivenDosage').on("keyup", function () {
        calcNum(this);
    });
    

    $('input[type=text],input[type=number],textarea').on("focus", function () {
        var that = this;
        var cHeight = $(window).height();
        setTimeout(function () {
            var ch = $(window).height();
            var myTop = $(that).offset().top;
            if (ch < cHeight) {
                //需要移动到视口顶部
                $('body>div').first().scrollbar.scrolly(100)
            }
        }, 1000);
    });
    pagedata.main.scroll(function () {
        if (pagedata.main.get(0).scrollHeight - pagedata.main.height() - pagedata.main.scrollTop() < 0) {
            $(".ah-patient-tofoot").addClass("ah-hide");
        } else {
            $(".ah-patient-tofoot").removeClass("ah-hide");
        }
    });

});

//滚动到底部
function foot() {
    var h = $('body[ah-body="wrap"]').find(".ah-main-content").first().height();//滚动条总高度
    pagedata.main.scrollTop(h);//滚动到底部
}
/*选择不同的单位*/
function selectUnitId(_this) {
    console.log(_this);
    var val = $(_this).val();
    $p = $(_this).parents("li")
    $p.find('#UnitId').val(val);
}
//计算数量
function calcNum(_this) {
    var $p = $(_this).parents('li').first();
    var fz = $p.find("#GivenTakeTypeId").val();
    var days = parseFloat($p.find('#GivenDays').val());//天数
    var dosage = parseFloat($p.find('#GivenDosage').val());//剂量
    var dos_u = parseFloat($p.find('#DosageContent').val());//剂量单位
    //翻译频次到每日
    var dfz = null;
    switch (parseInt(fz)) {
        case 12786: dfz = 3; break;
        case 12837: dfz = 1; break;
        case 12839: dfz = 2; break;
        case 12840: dfz = 4; break;
        case 12845: dfz = 1; break;
        case 12846: dfz = 0.5; break;
        case 12848: dfz = 2 / 7; break;
        case 12848: dfz = 1; break;
    }

    var v = dfz * days * dosage;//总剂量
    if (isNaN(v) || isNaN(dos_u) || dfz == null) return;
    if (v > 0 && dos_u > 0) {
        var val = Math.ceil(v / dos_u);
        console.log(val);
        $p.find('#Qty').val(val);//设置数量
        var u = $p.find('input[type=radio][ah-u="UNITS"]');
        u.prop("checked", true);//set small unit select
        $p.find("#UnitId").val(u.val());//set drug unit
        layui.form.render();
    }
}

//接诊按钮命令
function treatCMD(type) {
    switch (type) {
        case 0://取消接诊
            $.confirm("取消接诊", "是否取消本次接诊，取消后则接诊处于未接诊状态。", function () {
                $.loadJSON("/Doctor/CancelTreat", { treatId: pagedata.treatId }, function (jn) {
                    if (jn.rlt) { window.location.href = "/Doctor/Patientvisit"; }
                    else $.alertError(jn.msg);
                })
            })
            break;
        case 3://暂存 静默
            $.loadJSON("/Doctor/TempSaveTreat", { viewdata: $.form2Json('#treat_data'), treatId: pagedata.treatId },
                function (jn) {
                    if (jn.rlt) {
                    } else $.alertError(jn.msg);
                });
            break;
        case 1://暂存接诊
            SavePrescriptionTemp();
            $.loadJSON("/Doctor/TempSaveTreat", { viewdata: $.form2Json('#treat_data'), treatId: pagedata.treatId },
                function (jn) {
                    if (jn.rlt) {
                        $.confirm("暂存成功", "暂存成功，是否返回列表", function () {
                            $.loadUrl("/Doctor/Patientvisit");
                        })
                    } else $.alertError(jn.msg);
                });
            break;
        case 2://接诊完毕
            /*检查是否有没有保存的信息*/
            try {
                var haveerror = false;
                $.each($('#top_box [rolename=boxitem] input[name=adviceId]'),
                    function (i, item) {
                        if (!$(item).val()) {
                            $(item).parents("li").css("border", "1px solid red");
                            haveerror = true;
                        }
                    });
                if (haveerror) throw "还有没有保存的处方，请先保存。";
            } catch (ex) { $.alertWarning(ex); return; }

            $.loadJSON("/Doctor/TreatFinished", { viewdata: $.form2Json('#treat_data'), treatId: pagedata.treatId },
                function (jn) {
                    if (jn.rlt) {
                        //如果是全科，则弹出询问框
                        var havePay = parseFloat($('#sm_NeedPayAmount').text()) > 0;
                        if (pagedata.isCanCharge && havePay) {
                            $.confirm('是否直接收费', '您是全科医生可直接收费，是否前往？', function () {
                                $.loadUrl("/Charge/CHIS_Charge?pagefn=23&treatId=" + pagedata.treatId);
                            }, function () {
                                TreatFinishedClose();
                            });
                        } else {
                            TreatFinishedClose();
                        }
                    } else $.alertError(jn.msg);
                });
            function TreatFinishedClose() {
                $.ok("接诊成功，返回接诊列表！");
                $.loadUrl("/Doctor/Patientvisit");
            }
            break;
    }
    //检查处方是否都保存了
    function SavePrescriptionTemp() {
        try {
            $('section[role=DRUGS]').each(function (i, item) {
                if (!($(item).find('[d-name=prescriptionNo]').val())) throw { val: 1, item: item };
                $(item).find('li[rolename="boxitem-drugs-item"]').each(function (j, m) {
                    var v = $(m).find('#AdviceFormedId').val();
                    if (!v || v == "0") throw { val: 1, item: item };
                });
            });
            $('section[role=HERBS]').each(function (i, item) {
                if (!($(item).find('[d-name=prescriptionNo]').val())) throw { val: 2, item: item };
                $(item).find('[ah-rolename="boxitem-herb-item"]').each(function (j, m) {
                    var v = $(m).find('#Id').val();
                    if (!v || v == "0") throw { val: 2, item: item };
                });
            });
        }
        catch (e) {
            if (e.val == 1) SaveFormedCF(e.item);
            if (e.val == 2) SaveHerbCF(e.item);
        }

        function SaveFormedCF(item) {
            //console.log("save formed")
            //  $(item).find('a[ah-roleid="SaveFormedCF"]').click();
        }
        function SaveHerbCF(item) {
            // console.log("saveherb")
            //  $(item).find('a[ah-roleid="SaveHerbCF"]').click();
        }

    }

}

//刷新费用摘要
function RefreshSumaryFee() {
    $('a[ahd-role="RefreshFeeSumary"]').click();
}


function setHerbInit($c) {
    var $gr = $c.find('#Main_GivenRemark');
    if ($gr.val() == "") $gr.val("水煎服");
}
function sendNewHerbInput(e) {
    if (e.keyCode == 13) {
        var n = $(e.target).parents('[rolename="ZDRUG"]').first();
        var next = $(e.target).parents('[rolename="ZDRUG"]').first().nextAll('[rolename="ZDRUG"]').first();
        var np = next.find("#zDrugName");
        if (np.size() == 0) {
            next.find('#Qty').focus();
        } else next.find("#zDrugName").focus();

    }
}
function isShowPatientDetailBase(state, dom) {
    if (state) {
        $(dom).show();
    } else {
        $(dom).hide();
    }
}