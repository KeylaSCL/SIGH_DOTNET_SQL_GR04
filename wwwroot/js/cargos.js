(function () {
    'use strict';

    var modal = document.getElementById('modalEditar');
    if (!modal) { return; }

    var campoCodigo = document.getElementById('codigoEdicion');
    var campoDescripcion = document.getElementById('descripcionEdicion');
    var campoDepartamento = document.getElementById('departamentoEdicion');

    function abrir(codigo, descripcion, departamento) {
        campoCodigo.value = codigo;
        campoDescripcion.value = descripcion;
        campoDepartamento.value = departamento; // deja seleccionado el departamento actual
        modal.classList.remove('oculto');
        campoDescripcion.focus();
        campoDescripcion.select();
    }

    function cerrar() {
        modal.classList.add('oculto');
    }

    document.addEventListener('click', function (e) {
        var editar = e.target.closest('[data-accion="editar"]');
        if (editar) {
            abrir(editar.dataset.codigo, editar.dataset.descripcion, editar.dataset.departamento);
            return;
        }
        if (e.target.closest('[data-accion="cancelar-edicion"]') || e.target === modal) {
            cerrar();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') { cerrar(); }
    });

    // Confirmación antes de eliminar
    document.addEventListener('submit', function (e) {
        var form = e.target.closest('form.form-eliminar');
        if (form && !confirm('¿Eliminar el cargo ' + form.dataset.codigo +
            '? Esta acción no se puede deshacer.')) {
            e.preventDefault();
        }
    });

    // Si el servidor devolvió la ventana abierta (error al guardar), enfocar el campo
    if (!modal.classList.contains('oculto')) {
        campoDescripcion.focus();
    }
})();