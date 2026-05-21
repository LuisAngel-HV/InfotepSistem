using MantenimientoProductos.Aplicacion.Contratos;
using MantenimientoProductos.Aplicacion.Dtos;

namespace MantenimientoProductos.Presentacion;

public sealed class FormCategorias : Form
{
    private readonly ICategoriaServicio _servicio;
    private readonly DataGridView _rejilla;
    private readonly Button _btnRefrescar;
    private readonly Button _btnNuevo;
    private readonly Button _btnEditar;
    private readonly Button _btnEliminar;

    public FormCategorias(ICategoriaServicio servicio)
    {
        _servicio = servicio;

        Text = "Categorías — CRUD (Onion)";
        Width = 960;
        Height = 560;
        StartPosition = FormStartPosition.CenterScreen;

        _rejilla = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false
        };
        _rejilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CategoriaDto.IdCategoria), HeaderText = "Id", Width = 50 });
        _rejilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CategoriaDto.Nombre), HeaderText = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _rejilla.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(CategoriaDto.Estado), HeaderText = "Activo", Width = 60 });
        _rejilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CategoriaDto.FechaRegistro), HeaderText = "Registro", Width = 180 });

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(8),
            WrapContents = false
        };
        _btnRefrescar = new Button { Text = "Refrescar", AutoSize = true };
        _btnNuevo = new Button { Text = "Nuevo", AutoSize = true };
        _btnEditar = new Button { Text = "Editar", AutoSize = true };
        _btnEliminar = new Button { Text = "Eliminar", AutoSize = true };
        panel.Controls.AddRange(new Control[] { _btnRefrescar, _btnNuevo, _btnEditar, _btnEliminar });
        _btnEditar.Enabled = false;
        _btnEliminar.Enabled = false;

        Controls.Add(_rejilla);
        Controls.Add(panel);

        _btnRefrescar.Click += async (_, _) => await CargarAsync().ConfigureAwait(true);
        _btnNuevo.Click += (_, _) => AbrirEditor(null);
        _btnEditar.Click += (_, _) =>
        _rejilla.selectionChanged += (_, _) =>
        bool haySeleccion = _rejilla.CurrenctRow is not null;
        _btnEditar.Enabled = hayseleccion;
        _btnEliminar.Enabled = hayseleccion;

        {
            int? id = ObtenerIdSeleccionado();
            if (id is null)
            {
                MessageBox.Show(this, "Seleccione una fila.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AbrirEditor(id);
        };
        _btnEliminar.Click += async (_, _) => await EliminarSeleccionadoAsync().ConfigureAwait(true);

        Shown += async (_, _) => await CargarAsync().ConfigureAwait(true);
    }

    private int? ObtenerIdSeleccionado()
    {
        if (_rejilla.CurrentRow?.DataBoundItem is not CategoriaDto dto) return null;
        return dto.IdCategoria;
    }

    private void AbrirEditor(int? idCategoria)
    {
        using var editor = new FormEditarCategoria(_servicio, idCategoria);
        if (editor.ShowDialog(this) == DialogResult.OK)
            _ = CargarAsync();
    }

    private async Task EliminarSeleccionadoAsync()
    {
        int? id = ObtenerIdSeleccionado();
        if (id is null)
        {
            MessageBox.Show(this, "Seleccione una fila.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, "¿Eliminar la categoría seleccionada?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            bool ok = await _servicio.EliminarAsync(id.Value).ConfigureAwait(true);
            MessageBox.Show(this, ok ? "Eliminado." : "No se encontró el registro.", Text, MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            await CargarAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error al eliminar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarAsync()
    {
        try
        {
            _rejilla.DataSource = null;
            IReadOnlyList<CategoriaDto> datos = await _servicio.ListarAsync().ConfigureAwait(true);
            _rejilla.DataSource = datos.ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error al listar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
