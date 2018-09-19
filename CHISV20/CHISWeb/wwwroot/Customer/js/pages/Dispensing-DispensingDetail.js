$(function () {
    getAddressInfo();//获取发货地址信息
    $('.select-area').SelectArea();//弹出区域选择
    $("#myTab a").click(function (e) {
        e.preventDefault();
        $(this).tab("show");
    });
    $('#add-address').click(function () {
        layer.open({
            type: 1,
            shade: [0.5, '#f4f4f4'],
            area: ['30%', '50%'],
            title: '添加地址',
            content: $('#registerForm'),
            btn: ['确定', '取消'],
            yes: function (index, layero) {
                $('#registerForm').submit();
                layer.close(index);
            },
            cancel: function () {
                //右上角关闭回调
            }
        })
    });



    //表单验证
    $("#registerForm").validate({
        ignore: "",//允许隐藏区域的验证
        submitHandler: function (form) {
            //form.submit(); //采用程序方式提交
            //如果并不需要提交切换 则在此处理接下来的操作
            var d = {
                custId: $('#custId').val(),
                AddressId: null,
                custName: $('#customername').val(),
                telePhone: $('#telephone').val(),
                address: $('#addDetailAddress').val(),
                areaId: $('#areaId').val(),
                isDefault: $('#edit_isdefault').is(':checked'),
            }
            $.loadJSON("/base/Json_ModifyOrAddCustAddress", d, function (jn) {
                if (jn.rlt == true) {
                    setAddAddressClear();
                    window.location.reload();

                } else {
                    $.alertError(jn.msg);
                }
            });

        }
    });
    //激活当前Tab
    activeTab();
});

function activeTab() {
    if (pagedata.haveLocal) { $('#myTab a#tabtag_local').tab("show"); return; }
    if (pagedata.haveWebNetJK) { $('#myTab a#tabtag_webnet').tab("show"); return; }
}
//刷新新增地址为空
function setAddAddressClear() {
    $('#customername').val('');
    $('#telephone').val('');
    $('#address').val('');
    $('#areaId').val('');
    $('#edit_isdefault').removeAttr("checked");
}

//确认发药按钮函数
function sendJKWebNet() {
    var phone = $("#addr-phone").text().replace(/(^\s*)|(\s*$)/g, "");
    if (!$.f.isPhoneNo(phone)) {
        $.alertError("电话号码错误！")
        return false;
    }
    var d = {
        custId: $('#custId').val(),
        orderId: $('#order-NO').text(),
        selectedAddressId: $('#head_address_data .addr[data-isselected=1]').find("input.ah-hd-addressId").val(), //选择的地址Id       
        totalamount: $('#total-amount').text()
    };
    $.loadJSON("/Customer/Dispensing/Json_SendOrderToJK", d, function (jn) {
        if (jn.rlt == true) {
            if (pagedata.haveLocal == true) {
            } else {
                $.alertMsg("发送成功！");
                window.location.href = "/Customer/Dispensing/Dispensing";
            };
        } else $.alertError(jn.msg);

    });

}

//获取地址信息
function getAddressInfo(cusId, selAddressId) {
    var cid = $('#custId').val();
    var d = {
        custId: cid,
        selectedAddressId: selAddressId
    };
    $.loadJSON("/base/Json_AddressByCustId", d, function (jn) {
        if (jn.rlt == true) {
            //var content = "本次发货地址为:" + jn.addrInfor.fullAddress + "," + jn.addrInfor.contactName + "收" + "," + jn.addrInfor.mobile + "," + jn.addrInfor.zipCode;
            //$("#custAddressDeatil").html(content.replace(/null/ig, ''));
            //模板与模板数据
            $("#sendBtn").attr("disabled", true); //初始设置不能发送

            var html = template('successInfoTemplate', jn);
            //插入生成的数据
            document.getElementById('head_address_data').innerHTML = html;

            //点击地址
            $("#head_address_data").on("click", ".addr", function () {
                $('.inner').css("background", "url(/Customer/images/addrBox0-237-106.png) no-repeat");
                $(".suggest-address").attr('data-isselected', '0');
                $(this).children('.inner').css("background", "url(/Customer/images/addrBox1-237-106.png) no-repeat");
                $(this).attr('data-isselected', '1');

                $('#custAddressDeatil').empty();
                $('#custAddressDeatil').text("本次发货地址为：" + $(this).find(".town").text().trim().replace(/[\s|\u0020]+/g, "") + " ," + $(this).find(".addr-contact-name").text() + " ," + $(this).find(".addr-phone").text());

                //获取该地址是否合法并设置
                var selectedId = parseInt($(this).find("input.ah-hd-addressId").val());
                $.loadJSON("/base/Json_AddressIsLegal", { addressId: selectedId }, function (jn) {
                    if (jn.isLegal) $("#sendBtn").removeAttr("disabled", true);
                    else $("#sendBtn").attr("disabled", true);
                });
            });
            //设置默认
            $('#head_address_data .addr[data-isdefault=1]').click();
        } else $.alertError(jn.msg)

    });
};
//设置默认地址
function setDefaultAddressInfor(addressId, custId) {
    var d = { addressId: addressId, custId: custId };
    $.getJSON("/base/Json_SetDefaultAddress", d, function (jn) {
        if (jn.rlt = true) {
            //var content = "本次发货地址为:" + jn.addrInfor.fullAddress + "," + jn.addrInfor.contactName + "收" + "," + jn.addrInfor.mobile + "," + jn.addrInfor.zipCode;
            //$("#custAddressDeatil").html(content.replace(/null/ig, ''));
            getAddressInfo();


        } else $.alertError(jn.msg);
    })

}
//删除地址
function DeleteAddressInfor(adderssId) {
    var d = { addressId: adderssId, custId: $('#custId').val() };
    $.confirm("询问", "是否删掉本条地址信息", function () {
        $.getJSON("/base/Json_DeleteAddressInfor", d, function (jn) {
            if (jn.rlt == true) {
                getAddressInfo();
            } else $.alertError(jn.msg);
        });
    })
}

//修改地址
function modify_address(addrssId) {
    $.getJSON("/base/Json_GetAddressInfor", { addressId: addrssId }, function (jn) {
        if (jn.rlt = true) {
            $('#customerid').val(jn.itme.customerId);
            $('#customername').val(jn.itme.contactName);
            $('#telephone').val(jn.itme.mobile);
            $('#areaId').val(jn.itme.areaId);
            $('#addr_select').SelectArea({ cmd: "setAreaId", areaId: jn.itme.areaId })
            $('#addDetailAddress').val(jn.itme.addressDetail);
            $('#edit_address_id').val(jn.itme.addressId);
            $('#edit_isdefault').attr("checked", true);
            layer.open({
                type: 1,
                shade: [0.5, '#f4f4f4'],
                area: ['40%', '60%'],
                title: '修改信息',
                content: $('#registerForm'),
                btn: ['确定', '取消'],
                yes: function (index,layero) {
                    var d = {
                        custId: $('#customerid').val(),
                        custName: $('#customername').val(),
                        telePhone: $('#telephone').val(),
                        address: $('#addDetailAddress').val(),
                        areaId: $('#areaId').val(),
                        addressId: $('#edit_address_id').val(),
                        isDefault: $('#edit_isdefault').is(":checked")
                    }    
                    $.loadJSON("/base/Json_ModifyOrAddCustAddress", d, function (jn) {

                        if (jn.rlt == true) {
                            setAddAddressClear();
                            //window.location.reload();
                            //layer.close(index);
        
                            //layer.close(index)
                            //layer.closeAll();
                            //self.opener.location.reload();
                            //parent.location.reload();
                            layer.close(index)
                            getAddressInfo();
                        } else {
                            $.alertError(jn.msg);

                        }
                    });

                },
                //end: function () {
                //    location.reload();
                //},              
                cancel: function () {

                }
            })
        }
    });


}