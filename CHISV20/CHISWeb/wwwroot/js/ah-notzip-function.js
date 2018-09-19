//不能被压缩的js函数
function lumbdaFind(ms, id, findId) {
    return ms.find(m => m[id] == findId);
}
//Object.prototype.lumbdaFind = function (id, findId) {
//    var _this = this;
//    console.log(_this);
//    return _this.find(m => m[id] == findId);
//}