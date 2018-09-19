

//载入详细
function LoadMedicalHistoryDetail(treatId) {
    $('#Content').empty().load("/MedicalLib/MyMedicalRecordDetails", { treatId: treatId });
}