//$(function () {
//    var txt = $("#setAddr").text();
//    if (txt.indexOf("设置地址")) {
//        $("#setAddr").text().replace("设置地址", "<a herf='#'>设置地址</a>");
//    };
//    //显示用户地址列表
//    showCustAddrList();
//    //加载省份
//    getProvinces();
//    //清空表单
//    clearInputValue

//});


//通过GetJson 获取省份的列表
function getProvinces(func) {

    $.loadJSON("/api/common/GetAreas", function (jn) {
        if (jn.rlt) {
            for (var i = 0; i < jn.items.length; i++) {
                $("#_cmbProvince").append("<option value=" + jn.items[i].areaId + ">" + jn.items[i].name + "</option>");
            }
            //选择省
            $('#_cmbProvince').change(function () {
                var areaId = $('#_cmbProvince').val();
                getCitys(areaId);
            });
            if (func) func();
        }
    });
}
//areaId 获取市级的列表
function getCitys(aId, func) {
    var d = { parentId: aId };
    $.loadJSON("/api/common/GetAreas", d, function (jn) {
        if (jn.rlt) {
            for (var i = 0; i < jn.items.length; i++) {
                $("#_cmbCity").append("<option value=" + jn.items[i].areaId + ">" + jn.items[i].name + "</option>");
            }
            //选择县/区
            $('#_cmbCity').change(function () {
                var areaId = $('#_cmbCity').val();
                getAreas(areaId);
            });
            if (func) func();
        }
    });
}
//获取区/县
function getAreas(aId, func) {
    var d = { parentId: aId };
    $.loadJSON("/api/common/GetAreas", d, function (jn) {
        if (jn.rlt) {
            for (var i = 0; i < jn.items.length; i++) {
                $("#_cmbArea").append("<option value=" + jn.items[i].areaId + ">" + jn.items[i].name + "</option>");
            }

            $('#_cmbArea').change(function () {
                var areaId = $('#_cmbArea').val();
                $("#AreaId").val(areaId)
            });
            if (func) func();
        }
    });
}
//显示用户地址列表
function showCustAddrList() {
    $.post("/Customer/home/GetViewManageAddressList", {}, function (html) {
        $('#Address_List').html(html);
    });
}

//添加用户地址
function AddAddr() {
    //表单验证
    var validator1 = validator_addressmobile();
    var validator2 = validator_addressdetail();
    var validator3 = validator_contactname();
    var validator4 = validator_cty();
    if (validator1 && validator2 && validator3 && validator4) {
        var d = {
            addressId: $("#AddressId").val(),
            areaId: $("#AreaId").val(),
            addrDetail: $("#AddressDetail").val(),
            custName: $("#ContactName").val(),
            mob: $("#Mobile").val(),
            isDefault: $("#IsDefault").is(":checked")
        }
        $.getJSON("/Customer/basejson/Address/AddCustAddr", d, function (jn) {
            if (jn.rlt) {
                $.alertMsg(jn.msg);
                clearInputValue();
                showCustAddrList();
            } else $.alertError(jn.msg);
        });
    }
}

//表单验证
function validator_addressmobile() {
    var mobilereg = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$/;
    var mobile = $.trim($("#Mobile").val());
    if (!mobilereg.test(mobile)) {
        $("#addressmobile-error").css('display', "inline");
        return false;
    } else {
        $("#addressmobile-error").css('display', "none");
        return true;
    }
}
function validator_addressdetail() {
    var addressdetail = $.trim($("#AddressDetail").val());
    if (addressdetail == '') {
        $("#addressdetail-error").css('display', "inline");
        return false;
    } else {
        $("#addressdetail-error").css('display', "none");
        return true;
    }
}
function validator_contactname() {
    var contactname = $.trim($("#ContactName").val());
    if (contactname == '') {
        $("#addressname-error").css('display', "inline");
        return false;
    } else {
        $("#addressname-error").css('display', "none");
        return true;
    }
}
function validator_cty() {
    var dist = $.trim($(".dist").val());
    if (!dist) {
        $("#address-error").css('display', "inline");
        return false;
    } else {
        $("#address-error").css('display', "none");
        return true;
    }
}

//设置地址默认值
function setAddrIsDefault(addrId) {
    var d = {
        addressId: addrId
    }
    $.getJSON("/Customer/basejson/Address/SetDefaultAddress", d, function (jn) {
        if (jn.rlt) {
            $.alertMsg(jn.msg);
            showCustAddrList();
        }

    })

};
//删除地址
function delectAddr(addrId) {
    $.confirm("询问", "是否删除该地址?", function () {
        $.loadJSON("/Customer/basejson/Address/DeleteAddress", { addressId: addrId }, function (jn) {
            if (jn.rlt) {
                $.alertMsg("删除成功");
                showCustAddrList();
                clearInputValue();
            } else $.alertError(jn.msg);
        });
    })


};
//修改地址
function editAddr(aId) {
    d = { addrId: aId }
    $.getJSON("/Customer/basejson/Address/QueryAddrSingleRecord", d, function (jn) {
        if (jn.rlt) {
            $('#cty #AreaId').val(jn.item.areaId);
            $.set3LevelAddress({
                $province: $('#cty .prov'),
                $city: $('#cty .city'),
                $county: $('#cty .dist'),
                $val: $('#cty #AreaId')
            });

            $("#AddressId").val(jn.item.addressId);
            $("#AreaId").val(jn.item.areaId);
            $("#AddressDetail").val(jn.item.addressDetail);
            $("#ContactName").val(jn.item.contactName);
            $("#Mobile").val(jn.item.mobile);
            $('#IsDefault').iCheck(jn.item.isDefault?"check":"uncheck");
            $('#Remark').val(jn.item.remark);
            console.log(jn.item.isDefault);
            uiRefresh();

        } else $.alertError(jn.msg);
    });
};

function clearInputValue() {
    $("#_cmbProvince").val("");
    $("#_cmbCity").val("");
    $("#_cmbArea").val("");
    $("#AddressId").val("");
    $("#AreaId").val("");
    $("#AddressDetail").val("");
    $("#ContactName").val("");
    $("#Mobile").val("");
    $("#IsDefault").prop("checked", false);

    $("#address-error").css('display', "none");
    $("#addressdetail-error").css('display', "none");
    $("#addressname-error").css('display', "none");
    $("#addressmobile-error").css('display', "none");
}



