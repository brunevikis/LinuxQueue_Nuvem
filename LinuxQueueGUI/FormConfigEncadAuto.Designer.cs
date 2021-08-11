namespace LinuxQueueGUI
{
    partial class FormConfigEncadAuto
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
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.userControl12 = new LinuxQueueGUI.UserControl1();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(234, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Dados para a execução automática Encadeado";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(494, 215);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 32);
            this.button1.TabIndex = 22;
            this.button1.Text = "Aceitar Alterações";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // userControl12
            // 
            this.userControl12.Argument = "";
            this.userControl12.Command = "";
            this.userControl12.EnviarEmail = false;
            this.userControl12.Location = new System.Drawing.Point(11, 37);
            this.userControl12.Name = "userControl12";
            this.userControl12.Nome = "";
            this.userControl12.Size = new System.Drawing.Size(606, 172);
            this.userControl12.TabIndex = 23;
            this.userControl12.WorkingDirectory = "";
            // 
            // FormConfigEncadAuto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 290);
            this.Controls.Add(this.userControl12);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Name = "FormConfigEncadAuto";
            this.Text = "FormConfigEncadAuto";
            this.Load += new System.EventHandler(this.FormConfigEncad_Load);

            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private UserControl1 userControl12;
    }
}