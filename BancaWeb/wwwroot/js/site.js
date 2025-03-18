// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Global Toastr configuration
toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "timeOut": "3000"
};

// Global DataTables configuration
$.extend(true, $.fn.dataTable.defaults, {
    "language": {
        "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Spanish.json"
    },
    "responsive": true,
    "pageLength": 10,
    "ordering": true,
    "info": true,
    "searching": true
});

// Global AJAX error handling
$(document).ajaxError(function (event, xhr, settings, error) {
    if (xhr.status === 404) {
        toastr.error("No se encontró el recurso solicitado");
    } else if (xhr.status === 400) {
        toastr.error("Solicitud inválida");
    } else if (xhr.status === 500) {
        toastr.error("Error interno del servidor");
    } else {
        toastr.error("Error: " + error);
    }
});

// Show success message if exists in TempData
$(document).ready(function () {
    const successMessage = $("#successMessage").val();
    if (successMessage) {
        toastr.success(successMessage);
    }
});

// Format currency inputs
$("input[data-type='currency']").on({
    keyup: function() {
        formatCurrency($(this));
    },
    blur: function() { 
        formatCurrency($(this), "blur");
    }
});

function formatCurrency(input, blur) {
    let input_val = input.val();
    if (input_val === "") { return; }
    
    let original_len = input_val.length;
    let caret_pos = input.prop("selectionStart");
    
    if (input_val.indexOf(".") >= 0) {
        let decimal_pos = input_val.indexOf(".");
        let left_side = input_val.substring(0, decimal_pos);
        let right_side = input_val.substring(decimal_pos);

        left_side = formatNumber(left_side);
        right_side = formatNumber(right_side);
        right_side = right_side.substring(0, 2);
        input_val = left_side + "." + right_side;
    } else {
        input_val = formatNumber(input_val);
        input_val = input_val + ".00";
    }
    
    input.val(input_val);
}

function formatNumber(n) {
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
