/*
 Site	: http://www.tsyhis.com
 Author	: Feng Jesses
 Company : angel Health IT (www.tsjk365.com)
 */
jQuery(document).ready(function() {

    $('.page-container form').submit(function(){
        var username = $(this).find('.username').val();
        var password = $(this).find('.password').val();
        if(username == '') {
            $(this).find('.error').fadeOut('fast', function(){
                $(this).css('top', '17%');
            });
            $(this).find('.error').fadeIn('fast', function(){
                $(this).parent().find('.username').focus();
            });
            return false;
        }
        if(password == '') {
            $(this).find('.error').fadeOut('fast', function(){
                $(this).css('top', '43%');
            });
            $(this).find('.error').fadeIn('fast', function(){
                $(this).parent().find('.password').focus();
            });
            return false;
        }
    });

    $('.page-container form .username, .page-container form .password').keyup(function(){
        $(this).parent().find('.error').fadeOut('fast');
    });




});
$(window).resize(function(){
    var b="客服电话：0769-22898386"
    var a="版权信息：东莞市天使健康信息科技有限公司 备案号：009号 联系电话：0769-22898386"
    if($(window).width()<600){
        var btext=$('.login-bottom');
        btext.text("");
        btext.append(b);
    }else{
        var btext=$('.login-bottom');
        btext.text("");
        btext.append(a);
    }
});
