var ALLDOCTOR = {
    GetAllDoctors: function (pageIndex) {
        var searchText = $('input[ah-id="search-input"]').val();
        $.post("/Doctor/pv_GetAllDoctors", {
            searchText: searchText,
            pageIndex: pageIndex,
        }, function (html) {
            $(".AllDoctor-items").html(html); 
        });
    },
}