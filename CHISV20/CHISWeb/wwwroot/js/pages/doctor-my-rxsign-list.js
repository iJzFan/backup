var MyTreat={
    GetTreatList: function (pageIndex) {
        var dt0 = $('input[ah-id="startDate"]').val();
        var dt1 = $('input[ah-id="endDate"]').val();
        var searchText = $('input[ah-id="searchText"]').val(); 
        $('#tableContent').load("/doctor/LoadMyRxSignList", { searchText: searchText, dt0: dt0, dt1: dt1, pageIndex: pageIndex});

    }
}