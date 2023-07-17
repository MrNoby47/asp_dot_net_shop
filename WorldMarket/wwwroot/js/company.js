var CompanyTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    CompanyTable = $("#CompanyTb").DataTable({
        "ajax": {
            url: "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "state", "width": "15%" },
            { "data": "city", "width": "15%" },
            { "data": "phoneNnumber", "width": "15%" },
            {
                "data": "id",
                "render": function (data) { 
                            return `
                             <div class="w-75 btn-group" role="group">
                                   <a class="btn btn-primary mx-2"  href="/Admin/Company/Edit?id=${data}" ><i class="bi bi-pencil-square"></i></a>
                                  <a class="btn btn-secondary mx-2" Onclick=deleteCompany('/Admin/Company/Delete/${data}')><i class="bi bi-trash-fill"></i></a>
                               </div>
                 `},
                 "width":"15%"
            }
        ]
    });
}

function deleteCompany(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        CompanyTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    });
}