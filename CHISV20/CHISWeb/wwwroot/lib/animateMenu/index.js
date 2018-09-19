$(document).ready(function (ev) {
    var toggle = $('#ss_toggle');
    var menu = $('#ss_menu');
    var rot;
    $('#ss_toggle').on('click', function (ev) {
        rot = parseInt($(this).data('rot')) - 180;
        menu.css('transform', 'rotate(' + rot + 'deg)');
        menu.css('webkitTransform', 'rotate(' + rot + 'deg)');
        if (rot / 180 % 2 == 0) {
            toggle.parent().addClass('ss_active');
            toggle.addClass('close');
            menuShow(menu,true);
        } else {
            toggle.parent().removeClass('ss_active');
            toggle.removeClass('close');
            menuShow(menu,false);
        }
        $(this).data('rot', rot);
    });
    $("div[name='div_menu']").on('click', function (ev) {
        toggle.click();
    });
    menu.on('transitionend webkitTransitionEnd oTransitionEnd', function () {
        if (rot / 180 % 2 == 0) {
            $('#ss_menu div i').addClass('ss_animate');
        } else {
            $('#ss_menu div i').removeClass('ss_animate');
        }
    });
});
function menuShow(obj,state){
	$(obj).find('div[name="div_menu"]').each(function(){
		var _this = $(this);
		if(state){
			_this.show("slow").css("display","table");
		}else{
			_this.hide("slow");
		}
	})
}