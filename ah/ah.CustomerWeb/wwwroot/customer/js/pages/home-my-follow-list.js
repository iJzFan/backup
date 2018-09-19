var FOLLOW = {
    InitFollowLists: function () {
        $.get("/Customer/Follow", {}, function (html) {
            $("#MYFOOLOWTAB").html(html);
        })
    },
    CustomerFollow: function (stationId, doctorId, isFollow,_this) {
        _this = $(_this);
        $.isFollow(stationId, doctorId, isFollow, function () {
            $(_this).parents(".follow-doctor").remove();
        });
    }
}