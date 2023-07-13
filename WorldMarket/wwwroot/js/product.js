var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $("#prodTable").DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "name","width":"15%" },
            { "data": "price", "width": "15%" },
            { "data": "price50", "width": "15%" },
            { "data": "price100", "width": "15%" },
            { "data": "category.name", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                              <div class="w-75 btn-group" role="group">
                                   <a class="btn btn-primary mx-2"  href="/Admin/Product/Upsert?id=${data}" ><i class="bi bi-pencil-square"></i></a>
                                  <a class="btn btn-secondary mx-2"  href="/Admin/Product/Delete?=${data}"><i class="bi bi-trash-fill"></i></a>
                               </div>

                            `
                },
                "width":"15%"
            }
        ]
    });
}