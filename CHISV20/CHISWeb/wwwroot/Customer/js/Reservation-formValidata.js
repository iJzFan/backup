$(document).ready(function() {
	//1.手机号码验证
	jQuery.validator.addMethod("isPhone", function(value, element) {
		var length = value.length;
		return this.optional(element) || (length == 11 && /^(((13[0-9]{1})|(15[0-9]{1})|(17[0-9]{1})|(18[0-9]{1}))+\d{8})$/.test(value));
	}, "请正确填写您的手机号码。");
    //2. 身份证号码验证 
	jQuery.validator.addMethod("isIdCardNo", function (value, element) {
	    return this.optional(element) || idCardNoUtil.checkIdCardNo(value);
	}, "请正确输入您的身份证号码");
	//3.电话号码验证
	jQuery.validator.addMethod("isVerifyCode", function (value, element) {
	    var tel = "^\d{n}$"; // 区号－3、4位 号码－7、8位/^(\d{3,4}-)?\d{7,8}$/g
		return this.optional(element) || (tel.test(value));
	}, "请正确填写您的电话号码。");
    //4. 匹配密码，以字母开头，长度在6-12之间，必须包含数字和特殊字符。
	jQuery.validator.addMethod("isPwd", function(value, element) {
		var str = value;
		if (str.length < 6 || str.length > 18)
			return false;
		if (!/^[a-zA-Z]/.test(str)) 
			return false;
		if (!/[0-9]/.test(str))
			return fasle;
		return this.optional(element) || /[^A-Za-z0-9]/.test(str);
	}, "以字母开头，长度在6-12之间，必须包含数字和特殊字符。");
    //5. 邮政编码验证 
	jQuery.validator.addMethod("isZipCode", function (value, element) {
	    var tel = /^[0-9]{6}$/;
	    return this.optional(element) || (tel.test(value));
	}, "请正确填写您的邮政编码");
    //护照编号验证
	jQuery.validator.addMethod("passport", function (value, element) {
	    return this.optional(element) || checknumber(value);
	}, "请正确输入您的护照编号");

	$("#registerForm").validate({
		errorElement : 'span',
		errorClass: 'help-block',

		rules : {
		    CustomerName: "required",
			email : {
				required : true,
				email : true
			},
			password : {
				required : true,
				isPwd : true
			},
			confirm_password : {
				required : true,
				isPwd : true,
				equalTo : "#password"
			},
			IDcard: {
			    required: false,
			    isIdCardNo: true
			},
			Telephone: {
				required : true,
				isPhone : true
			},
			verifyCode: {
                required:true,
			    isVerifyCode: true
			},
			Address: {
				minlength : 5
			},
			passport: {
			    required: true,
			    passport: true
			}
		},
		messages : {
		    CustomerName: "请输入姓名",//请输入姓名
			email : {
				required : "请输入Email地址",
				email : "请输入正确的email地址"
			},
			password : {
				required : "请输入密码",
				minlength : jQuery.format("密码不能小于{0}个字 符")
			},
			confirm_password : {
				required : "请输入确认密码",
				minlength : "确认密码不能小于5个字符",
				equalTo : "两次输入密码不一致不一致"
			},
             IDcard: {
                 required: "请输入有效的身份证号码 "
            },
             Telephone: {
				required : "请输入手机号码"
			},
             verifyCode: {
				required : "请输入4位验证码"
			},
             Address: {
				required : "请输入家庭地址",
				minlength : jQuery.format("家庭地址不能少于{0}个字符")
             },
             passport: {
                 required: "请输入护照编号",
                 passport: "请输入正确的护照编号"
             }
		},
		errorPlacement : function(error, element) {
			//element.next().remove();
			element.after('<span class="glyphicon glyphicon-remove form-control-feedback" aria-hidden="true"></span>');
			element.closest('.form-group').append(error);
		},
		highlight : function(element) {
			$(element).closest('.form-group').addClass('has-error has-feedback');
		},
		success : function(label) {
			var el=label.closest('.form-group').find("input");
			el.next().remove();
			el.after('<span class="glyphicon glyphicon-ok form-control-feedback" aria-hidden="true"></span>');
			label.closest('.form-group').removeClass('has-error').addClass("has-feedback has-success");
			label.remove();
		},
		submitHandler: function (form) {
		    form.submit();
		}

	})
});