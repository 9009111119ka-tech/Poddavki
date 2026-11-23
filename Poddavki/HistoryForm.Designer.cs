namespace Poddavki
{
    partial class HistoryForm
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
            this.btnClose = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.dgvMoves = new System.Windows.Forms.DataGridView();
            this.dgvGames = new System.Windows.Forms.DataGridView();
            this.lblGames = new System.Windows.Forms.Label();
            this.lblMoves = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMoves)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGames)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnClose.Location = new System.Drawing.Point(14, 323);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(364, 58);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Назад";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            this.btnExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnExport.Location = new System.Drawing.Point(384, 323);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(365, 58);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "Экспорт";
            this.btnExport.UseVisualStyleBackColor = true;
            // 
            // dgvMoves
            // 
            this.dgvMoves.AllowUserToAddRows = false;
            this.dgvMoves.AllowUserToDeleteRows = false;
            this.dgvMoves.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMoves.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMoves.Location = new System.Drawing.Point(384, 33);
            this.dgvMoves.MultiSelect = false;
            this.dgvMoves.Name = "dgvMoves";
            this.dgvMoves.ReadOnly = true;
            this.dgvMoves.RowHeadersVisible = false;
            this.dgvMoves.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMoves.Size = new System.Drawing.Size(365, 267);
            this.dgvMoves.TabIndex = 2;
            // 
            // dgvGames
            // 
            this.dgvGames.AllowUserToAddRows = false;
            this.dgvGames.AllowUserToDeleteRows = false;
            this.dgvGames.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvGames.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvGames.Location = new System.Drawing.Point(12, 33);
            this.dgvGames.MultiSelect = false;
            this.dgvGames.Name = "dgvGames";
            this.dgvGames.ReadOnly = true;
            this.dgvGames.RowHeadersVisible = false;
            this.dgvGames.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvGames.Size = new System.Drawing.Size(365, 267);
            this.dgvGames.TabIndex = 3;
            // 
            // lblGames
            // 
            this.lblGames.AutoSize = true;
            this.lblGames.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblGames.Location = new System.Drawing.Point(8, 9);
            this.lblGames.Name = "lblGames";
            this.lblGames.Size = new System.Drawing.Size(149, 24);
            this.lblGames.TabIndex = 4;
            this.lblGames.Text = "Список партий:";
            // 
            // lblMoves
            // 
            this.lblMoves.AutoSize = true;
            this.lblMoves.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblMoves.Location = new System.Drawing.Point(381, 9);
            this.lblMoves.Name = "lblMoves";
            this.lblMoves.Size = new System.Drawing.Size(140, 24);
            this.lblMoves.TabIndex = 5;
            this.lblMoves.Text = "Список ходов:";
            this.lblMoves.Click += new System.EventHandler(this.label2_Click);
            // 
            // HistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(761, 392);
            this.Controls.Add(this.lblMoves);
            this.Controls.Add(this.lblGames);
            this.Controls.Add(this.dgvGames);
            this.Controls.Add(this.dgvMoves);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnClose);
            this.Name = "HistoryForm";
            this.Text = "История партий";
            ((System.ComponentModel.ISupportInitialize)(this.dgvMoves)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGames)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.DataGridView dgvMoves;
        private System.Windows.Forms.DataGridView dgvGames;
        private System.Windows.Forms.Label lblGames;
        private System.Windows.Forms.Label lblMoves;
    }
}