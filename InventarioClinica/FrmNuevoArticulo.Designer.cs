namespace InventarioClinica
{
    partial class FrmNuevoArticulo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmNuevoArticulo));
            this.cmbEstante = new System.Windows.Forms.ComboBox();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.txtPresentacion = new System.Windows.Forms.TextBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtConcentracion = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numMaximaCantidad = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPideMasVencera = new System.Windows.Forms.TextBox();
            this.numMinimaCantidad = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numMaximaCantidad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinimaCantidad)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbEstante
            // 
            this.cmbEstante.Font = new System.Drawing.Font("Calisto MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEstante.FormattingEnabled = true;
            this.cmbEstante.Location = new System.Drawing.Point(18, 96);
            this.cmbEstante.Name = "cmbEstante";
            this.cmbEstante.Size = new System.Drawing.Size(265, 27);
            this.cmbEstante.TabIndex = 1;
            // 
            // txtCodigo
            // 
            this.txtCodigo.Font = new System.Drawing.Font("Calisto MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCodigo.Location = new System.Drawing.Point(324, 96);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(115, 26);
            this.txtCodigo.TabIndex = 2;
            // 
            // txtNombre
            // 
            this.txtNombre.Font = new System.Drawing.Font("Calisto MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNombre.Location = new System.Drawing.Point(472, 96);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(165, 26);
            this.txtNombre.TabIndex = 3;
            // 
            // txtPresentacion
            // 
            this.txtPresentacion.Font = new System.Drawing.Font("Calisto MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPresentacion.Location = new System.Drawing.Point(658, 96);
            this.txtPresentacion.Name = "txtPresentacion";
            this.txtPresentacion.Size = new System.Drawing.Size(115, 26);
            this.txtPresentacion.TabIndex = 4;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Font = new System.Drawing.Font("Calisto MT", 12F);
            this.btnGuardar.Location = new System.Drawing.Point(415, 234);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(118, 35);
            this.btnGuardar.TabIndex = 5;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label1.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(28, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 18);
            this.label1.TabIndex = 6;
            this.label1.Text = "Estante";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label2.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(321, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 18);
            this.label2.TabIndex = 7;
            this.label2.Text = "Código";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label3.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(469, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 18);
            this.label3.TabIndex = 8;
            this.label3.Text = "Nombre";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label4.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(655, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 18);
            this.label4.TabIndex = 9;
            this.label4.Text = "Presentación";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label5.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(803, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 18);
            this.label5.TabIndex = 11;
            this.label5.Text = "Concentración";
            // 
            // txtConcentracion
            // 
            this.txtConcentracion.Font = new System.Drawing.Font("Calisto MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConcentracion.Location = new System.Drawing.Point(806, 98);
            this.txtConcentracion.Name = "txtConcentracion";
            this.txtConcentracion.Size = new System.Drawing.Size(115, 26);
            this.txtConcentracion.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label6.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(129, 155);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(203, 18);
            this.label6.TabIndex = 13;
            this.label6.Text = "Máxima Cantidad a pedir";
            // 
            // numMaximaCantidad
            // 
            this.numMaximaCantidad.Font = new System.Drawing.Font("Calisto MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numMaximaCantidad.Location = new System.Drawing.Point(132, 176);
            this.numMaximaCantidad.Name = "numMaximaCantidad";
            this.numMaximaCantidad.Size = new System.Drawing.Size(120, 26);
            this.numMaximaCantidad.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label7.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label7.Location = new System.Drawing.Point(572, 179);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(122, 18);
            this.label7.TabIndex = 16;
            this.label7.Text = "Si pide más de";
            // 
            // txtPideMasVencera
            // 
            this.txtPideMasVencera.Font = new System.Drawing.Font("Calisto MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPideMasVencera.Location = new System.Drawing.Point(700, 176);
            this.txtPideMasVencera.Name = "txtPideMasVencera";
            this.txtPideMasVencera.Size = new System.Drawing.Size(115, 26);
            this.txtPideMasVencera.TabIndex = 15;
            // 
            // numMinimaCantidad
            // 
            this.numMinimaCantidad.Font = new System.Drawing.Font("Calisto MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numMinimaCantidad.Location = new System.Drawing.Point(358, 176);
            this.numMinimaCantidad.Name = "numMinimaCantidad";
            this.numMinimaCantidad.Size = new System.Drawing.Size(120, 26);
            this.numMinimaCantidad.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label8.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(355, 155);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(197, 18);
            this.label8.TabIndex = 17;
            this.label8.Text = "Minima Cantidad a pedir";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.label9.Font = new System.Drawing.Font("Lucida Bright", 12F, System.Drawing.FontStyle.Bold);
            this.label9.Location = new System.Drawing.Point(821, 178);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 18);
            this.label9.TabIndex = 19;
            this.label9.Text = "se vencerá";
            // 
            // FrmNuevoArticulo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 308);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.numMinimaCantidad);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtPideMasVencera);
            this.Controls.Add(this.numMaximaCantidad);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtConcentracion);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.txtPresentacion);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.txtCodigo);
            this.Controls.Add(this.cmbEstante);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmNuevoArticulo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Articulo";
            this.Load += new System.EventHandler(this.FrmNuevoArticulo_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numMaximaCantidad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinimaCantidad)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbEstante;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.TextBox txtPresentacion;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtConcentracion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numMaximaCantidad;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtPideMasVencera;
        private System.Windows.Forms.NumericUpDown numMinimaCantidad;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
    }
}