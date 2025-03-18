$(document).ready(function() {
    // Inicializar DataTable
    $('.table').DataTable({
        "order": [[0, "desc"]], // Ordenar por fecha descendente
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Spanish.json"
        },
        "responsive": true,
        "pageLength": 10
    });
});