var RECANT = {
    InitFollowRecantLists: function () {
        $.get("/Customer/follow/recent", {}, function (html) {
            $("#MYFOOLOWTAB").html(html);
        })
    },
    CustomerFollow: function (stationId, doctorId, isFollow,_this) {
        _this = $(_this);
        $.isFollow(stationId, doctorId, isFollow, function () {
            $(_this).parents(".follow-doctor").remove();
        });
    },
    ContinueAppointment: function (id,name,type) {
        if (type == "doctor") {
            if ($(document).width() > 800) {
                $.err("请在微信公众号预约!");
            } else {
                window.location.href = "/Customer/Appointment/SelectDoctorInfo?doctorId=" + id;
            }
        } else if (type == "station") {
            window.location.href = "/Customer/Appointment/IndexStep2?stationName=" + name + "&stationId=" + id;
        }
    },
}