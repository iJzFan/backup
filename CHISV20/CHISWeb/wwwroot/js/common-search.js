/*
 *    有关搜索的一般性方法
 *    作者 Rex
 */

$(function () {
    //单项选择
    $('.selector input[type=radio]').on("click", function () {
        var $c = $(this).parents('.sl-wrap').find('.sl-key').first();
        var titleText = $c.text();
        var titleKey = $c.attr("title-key");
        var value = $(this).val();
        var valName = $(this).next().text();
        if(value) SEARCH.addSearchItem(titleText, titleKey, valName, value, this);
    });
    //输入框输入
    $('.ah-more-search-btn').on('click', function () {
        var $p = $(this).parents(".ah-search-input");
        var val = ""; valName = "";
        var $cs = $p.find('input[type=text]');
        //如果是单输入内容，则直接展示内容，如果是多输入内容，则拼接内容 key0=val0;key1=val1;
        if ($cs.length == 1) {
            val = $cs.first().val();
            var nt = $cs.first().attr("name-title");
            valName += (nt ? nt + "=" : "") + val;
        } else {
            $cs.each(function (i, m) {
                val += $(m).attr("name") + "=" + $(m).val() + ";"
                var nt = $(m).attr("name-title");
                valName += (nt ? nt + "=" : "") + $(m).val() + ";";
            });
        }

        $p.parents(".sl-wrap").find("input[type=radio]").prop("checked", false);//取消输入
        var $c = $(this).parents('.sl-wrap').find('.sl-key').first();
        var titleText = $c.text();
        var titleKey = $c.attr("title-key");
        if(val) SEARCH.addSearchItem(titleText, titleKey, valName, val, this);
    });

    $('.sel-items').on("click", ".crumb-select-item>i", function (event) {
        SEARCH.removeSelected(this, event);
    });
});



var SEARCH = {
    searchUrl: "",
    addSearchItem: function (titleText, titleKey, valName, value, _this) {
        // 数据存放在 title-key,value属性里,比如 title-key=SearchRange value=ThisDay
        var h = '<a class="crumb-select-item" href="" rel="nofollow" title="' + titleText + '" title-key="' + titleKey + '" value="' + value + '" >';
        h += '<b>' + titleText + '</b>';
        h += '<em>' + valName + '</em>';
        h += '<i></i></a>'; 
        var $cc = $('#search_items').find("a[title-key='" + titleKey + "']");
        if ($cc.length > 0) {
            //更新
            $cc.before(h).remove();
        } else {//添加
            $('#search_items>.sel-items').append(h);
        }
        $(_this).parents(".J_selectorLine").css("display", "none");//隐藏搜索栏目
        SEARCH.search(1);
    },
    addSearchText: function () {
        var searchText = $('input[ah-id=searchText]').val();
        var b = searchText || searchText == "0";
        var $c = $('.crumbs-nav-item .search-key');
        if ($c.length > 0) {
            if (b) $c.text(searchText);
            else {
                $('#search_items').find(".crumbs-arrow").remove();
                $('#search_items').find(".crumbs-nav-item").remove();
            }
        }
        else {
            if (b) {
                var h = '<i class="crumbs-arrow">&gt;</i><div class="crumbs-nav-item"><strong class="search-key" title-key="SearchText">' + searchText + '</strong></div>';
                $('#search_items').append(h);
            }
        }
        SEARCH.search(1);
    },
    removeSelected: function (_this, event) {
        $(_this).parents(".crumb-select-item").remove();
        //阻止冒泡
        event.stopPropagation();
        event.preventDefault();
        //显示对应的高级选项，并设置高级选择的内容为空
        var titleKey = $(_this).parents("a").first().attr("title-key");
         
        var $p = $('.sl-key[title-key=' + titleKey + ']').parents(".J_selectorLine");
         
        $p.css("display", "");//去掉隐藏属性
        $p.find("input[type=text]").val('');
        $p.find("input[type=radio]").prop("checked", false);
        //提交搜索
        SEARCH.search(1);
        return false;
    },
    //获取出库清单
    search: function (pageIndex) {
        //收集搜索条件form的信息
        var postData = { pageIndex: pageIndex };
        var $d = $('#search_items .crumb-select-item');
        $d.each(function (i, m) {
            postData[$(m).attr("title-key")] = $(m).attr("value");
        });
        var searchText = $('#search_items .search-key').text();
        postData.searchText = searchText;
        
        $.post(SEARCH.searchUrl, postData, function (html) {
            $('div[ah-id=OUT-TABLE]').html(html);
        });
    }
}
