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
        if (value) SEARCH.addSearchItem(titleText, titleKey, valName, value, this);
    });
    //输入框输入
    $('.ah-search-input button').on('click', function () {
        var $p = $(this).parents(".ah-search-input");
        var val = ""; valName = "";
        var $cs = $p.find('input[type=text]');
        //如果是单输入内容，则直接展示内容，如果是多输入内容，则拼接内容 key0=val0;key1=val1;
        if ($cs.length == 1) {
            val = $cs.first().val();
            var nt = $cs.first().attr("name-title");
            valName += (nt ? nt + "=" : "") + val;
            if ($cs.attr("ah-tree")) {
                val = $cs.attr("ah-tree");
                valName = $cs.val();
            }
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
        if (val) SEARCH.addSearchItem(titleText, titleKey, valName, val, this);
    });

    $('.sel-items').on("click", ".crumb-select-item > i", function (event) {

        SEARCH.removeSelected(this, event);
    });
    $(".J_extMore").on("click", function () {
        var _this = $(this);
        var w = _this.parents(".J_selectorLine").first();
        if (w.hasClass("select_open")) {
            w.removeClass("select_open");
            _this.html("更多<i></i>");
        } else {
            w.addClass("select_open");
            _this.html("收起<i></i>");
        }
    });
});



var SEARCH = {
    searchUrl: "",
    searchMain: "",
    searchPagerId:"pager",
    searchQuery: "",
    exportUrl:"",
    addSearchQuery: function (key, val) {
        if (SEARCH.searchQuery.indexOf(key + "=") >= 0) {
            SEARCH.searchQuery = SEARCH.searchQuery.replace(new RegExp(key + "=\\w+","gi"), key + "=" + val);
        } else {
            SEARCH.searchQuery += (SEARCH.searchQuery.length > 0 ? "&" : "") + key + "=" + val;
        }
    },
    addSearchItem: function (titleText, titleKey, valName, value, _this) {
        $(".search-conditions").css("display", "flex");
        // 数据存放在 title-key,value属性里,比如 title-key=SearchRange value=ThisDay
        var h = '<a class="crumb-select-item" href="javascript:void(0)" rel="nofollow" title="' + titleText + '" title-key="' + titleKey + '" value="' + value + '" >';
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
                $('.ah-main-breadcrumbs').find(".crumbs-arrow").remove();
                $('.ah-main-breadcrumbs').find(".crumbs-nav-item").remove();
            }
        }
        else {
            if (b) {
                var h = '<i class="crumbs-arrow">&gt;</i><div class="crumbs-nav-item"><strong class="search-key" title-key="SearchText">' + searchText + '</strong></div>';
                $('.ah-main-breadcrumbs').append(h);
            }
        }
        SEARCH.search(1);
    },
    removeSelected: function (_this, event) {
        var wrap = $(_this).parents(".sel-items");
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

        if (wrap.find("a").length == 0) {
            $(".search-conditions").hide();
        }
        //提交搜索
        SEARCH.search(1);
        return false;
    },
    //获取出库清单
    search: function (pageIndex, pms) {
        //收集搜索条件form的信息
        var postData = { pageIndex: pageIndex };
        var $d = $('#search_items .crumb-select-item');
        $d.each(function (i, m) {
            postData[$(m).attr("title-key")] = $(m).attr("value");
        });
        var searchText = $('.ah-main-breadcrumbs .search-key').text();
        postData.searchText = searchText;

        var searchUrl = SEARCH.searchUrl;
        if (!pms) pms = SEARCH.searchQuery; else pms = pms + "&" + SEARCH.searchQuery;
        if (pms) {
            if (pms.indexOf('?') == 0) pms = pms.substr(1);
            var cc = SEARCH.searchUrl.indexOf('?') > 0 ? "&" : "?";
            searchUrl = SEARCH.searchUrl + cc + pms ;
        }
        $.post(searchUrl, postData, function (html) {
            $('div[ah-id=' + SEARCH.searchMain + ']').html(html);
        });
    },
    clearTable: function () {
        $('div[ah-id=' + SEARCH.searchMain + ']').html("清空，重新点击【搜索】");
        $('#' + SEARCH.searchPagerId).empty();
    },
    loadRowDetail: function (e, url) {
        //获取行详细信息
        var $target = $(e.target);
        var $table = $target.parents("table").first();
        var $tr = $target.parents("tr").first();
        var $n = $tr.next('.tr-detail');
        $table.find(".tr-detail").not($n).hide();

        var colnum = $tr.find("td:visible").length;
        //适配平板上旋转时 列的合并数目
        $(window).resize(function () {
            colnum = $tr.find("td:visible").length;
            $tr.next().find("td").first().attr("colspan", colnum);
        });
        if (url) {
            var $td = null;
            if ($n.length == 1) {
                $td = $n.find("td").first(); $td.attr("colspan", colnum); $n.toggle();
                if ($n.is(":visible")) $target.addClass("ah-colspan");
                else $target.removeClass("ah-colspan");
            }
            else {
                $trnew = $("<tr class='tr-detail'><td colspan='" + colnum + "'><span>载入中...</span></td></tr>");
                $tr.after($trnew);
                $td = $trnew.find("td");

                setTimeout(function () {
                    $.post(url, null, function (html) {
                        $td.html(html);
                        $target.addClass("ah-colspan");
                    });
                }, 300);
            }

        }
    }
    , getAllQuery: function () {
        //收集搜索条件form的信息
        var postData = {};
        var $d = $('#search_items .crumb-select-item');
        $d.each(function (i, m) {
            postData[$(m).attr("title-key")] = $(m).attr("value");
        });
        var searchText = $('.ah-main-breadcrumbs .search-key').text();
        postData.searchText = searchText;

        var q=SEARCH.searchQuery;   
        for (var key in postData) {
            q += "&" + key + "=" + postData[key];
        }
        q=q.replace(/^\&+/g,'');
        return q;
    }
    , exportExcel: function () {
        //导出数据
        var url = SEARCH.exportUrl + "?" + SEARCH.getAllQuery();                
        window.open(url);
    }

}


/*通用的弹窗增删改查
titleName:
isUrlOne:true/false
urlOne
urlAdd/urlModify/urlDelete/urlView
*/
function Crud(opt) {
    return {
        Add: function (callback) {
            var handle = $.open({
                type: opt.type | 2, area: opt.area,
                title: opt.title || '新增' + opt.titleName,
                content: opt.urlAdd || (opt.urlOne + "?op=NEWF"),
                btn: opt.btn || ['确定', '取消'],
                yes: function (index, layero) {
                    var fn = callback || opt.addClick;
                    if (fn) fn(index, layero, top.window[layero.find('iframe')[0]['name']]);
                }
            });
        },
        Modify: function (id, callback) {
            var handle = $.open({
                type: opt.type | 2, area: opt.area,
                title: opt.title || '修改' + opt.titleName,
                content: opt.urlModify || (opt.urlOne + "?op=MODIFYF&recid=" + id),
                btn: opt.btn || ['确定', '取消'],
                yes: function (index, layero) {
                    var fn = callback || opt.modifyClick;
                    if (fn) fn(index, layero, top.window[layero.find('iframe')[0]['name']]);
                }
            });
        },
        Delete: function (id, callback, event) {
            $.confirm("确认删除", "是否真的删除？", function () {
                $.post(opt.urlDelete || (opt.urlOne + "?op=DELETE&recid=" + id), {}, function (jn) {
                    var fn = callback || opt.deleteClick;
                    if (fn) fn(jn, event);
                });
            });
        },
        View: function (id) {
            var handle = $.open({
                type: opt.type | 2, area: opt.area,
                title: opt.title || '查看' + opt.titleName + '详情',
                content: opt.content || (opt.urlOne + "?op=VIEW&recid=" + id),
                btn: opt.btn || ['取消'],
                yes: function () { top.layer.close(handle); }
            });
        },
        ver: "1.0"
    };
}

