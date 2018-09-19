var GIFT = {
    add: function () {
        $.open({
            type: 2,
            title: "新增商品",
            fix: false,
            content: "/Gift/Create",
            btn: ["确定", "取消"],
            yes: function (index, layero) {
                //得到iframe页的窗口对象
                var win = top.window[layero.find('iframe')[0]['name']];
                win.createSub();
            }
        });
    },
    edit: function (id) {
        $.open({
            type: 2,
            title: "修改商品",
            fix: false,
            content: "/Gift/Edit/"+id,
            btn: ["确定", "取消"],
            yes: function (index, layero) {
                //得到iframe页的窗口对象
                var win = top.window[layero.find('iframe')[0]['name']];
                win.editSub(index);
            }
        });
    },
}
 

