var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else {
        if (url.includes("inprocess")) {
            loadDataTable("inprocess");
        }
        else {
            if (url.includes("complete")) {
                loadDataTable("complete");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
   
});

function loadDataTable(status) {
    dataTable = $("#orderTable").DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll?status="+ status
        },
        "columns": [
            { "data": "id","width":"5%" },
            { "data": "name", "width": "25%" },
            { "data": "orderTotal", "width": "15%" },
            { "data": "paymentStatus", "width": "15%" },
            {"data":  "phoneNumber", "width":"10%"},
            { "data": "applicationUser.email", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                              <div class="w-75 btn-group" role="group">
                                   <a class="btn btn-primary mx-2"  href="/Admin/Order/Details?orderId=${data}" ><i class="bi bi-pencil-square"></i></a>
                               </div>

                            `
                },
                "width":"5%"
            }
        ]
    });
}
