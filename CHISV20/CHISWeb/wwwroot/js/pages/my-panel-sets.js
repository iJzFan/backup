
function saveConfig(_this, type) {
    var seckey = _this.name;
    var secval = "";
    if (type === undefined) {
        console.log(_this.tagName);
        switch (_this.tagName) {
            case "SELECT": secval = $(_this).val(); break;
            case "INPUT": secval = getvalues(_this.name); break;
            case "RADIO": secval = getvalues(_this.name); break;
        }
    } else {
        if (type == "bool") {
            if (_this.tagName == "INPUT" && _this.type == "checkbox") {
                if (_this.checked) secval = "True";
                else secval = "False";
            }
        }
    }

    $.post("/MyPanel/SetMyConfig", { secKey: seckey, secVal: secval }, function (jn) {
        if (jn.rlt) $.ok("设置成功！");
        else $.error("设置失败：" + jn.msg);
    })

    function getvalues(n) {
        var rtn = [];
        $('input[name=' + n + ']:checked').each(function (i, v) {
            rtn.push($(v).val());
        });
        return rtn.join();
    }

}

function addDoctor() {
    $.open({
        type: 2,
        title: '查找医生',
        area: ['600px', '80%'],
        content: "/Search/SearchDoctors" //iframe的url              
    });
}
function delRxDoctor(doctorId) {
    $.post("/MyPanel/DelRxDoctor", { doctorId: doctorId }, function (jn) {
        if (jn.rlt) refreshRxDoctorList();
        else $.error(jn.msg);
    });
}
function setDefRxDoctor(doctorId) {
    $.post("/MyPanel/SetDefRxDoctor", { doctorId: doctorId }, function (jn) {
        if (jn.rlt) refreshRxDoctorList();
        else $.error(jn.msg);
    });
}

//选择添加这个医生为处方医生
function doctorSelect(doctorId) {
    console.log(doctorId);
    $.post("/MyPanel/AddRxDoctor", { doctorId: doctorId }, function (jn) {
        if (jn.rlt) refreshRxDoctorList();
        else $.error(jn.msg);
    });
}


function refreshRxDoctorList() {
    $.post("/MyPanel/GetMyRxDoctors", function (html) {
        $('#my_rxdoctors').html(html);
    });
}