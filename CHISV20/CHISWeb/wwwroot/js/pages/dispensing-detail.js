 

$(function () {
    $(".ah-drugSumary-tab").on("click", "li", function () {
        var _this = $(this);
        var c = "ah-drugList-border-" + _this.attr("ah-type");
        if (_this.attr("ah-type") == "all") {
            _this.parents(".ah-dispensing-left").first().find("li").each(function () {
                if (!$(this).hasClass("ah-drugList-title") && !$(this).parent().hasClass("ah-drugSumary-tab")) {
                    $(this).css("display", "flex");
                }
            });
        } else {
            _this.parents(".ah-dispensing-left").first().find("li").each(function () {
                if (!$(this).hasClass("ah-drugList-title") && !$(this).hasClass(c) && !$(this).parent().hasClass("ah-drugSumary-tab")) {
                    $(this).hide();
                } else {
                    $(this).css("display", "flex");
                }
            });
        }
        //滚动条初始化
        $(".scrollbar-dynamic").each(function () {
            $(this).scrollbar();
        });
    })
})

function refreshDispensingSummary() {
    $.post("/Dispensing/LoadDispensingDetailSumary", { treatId: pagedata.treatId }, function (html) {
        $('#dispensing_summary').html(html);
    })

}



