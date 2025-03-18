$(document).ready(function() {
    // Inicializar validación del formulario
    const form = $('form');
    
    form.on('submit', function(e) {
        if (!form.valid()) {
            e.preventDefault();
            return;
        }

        const amount = parseFloat($('#Amount').val());
        if (amount <= 0) {
            e.preventDefault();
            toastr.error('El monto debe ser mayor a 0');
            return;
        }
    });

    // Formatear monto como moneda
    $('#Amount').on('change', function() {
        const amount = parseFloat($(this).val());
        if (!isNaN(amount)) {
            $(this).val(amount.toFixed(2));
        }
    });
});