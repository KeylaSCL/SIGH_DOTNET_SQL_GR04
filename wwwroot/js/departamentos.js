(function () {
    'use strict';

    var modal = document.getElementById('modalEditar');
    if (!modal) { return; }

    var campoCodigo = document.getElementById('codigoEdicion');
    var campoDescripcion = document.getElementById('descripcionEdicion');

    function abrir(codigo, descripcion) {
        campoCodigo.value = codigo;
        campoDescripcion.value = descripcion;
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
            abrir(editar.dataset.codigo, editar.dataset.descripcion);
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
        if (form && !confirm('¿Eliminar el departamento ' + form.dataset.codigo +
                             '? Esta acción no se puede deshacer.')) {
            e.preventDefault();
        }
    });

    // Si el servidor devolvió la ventana abierta (error al guardar), enfocar el campo
    if (!modal.classList.contains('oculto')) {
        campoDescripcion.focus();
    }
})

    /* ===== CRUD Cargos: list box de departamentos ===== */
    .crud - premium.dep - field select {
    width: 100 %; min - width: 0; font - family: inherit; font - size: .9rem;
    color: #293442; background: #fff; border: 1.5px solid #d2dae1;
    border - radius: 10px; padding: 6px;
    transition: border - color .16s ease, box - shadow .16s ease;
}
.crud - premium.dep - field select:focus {
    outline: none; border - color: #2a85d0; box - shadow: 0 0 0 3px rgba(31, 111, 181, .18);
}
.crud - premium.dep - field select option { padding: 9px 10px; border - radius: 6px; }
.crud - premium.dep - field select option:checked {
    background: #1f6fb5 linear - gradient(0deg, #1f6fb5, #1f6fb5); color: #fff;
}
.crud - premium.dep - warning {
    margin - bottom: 14px; padding: 11px 13px; border - radius: 10px;
    background: #fff7ed; border: 1px solid #f3d3b3; color: #9a5a22;
    font - size: .78rem; line - height: 1.5;
}
.crud - premium.dep - btn:disabled { opacity: .55; cursor: not - allowed; }
@media(max - width: 620px) {
  .crud - premium.dep - table tbody tr td.col - depto {
        grid - column: 2; align - self: center; overflow - wrap: anywhere;
    }
}();


