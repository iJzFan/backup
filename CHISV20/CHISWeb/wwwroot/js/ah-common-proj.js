//显示签名板
function showSign() {
    $.open({
        area:['100%','100%'],
        type: 2,
        title: "签名板",
        content: "/Tools/Sign"
    });
}