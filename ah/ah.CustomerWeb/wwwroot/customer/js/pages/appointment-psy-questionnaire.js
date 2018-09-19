$(function () {
    $('#savaPsychPretreatForm').ajaxForm({
        beforeSubmit: PSY.submitPsyBeforeSubmit,
        url: "/Customer/Appointment/PsyQuestionnaire",
        type:"POST",
        error: function (request) { layer.msg("保存失败，请重试！")},
        success: PSY.submitPsySuccess
    }).submit(function () { return false; }); 
});
var PSY = {
    //关闭窗口
    closePsy: function () {
        var index = parent.layer.getFrameIndex(window.name); //获取窗口索引
        parent.layer.close(index);
    },
    //登记表校验
    submitPsyBeforeSubmit: function () {
        console.log("校验");
    },
    //提交成功
    submitPsySuccess: function (jn) {
        console.log(jn);
        if (jn.rlt) {
            parent.APPOINTMENT.PsyQuestionnaire_CallBack(jn.PropName, jn.PropValue);
            PSY.closePsy();
        }
        
    },
}