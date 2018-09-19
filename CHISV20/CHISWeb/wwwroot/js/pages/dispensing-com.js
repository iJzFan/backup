//获取物流信息
function getLogistics(_this,supplierId, supplierOrderNo) {   
    $.get("/openapi/Utils/HtmGetLogisticsStatus", { supplierId: supplierId, supplierOrderNo: supplierOrderNo}, function (html) {       
        layer.tips(html, _this, {
            tips: [1, '#3595CC'],
            time: 10000
        });
    });
}