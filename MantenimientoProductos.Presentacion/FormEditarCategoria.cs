using MantenimientoProductos.Aplicacion.Contratos;

namespace MantenimientoProductos.Presentacion;

public sealed class FormEditarCategoria : Form
{
    private readonly ICategoriaServicio _servicio;
    private readonly int? _idCategoria;
    private readonly TextBox _txtNombre;
    private readonly CheckBox _chkEstado;
    private readonly Button _btnGuardar;
    private readonly Button _btnCancelar;

    public FormEditarCategoria(ICategoriaServicio servicio, int? idCategoria)
    {
        _servicio = servicio;
        _idCategoria = idCategoria;

        Text = idCategoria is null ? "Nueva categoría" : $"Editar categoría #{idCategoria}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Width = 420;
        Height = 200;
        Padding = new Padding(12);

        var lblNombre = new Label { Text = "Nombre:", AutoSize = true, Top = 20, Left = 12 };
        _txtNombre = new TextBox { Left = 12, Top = 42, Width = 360 };
        _chkEstado = new CheckBox { Text = "Activo", Left = 12, Top = 78, Checked = true };

        _btnGuardar = new Button { Text = "Guardar", Left = 12, Top = 120, DialogResult = DialogResult.None };
        _btnCancelar = new Button { Text = "Cancelar", Left = 120, Top = 120, DialogResult = DialogResult.Cancel };

        AcceptButton = _btnGuardar;
        CancelButton = _btnCancelar;

        Controls.AddRange(new Control[] { lblNombre, _txtNombre, _chkEstado, _btnGuardar, _btnCancelar });

        _btnGuardar.Click += async (_, _) => await GuardarAsync().ConfigureAwait(true);
        Shown += async (_, _) => await CargarSiEdicionAsync().ConfigureAwait(true);
    }

    private async Task CargarSiEdicionAsync()
    {
        if (_idCategoria is null) return;
        try
        {
            var dto = await _servicio.ObtenerAsync(_idCategoria.Value).ConfigureAwait(true);
            if (dto is null)
            {
                MessageBox.Show(this, "No se encontró la categoría.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            _txtNombre.Text = dto.Nombre;
            _chkEstado.Checked = dto.Estado;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task GuardarAsync()
    
    {
        string nombre = _txtNombre.Text.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            MessageBox.Show(this,
                "El nombre es obligatorio.",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            _txtNombre.Focus();
            return;
        }

        if (nombre.Length < 3)
        {
            MessageBox.Show(this,
                "El nombre debe tener mínimo 3 caracteres.",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            _txtNombre.Focus();
            return;
        }

        foreach (char c in nombre)
        {
            if (!char.IsLetter(c) && c != ' ')
            {
                MessageBox.Show(this,
                    "El nombre solo puede contener letras.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _txtNombre.Focus();
                return;
            }
        }

        try
        {
            if (_idCategoria is null)
            {
                int nuevoId = await _servicio
                    .CrearAsync(nombre, _chkEstado.Checked)
                    .ConfigureAwait(true);

                MessageBox.Show(this,
                    $"Creado con Id {nuevoId}.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                bool ok = await _servicio
                    .ActualizarAsync(_idCategoria.Value, nombre, _chkEstado.Checked)
                    .ConfigureAwait(true);

                if (!ok)
                {
                    MessageBox.Show(this,
                        "No se pudo actualizar.",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                MessageBox.Show(this,
                    "Actualizado correctamente.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    private void TxtNombre_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsLetter(e.KeyChar) &&
            !char.IsControl(e.KeyChar) &&
            e.KeyChar != ' ')
        {
            e.Handled = true;
        }
    }
}
