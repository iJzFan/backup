/*

用于自定义验证的前端验证代码

*/

$(function ($) {

    $.validator.addMethod("prcid",
        function (value, element, parameters) {
            if (value) return /^(\d{15}|\d{18}|\d{17}[\d|X|x]{1})$/g.test(value);
            else return true;
        });
 
    $.validator.addMethod("mobile",
        function (value, element, parameters) {
            if (value) return /^1[356789]\d{9}$/g.test(value);
            else return true;
        });


    $.validator.unobtrusive.adapters.addBool("prcid");
    $.validator.unobtrusive.adapters.addBool("mobile");

}(jQuery));
