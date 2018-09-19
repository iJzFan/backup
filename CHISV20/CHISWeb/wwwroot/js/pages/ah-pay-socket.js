//播放待扫描提示音
function PlayOrderAudio() {
    var order = $("audio[ah-id='ah-order-tips']")[0];
    order.volume = 1;
    order.play();
}
//播放失败提示音
function PlayErrorAudio() {
    var error = $("audio[ah-id='ah-pay-error']")[0];
    error.volume = 1;
    error.play();
}
//播放支付提示音
function PlaySuccessAudio() {
    var success = $("audio[ah-id='ah-pay-success']")[0];
    success.volume = 1;
    success.play();
}
//初始化音频，启动socket
function initAudioAndSocket() {
    var order = $("audio[ah-id='ah-order-tips']")[0];
    var error = $("audio[ah-id='ah-pay-error']")[0];
    order.volume = 0;
    order.play();
    error.volume = 0;
    error.play();
    var success = $("audio[ah-id='ah-pay-success']")[0];
    success.volume = 0;
    success.play();
    $(".ah-pay-tips-wrap").addClass("ah-hide");

    //启动链接socket
    doStart();
}
//初始化连接
function doConnect(open, accept, close, error) {
    //创建websocket,并定义事件回调
    if (socket != null) { socket.close(); socket == null; }
    try {
        socket = new WebSocket(uri);
    }
    catch (e) { alert(e);}
    socket.onopen = function (e) { open(); };
    socket.onclose = function (e) { close(); };
    socket.onmessage = function (e) { accept(e.data); };
    socket.onerror = function (e) { error(e.data); };
    console.log(socket);
 }
//链接socket
function doStart() { doConnect(open, accept, close, error); }
//发送信息
function doSend(message) { socket.send(message); }
//关闭socket
function doClose() { socket.close(); socket = null; }


//测试用，结果输出到控制台
function msg(msg) {
    console.log(msg);
}
//打开连接回调
function open() {
    msg("连接打开");
    $(".ah-barcode-main").show();//显示二维码区域
    $(".ah-barcode-other").show();//显示支付类型提示
    pagedata.status.css("top", "initial");//缩小连接状态提示
    pagedata.status.find("div").first().html("连接打开!").attr("class","ah-socket-status2");
}
//关闭连接回调
function close() {
    msg("连接关闭");
    $(".ah-barcode-main").hide();//隐藏二维码区域
    $(".ah-barcode-other").hide();//隐藏支付类型提示
    pagedata.status.css("top", "148px");//放大连接状态提示
    var state = pagedata.status.find("div").first();
    state.attr("class", "ah-socket-status1").html("连接断开,5秒后自动重连!");
    var n = 5;
    clearTimeout(pagedata.handle);//删除定时器
    PlayErrorAudio();
    pagedata.handle = setInterval(function () {
        state.html("连接断开," + (n--)+"秒后自动重连!");
        if (n == -1) {
            doStart(); //刷新
            clearTimeout(pagedata.handle);//删除定时器
        }
    }, 1000);
}
//接收数据回调
function accept(result) {
    msg("从服务器获取数据:" + result);
    var jn = eval("(" + result + ")");
    console.log(jn);
    if (jn.rlt && jn.payOrderId) {
        var url = qruri + "&payOrderId=" + jn.payOrderId;
        doPayConnect(url);
    }

    //封装到此调用函数内
    function doPayConnect(url, open, accept, close, error) {
        //创建websocket,并定义事件回调
        if (qrSocket != null) { qrSocket.close(); qrSocket == null; }
        qrSocket = new WebSocket(url);
        qrSocket.onopen = function (e) { qropen(); };
        qrSocket.onclose = function (e) { qrclose(); };
        qrSocket.onmessage = function (e) { qraccept(e.data); };
        qrSocket.onerror = function (e) { qrerror(e.data); };
    }

    function qropen() { msg("【二维码监控】启动"); }
    function qrclose() {
        msg("【二维码监控】关闭");
        pagedata.status.find("div").first().hide();//隐藏连接状态
        $(".ah-pay-socket-tips-error").append("<div id='error-tips-time'>5秒后重新载入!</div>");
        var tips = $("#error-tips-time");
        var n = 5;
        clearTimeout(pagedata.handle);//删除定时器
        pagedata.handle = setInterval(function () {
            tips.html((n--) + "秒后重新载入!");
            if (n == -1) {
                setPayType_TIMEOUT();
                tips.remove();
                clearTimeout(pagedata.handle);//删除定时器
                return;
            }
        }, 1000);
    }
    function qraccept(result) {
        msg("【二维码监控】获取数据：" + result);
        var jn = eval("(" + result + ")");
        console.log(jn);
        switch (jn.status) {
            case "GETPAYINFO"://获取到二维码相关数据
                setPayType_GETPAYINFO(jn);
                PlayOrderAudio();
                break;
            case "PAYEDSUCCESS"://已经支付成功了
                setPayType_PAYEDSUCCESS(jn);
                PlaySuccessAudio();
                break;
            case "THISPAYOK"://当次支付成功
                setPayType_THISPAYOK(jn);
                PlaySuccessAudio();
                break;
            case "ERROR"://出现失败
                setPayType_ERROR(jn);
                PlayErrorAudio();
                break;
            case "TIMEOUT"://超时
                setPayType_TIMEOUT(jn);
                PlayErrorAudio();
                break;
            default://未知状态
                break;
        }
    }
    function qrerror(err) { msg("【二维码监控】Error:" + err); }


}
//错误回调
function error(result) { msg("<div style='color:red'>错误：" + result + "</div>"); }
function send() {
    var msg = document.getElementById("sendtxt").value;
    doSend(msg); msg("发送数据:" + msg);
}
//设置收费基本信息
function setPayInfo(cname, amout,url) {
    pagedata.cname.html(cname);//设置用户名字
    pagedata.amout.html("￥" + parseFloat(amout)/100);//设置收费金额
    var w = $(window).width();
    var opt = {
        text: url,
        width: 300,
        height: 300,
    }
    if (w < 800) {
        opt.height = opt.width = w * 0.6;
    }
    pagedata.barcode.html("");//清除二维码原内容
    pagedata.barcode.qrcode(opt);//生成二维码
    pagedata.status.find("div").first().hide();//隐藏连接状态
    pagedata.tips.hide();//隐藏提示
    pagedata.codetype.show();//显示支付类型
    clearTimeout(pagedata.handle);//删除定时器
}


//设置为等待数据中（操作超时）
function setPayType_TIMEOUT(jn) {
    pagedata.cname.html("...");
    pagedata.amout.html("...");
    var img = '<img src="/images/loading-socket.gif"/>';
    if (jn) {
        img += "<div class='ah-pay-socket-tips'>" + jn.msg + "</div>";
    } else {
        img += "<div class='ah-pay-socket-tips'>等待数据中!</div>";
    }
    pagedata.status.find("div").first().show();//显示连接状态
    pagedata.barcode.html(img);//显示等待中的图标
    pagedata.tips.show();//显示提示
    pagedata.codetype.hide();//隐藏支付类型
    clearTimeout(pagedata.handle);//删除定时器
}
//设置为待扫描。
function setPayType_GETPAYINFO(jn) {
    setPayInfo(jn.customerName, jn.totalAmount, jn.union2DCodeUrl);
}

//提示这是支付过的订单。
function setPayType_PAYEDSUCCESS(jn) {
    var img = '<img src="/images/ah-order-tips.png"/>';
    img += "<div class='ah-pay-socket-tips'>" + jn.msg + "</div>";
    pagedata.barcode.html(img);
    pagedata.tips.show();//显示提示
    pagedata.codetype.hide();//隐藏支付类型
}
//提示当次支付成功
function setPayType_THISPAYOK(jn) {
    var img = '<img src="/images/ah-order-tips.png"/>';
    img += "<div class='ah-pay-socket-tips'>" + jn.msg + "</div>";
    pagedata.barcode.html(img);
    pagedata.tips.show();//显示提示
    pagedata.codetype.hide();//隐藏支付类型
}
//提示失败
function setPayType_ERROR(jn) {
    var img = '<img src="/images/ah-pay-error.png"/>';
    img += "<div class='ah-pay-socket-tips-error'>" + jn.msg + "</div>";
    pagedata.barcode.html(img);
    pagedata.tips.show();//显示提示
    pagedata.codetype.hide();//隐藏支付类型
}

//选中支付方式
function activePayType(_this,val) {
    pagedata.codetype.find("img").each(function () {
        $(this).removeClass("ah-active-paytype");
    });
    $(_this).addClass("ah-active-paytype");
    //赋值支付类型 可以根据后台修改activePayType所传过来的值  暂时为 wechat 和 alipay
    $("#pay_type").val(val);
}