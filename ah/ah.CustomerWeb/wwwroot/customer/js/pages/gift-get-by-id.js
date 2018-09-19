var GIFTINFO = {
    ExchangeGift: function (gid, type) {
        var opt = {
            giftId: gid,
            count: $("#number").val(),
            address:null,
        };
        if (type == 0) {
            //实物 需要获取地址
            var customerName = $("#customerName").val();
            var phoneNumber = $("#phoneNumber").val();
            var city = $("#city-picker").val();
            if (customerName.length <= 0) {
                $.err("请选择有收件人的地址！");
                return false;
            }
            if (!(/^1[3|4|5|8][0-9]\d{4,8}$/.test(phoneNumber))) {
                $.err("请选择有正确的手机号的地址！");
                return false;
            }
            if (city.length <= 0) {
                $.err("请选择有地区的地址！");
                return false;
            }
            var other = $("#city-other").val();
            if (other.length <= 0) {
                $.err("请选择有详细地址的地址！");
                return false;
            }
            opt.phoneNumber = phoneNumber;
            opt.customerName = customerName;
            opt.address = city + " " + other;
        }
        $.get("/Customer/Gift/ExchangeGift", opt, function (jn) {
            if (jn.rlt) {
                $.ok("兑换成功，3秒后自动返回到我的积分！");
                window.setTimeout(function () {
                    window.location.href = "/Customer/Gift";
                }, 3000);
            } else {
                $.err(jn.msg);
            }
        });
    },
    //获取用户默认地址
    GetCustomerDefaultAddress: function (customerId) {
        $.get(CONFIG.ApiRoot + "/openapi/HealthorInfo/GetCustomerDefaultAddress", { customerId: customerId }, function (jn) {
            if (jn.rlt) {
                var _this = $(".defaultAddress");
                if (jn.item != null) {
                    var wrap = '<div class="ah-flex" style="align-items:center;"><span id="name"></span ><span id="phone"></span></div><div id="address"></div>';
                    $(".defaultAddress-bj").html(wrap);
                    _this.find("#address").html(GIFTINFO.AddressReturnVal(jn.item.mergerName) + " " + GIFTINFO.AddressReturnVal(jn.item.fullAddress));
                    _this.find("#name").html(GIFTINFO.AddressReturnVal(jn.item.contactName));
                    _this.find("#phone").html(GIFTINFO.AddressReturnVal(jn.item.mobile));

                    //赋值
                    $("#customerName").val(GIFTINFO.AddressReturnVal(jn.item.contactName));
                    $("#phoneNumber").val(GIFTINFO.AddressReturnVal(jn.item.mobile));
                    $("#city-picker").val(GIFTINFO.AddressReturnVal(jn.item.mergerName));
                    $("#city-other").val(GIFTINFO.AddressReturnVal(jn.item.fullAddress));
                    //判断是否为默认
                    if (jn.item.isDefault) {
                        var html = "<span class='default-address'>默</span>"
                        _this.find("#phone").append(html);
                    }
                } else {
                    var wrap = '<div class="address-select">请选择地址</div>';
                    $(".defaultAddress-bj").html(wrap);
                }
                
            } else {
                $.err("获取用户默认地址失败！");
            }
        });
    },
    //地址点击监听
    SelectAddress: function (customerId) {
        //选择地址监听
        $(".ah-addressesList-wrap").on("click", ".popup-ul li", function () {
            var wrap = '<div class="ah-flex" style="align-items:center;"><span id="name"></span ><span id="phone"></span></div><div id="address"></div>';
            $(".defaultAddress-bj").html(wrap);
            var data = JSON.parse($(this).attr("ah-data"));
            var _this = $(".defaultAddress");
            _this.find("#address").html(GIFTINFO.AddressReturnVal(data.mergerName) + " " + GIFTINFO.AddressReturnVal(data.fullAddress));
            _this.find("#name").html(GIFTINFO.AddressReturnVal(data.contactName));
            _this.find("#phone").html(GIFTINFO.AddressReturnVal(data.mobile));

            //赋值
            $("#customerName").val(GIFTINFO.AddressReturnVal(data.contactName));
            $("#phoneNumber").val(GIFTINFO.AddressReturnVal(data.mobile));
            $("#city-picker").val(GIFTINFO.AddressReturnVal(data.mergerName));
            $("#city-other").val(GIFTINFO.AddressReturnVal(data.fullAddress));

            GIFTINFO.CloseAddressList();
        });
        //点击更多地址监听
        $(".defaultAddress").click(function () {
            GIFTINFO.UpdataAddressList(customerId);
        });
    },
    //更新地址列表
    UpdataAddressList: function (customerId) {
        $.get(CONFIG.ApiRoot + "/openapi/HealthorInfo/GetCustomerAddresses", { customerId: customerId }, function (jn) {
            if (jn.rlt) {
                $(".ah-giftPopup-wrap").hide();
                var list = $(".ah-addressesList-wrap");
                var html = "";
                var name = $("#customerName").val();
                var phone = $("#phoneNumber").val();
                var city = $("#city-picker").val();
                var full = $("#city-other").val();
                for (var i = 0; i < jn.items.length; i++) {
                    var _this = jn.items[i];
                    var json = JSON.stringify(_this);
                    if (_this.contactName == null) _this.contactName = "null";
                    if (_this.mobile == null) _this.mobile = "null";
                    if (_this.mergerName == null) _this.mergerName = "null";
                    if (_this.fullAddress == null) _this.fullAddress = "null";

                    if (_this.contactName == name && _this.mobile == phone && _this.mergerName == city && _this.fullAddress == full) {
                        html += "<li class='ah-active-adderss' ah-data='" + json + "'>";
                    } else {
                        html += "<li ah-data='" + json + "'>";
                    }
                    html += "<div class=\"ah-flex\" style=\"align-items: center;\">";
                    html += "<span class='name'>" + _this.contactName + "</span><span class='mobile'>" + _this.mobile + "</span>";
                    if (_this.isDefault) {
                        html += "<span class=\"default-address\">默</span>";
                    }
                    html += "</div>";
                    html += "<div class='ah-addressList-merger'>" + _this.mergerName + " " + _this.fullAddress + "</div></li>";
                }
                list.find(".popup-ul").html(html);
                list.show();
            }
        });
    },
    CloseAddressList:function() {
        $(".ah-giftPopup-wrap").show();
        $(".ah-addressesList-wrap").hide();
    },
    AddressReturnVal: function (val) {
        if (val != "null" && val != null && val != "") {
            return val;
        } else {
            return "null";
        }
    },
    AddAddress: function () {
        $.open({
            type: 2,
            title: "新建地址",
            closeBtn:0,
            content: '/Customer/Tools/Address'
        });
    }
}