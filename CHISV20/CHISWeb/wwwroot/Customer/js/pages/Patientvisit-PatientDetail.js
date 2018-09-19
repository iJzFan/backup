/*西药处方的相关操作类*/
function FormedDrugPrescription(treatId,onSave) {
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
            outUnitbId: drug.unit_b,
            outUnitsId: drug.unit_s,
            disageUnitId: drug.minUnit,

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

        $li.on('click', '[evtid="DELETDRUG"]', function () { DeleteDrug(this); });
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
                content: "/Customer/Patientvisit/selectFormedDrug",
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
            $.loadJSON("/Customer/Patientvisit/DeletePrescription", { treatId: treatId, prescriptionNo: f.prescriptionNo }, function (jn) {
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
            $.loadJSON("/Customer/Patientvisit/SaveDrugs", { drugs: drugArray, treatId: pagedata.treatId, prescriptionNo: prescriptionNo }, function (jn) {
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
        bsend && prescriptionNo && $.loadJSON("/Customer/Patientvisit/SetDrugsAsOneGroup", { treatId: pagedata.treatId, adviceIds: adviceIds, prescriptionNo: prescriptionNo }, function (jn) {
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





    //删除单个药
    function DeleteDrug(_this) {
        $.confirm("确认", "是否删除该药？", function () {
            $li = $(_this).parents('li');
            var adviceId = $li.find('[name="adviceId"]').val();
            if (adviceId) {
                //向后端发送数据验证后删除
                $.loadJSON("/Customer/Patientvisit/DeleteOneAdvice", { adviceId: adviceId, treatId: treatId }, function (jn) { if (jn.rlt) { $li.remove(); } else $.alertError(jn.msg); });
            } else $li.remove();
        })
    }



}

//该部分初始化所有开出的药品信息
function InitialTreatAdivces(treatId,onSave) {
    $.loadJSON("/Customer/Patientvisit/GetTreatAdvices", { treatId: treatId }, function (jn) {
        $.each(jn.items, function (i, item) { new FormedDrugPrescription(treatId,onSave).CreatePrescription(item); });
    });
}