var GIFTORDER = {
    Ship: function (id) {
        $.open({
            type: 2,
            title: "发货",
            fix: false,
            area: ["700px","550px"],
            content: "/Gift/ShipView/"+id,
            btn: ["发货", "取消"],
            yes: function (index, layero) {
                //得到iframe页的窗口对象
                var win = top.window[layero.find('iframe')[0]['name']];
                win.ShipSub();
            }
        });
    },
}
 

