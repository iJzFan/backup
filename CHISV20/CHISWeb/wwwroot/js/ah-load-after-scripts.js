$(function () {
    //载入最后的复写样式采用超时载入主要是layer自身也会在网页最后载入，避免载入在它的前面
    setTimeout(function () {
      //  $("head").append('<link rel="stylesheet" href="/css/common-overide-last.css" />');
        $("head").append('<link rel="stylesheet" href="/css/ah-overide-last.css" />');
    }, 1000);
});