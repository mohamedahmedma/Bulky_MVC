﻿var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/user/getall' },

        "columns": [
            { data: 'name' },
            { data: 'email' },
            { data: 'phoneNumber' },
            { data: 'company.name' },
            { data: 'role' },
            {
                data: {id : 'id' , lockoutEnd : 'lockoutEnd'},
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        return  `
                                    <div class="text-center">
                                       <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="curdor:pointer; width: 100px;">
                                            <i class="bi bi-lock-fill"></i> Lock
                                        </a>
                                        <a href="/admin/user/RoleManagment?userId=${data.id}" class="btn btn-danger text-white" style="curdor:pointer; width: 150px;">
                                            <i class="bi bi-pencil-square"></i> Permission
                                        </a>
                                    </div>
                        
                                `
                    }
                    else {
                        return `
                                    <div class="text-center">
                                     <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white" style="curdor:pointer; width: 100px;">
                                            <i class="bi bi-unlock-fill"></i> Unlock
                                        </a>
                                        
                                        <a href="/Admin/User/RoleManagment?userId=${data.id}" class="btn btn-danger text-white" style="curdor:pointer; width: 150px;">
                                            <i class="bi bi-pencil-square"></i> Permission
                                        </a>
                                    </div>
                        
                                `
                    }
                    
                },
                "width": "25%"
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                dataTable.ajax.reload();
                toastr.success(data.message);
            }
            
        }
        
    });
}


