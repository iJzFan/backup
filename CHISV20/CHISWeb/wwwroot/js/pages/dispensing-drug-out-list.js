var OUTDRUG = {
    initDispensingDrugOutList: function () {
        alert("刷新数据，开发中");
    }
}

function searchDrug(drugId) {
    var $c = $('[ah-id="searchText"]');
    $c.val(drugId);
    $c.parent().find("button").click();
}