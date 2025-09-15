/* Variables */
var p = $("#participants").val();
var row = $(".participantRow");

/* Functions */
function getP() {
    p = $("#participants").val();
}

function addRow() {
    row.clone(true, true).appendTo("#participantTable");
}

function removeRow(button) {
    button.closest("tr").remove();
}
/* Doc ready */
$(".add").on('click', function () {
    getP();
    if ($("#participantTable tr").length < 17) {
        addRow();
        var i = Number(p) + 1;
        $("#participants").val(i);
    }
    $(this).closest("tr").appendTo("#participantTable");
    if ($("#participantTable tr").length === 0) {
        $(".remove").hide();
    } else {
        $(".remove").show();
    }
});
$(".remove").on('click', function () {
    getP();
    if ($("#participantTable tr").length === 1) {
        //alert("Can't remove row.");
        $(".remove").hide();
    } else if ($("#participantTable tr").length - 1 == 0) {
        $(".remove").hide();
        removeRow($(this));
        var i = Number(p) - 1;
        $("#participants").val(i);
    } else {
        removeRow($(this));
        var i = Number(p) - 1;
        $("#participants").val(i);
    }
});
$("#participants").change(function () {
    var i = 0;
    p = $("#participants").val();
    var rowCount = $("#participantTable tr").length - 2;
    if (p > rowCount) {
        for (i = rowCount; i < p; i += 1) {
            addRow();
        }
        $("#participantTable #addButtonRow").appendTo("#participantTable");
    } else if (p < rowCount) {
    }
});