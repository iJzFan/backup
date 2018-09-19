$(function () {
    //首先设置只读
    if (typeof pagedata != "undefined" && pagedata.isView) {
        $('input[type=text],input[type=password],input[type=number],input[type=date],input[type=checkbox],input[type=email],textarea').prop("readonly", true);
        $('.status3 select,select').attr("readonly", "readonly");
        $('.input-group.input-display').attr("readonly", "");//针对input-group不能很好处理readonly的显示情况
    }
    //滚动条初始化
    $(".scrollbar-dynamic").each(function () {
        $(this).scrollbar();
    });
    //时间插件不弹出输入法
    $(".ah-date").focus(function () {
        //console.log($(this).hasClass("ah-data-can-input"));
        if ($(this).hasClass("ah-data-can-input")) { }
        else document.activeElement.blur();
    })

    //form的设计
    $.extend($.validator.defaults, { ignore: "" });//隐藏元素也需要验证
    /*---------------------- 异步发送form表单 -----------------------------------*/
    //ah-role-method="ajax" ah-success-callback="" ah-fail-callback 通过callback来返回
    $(document).on("submit", "[ah-role-method]", function (event) {
        var that = this;
        try {
            $(this).ajaxSubmit({
                //target: '#output',          //把服务器返回的内容放入id为output的元素中      
                beforeSubmit: function (formData, jqForm, options) {
                    //提交前的回调函数 
                    //formData: 数组对象，提交表单时，Form插件会以Ajax方式自动提交这些数据，格式如：[{name:user,value:val },{name:pwd,value:pwd}]  
                    //jqForm:   jQuery对象，封装了表单的元素     
                    //options:  options对象  
                    var queryString = $.param(formData);   //name=1&address=2  
                    var formElement = jqForm[0];              //将jqForm转换为DOM对象  
                    //  var address = formElement.address.value;  //访问jqForm的DOM元素  
                    //对多选一必填项目的前端验证
                    var $c =$(that).find('[ah-role-validate="require_one"]');
                    if ($c.length > 0) {
                        var bhave = false;
                        $c.find('input[type=text]').each(function (i, m) {
                            bhave = bhave || ($(m).val().length > 0);
                        });
                        if (!bhave) {
                            if ($c.find('[ah-val=ONE_RQUIRED]').size() == 0)
                                $c.append("<span ah-val='ONE_RQUIRED' class='field-validation-error'>必须至少填写一个数据</span>")
                            return false;
                        }
                    }
                  //  jqForm.validate();//验证是否通过
                    return jqForm.valid();
                    // return true;  //只要不返回false，表单都会提交,在这里可以对表单元素进行验证  
                },
                success: function (responsejn, statusText) {
                    var successCallback = $(that).attr("ah-success-callback");
                    if (successCallback) {
                        //运行特定的成功回调函数
                        var fc = eval(successCallback); new fc(responsejn, that);
                    }
                },
                //url: url,                 //默认是form的action， 如果申明，则会覆盖  
                //type: type,               //默认是form的method（get or post），如果申明，则会覆盖  
                dataType: 'json',           //html(默认), xml, script, json...接受服务端返回的类型  
                //clearForm: true,          //成功提交后，清除所有表单元素的值  
                //resetForm: true,          //成功提交后，重置所有表单元素的值  
                timeout: 3000               //限制请求的时间，当请求大于3秒后，跳出请求  
            });

        }
        catch (ex) {
            $.err("出错:" + ex);
            var failCallback = $(this).attr("ah-fail-callback");
            if (failCallback) {
                //运行特定的失败函数
                var fc = eval(failCallback);
                new fc(event, this);
            }
        }
        finally { event.stopPropagation(); return false; }

    });
    /*----------------------------异步发送form表单.end -----------------------------------*/

    $('[ah-role-validate=require_one]>input[type=text]').on("change", function () {

    });

    //post json请求封装
    $.postJson = function (url, data, success){
        $.ajax({
            type: 'POST',
            url: url,
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            success: function (jn) {
                success(jn)
            },
        });
    }
});