var GIFT = {
    GetGiftList: function (opt) {
        $.get("/Customer/Gift/OrderList", {}, function (html) {
            $("#OrderList").html(html);
            GIFT.InitGiftListUse();
        })
    },
    InitGiftListUse: function () {
        //订单详情
        $(".ah-giftMain-title2").on("click", function (event) {
            event.stopPropagation();
            if ($(window).width() <= 800) {
                var id = $(this).attr("ah-id");
                GIFT.GetOrderInfo(id);
            }
        });
        //使用券
        $("#OrderList").on("click", ".ah-gift-active", function (event) {
            event.stopPropagation();
            var _this = $(this);
            var state = _this.attr("ah-state");
            var orderId = _this.attr("ah-id");
            var customerId = _this.attr("ah-customerId")
            //判断是否可操作
            switch (state) {
                case "未使用": {
                    var url = "/Customer/Gift/UseGift?giftOrderId=" + orderId + "&&customerId=" + customerId;
                    window.location.href = url
                    break;
                }
                default: {
                    GIFT.GetOrderInfo(orderId);
                }
            }
        });
    },
    GetOrderInfo: function (id) {
        var url = "/Customer/Gift/OrderInfo?giftOrderId=" + id;
        window.location.href = url;
    }
}