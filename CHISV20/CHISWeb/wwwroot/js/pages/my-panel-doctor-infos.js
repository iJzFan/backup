//头像修改
$('#EditDoctorPic').click(function () {
    var handle = $.open({
        type: 2,
        title: "上传图片",
        content: '/tools/UploadPic?posType=doctor&sourceId=doctor&fileName=' + $('#Customer_CustomerPic_info').val()
    });
});
//身份证上传
$('#EditCertAPic').click(function () {
    var handle = $.open({
        type: 2,
        title: "上传正面身份证",
        content: '/tools/UploadPic?picType=cert&size=856x540&posType=cert&sourceId=certA&fileName=' + $('#Customer_CustomerPic_CertA').val()
    });
});
$('#EditCertBPic').click(function () {
    var handle = $.open({
        type: 2,
        title: "上传背面身份证",
        content: '/tools/UploadPic?picType=cert&size=856x540&posType=cert&sourceId=certB&fileName=' + $('#Customer_CustomerPic_CertB').val()
    });
});
//上传图片回调
function setPicName(fileName, sourceId) {
    if (sourceId == "doctor") {
        $('#cusImg').attr("src", pagedata.docRoot + fileName);
        $('#DoctorPhotoUrl').val(fileName);
    } else if (sourceId == "certA") {
        $('#certA').attr("src", pagedata.certRoot + fileName);
        $('#IDCardAImg').val(fileName);
    } else if (sourceId == "certB") {
        $('#certB').attr("src", pagedata.certRoot + fileName);
        $('#IDCardBImg').val(fileName);
    }
}