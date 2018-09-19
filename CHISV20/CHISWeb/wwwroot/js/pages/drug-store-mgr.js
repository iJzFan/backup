function deleteStaff(loginExtId) {
    $.deleteData({
        url: "/Nurse/DeleteLoginExt",
        data: { loginExtId: loginExtId },
        fnSuccess: function () { $.ok("成功删除了！"); }
    });
}
//设置可用
function setEnabledStaff(loginExtId, bEnable) {
    $.get("/Nurse/SetEnabledStaff", { loginExtId: loginExtId, bEnable: bEnable }, function (html) {
        $('#MyStoreStaff').html(html);
    });
}
//修改
function editStaff(loginExtId) {
    $.get("/Nurse/EditLoginExt", { loginExitId: loginExtId }, function (html) {
        $('#MyStaffEdit').html(html);
    });
}
//新增
function createNewStaff() {
    $.get("/Nurse/EditLoginExt", { loginExitId: 0 }, function (html) {
        $('#MyStaffEdit').html(html);
    });
}

function sendLoginEditSuccess(jn) {
    if (jn.rlt) {
        $.ok("添加成功");
        $.get("/Nurse/LoadPvMyStaff", null, function (html) {
            $('#MyStoreStaff').html(html);
            $('#MyStaffEdit').empty();
        });
    } else {
        $.err("添加失败:" + jn.msg);
    }
}
function sendLoginEditFailed(jn) { $.err("传输失败了！"); }



function seeRoleDetail(_this) {
    $.getJson("/Nurse/SeeRoleDetail", { roleKey: _this.value }, function (jn) {
        var h = "<ul class='ah-role-detail'>";
        $.each(jn, function (i, m) {
            h += "<li>" + m.loginExtRoleFuncRmk + "(" + m.loginExtRoleFuncKey + ")" + "</li>";
        });
        h += "<ul>"
        $('#roleDetail').html(h);
    });
}

function initialData() {
    var roles = $('#LoginExtRoleKeys').val().split(',');
    $.each(roles, function (i, m) {
        if (m != null && m.length > 0) {
            var opt = $('#canSelRoles option[value=' + m + ']');
            $('#selectedRoles').append(opt);
        }
    });
}

function addRole(_this) {
    var $top = $(_this).parents(".ah-addrole-grp").first();
    var sel = $top.find("select#canSelRoles");
    var v = sel.val();
    if (v) {
        var $opt = sel.find("option:selected");
        var $targ = $top.find('select#selectedRoles');
        if (isExistOption($targ, v)) return;
        else {
            $targ.append($opt);
            // sel.remove($opt);
        }
        updateRoles();
    }
}
function removeRole(_this) {
    var $top = $(_this).parents(".ah-addrole-grp").first();
    var sel = $top.find("select#selectedRoles");
    var v = sel.val();
    if (v) {
        var $opt = sel.find("option:selected");
        var $targ = $top.find('select#canSelRoles');
        if (isExistOption($targ, v)) return;
        else {
            $targ.append($opt);
            //  sel.remove($opt);
        }
        updateRoles();
    }
}
function updateRoles() {
    var vals = new Array();
    $('#selectedRoles option').each(function () {
        var txt = $(this).val();   //获取option值   
        if (txt != '') vals.push(txt);
    });
    if (vals.length == 0) $('#LoginExtRoleKeys').val('');
    else $('#LoginExtRoleKeys').val(vals.join(","));
}

//判断是否存在select选项
function isExistOption($select, value) {
    var isExist = false;
    var count = $select.find('option').length;
    for (var i = 0; i < count; i++) {
        if ($select.get(0).options[i].value == value) {
            isExist = true;
            break;
        }
    }
    return isExist;
}  