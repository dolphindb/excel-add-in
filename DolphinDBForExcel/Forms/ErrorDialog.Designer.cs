namespace DolphinDBForExcel.Forms
{
    partial class ErrorDialog
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Message = new System.Windows.Forms.TabPage();
            this.ErrorMessageTextBox = new System.Windows.Forms.TextBox();
            this.Detail = new System.Windows.Forms.TabPage();
            this.ErrorDetailTextBox = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.Message.SuspendLayout();
            this.Detail.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Message);
            this.tabControl1.Controls.Add(this.Detail);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(428, 315);
            this.tabControl1.TabIndex = 0;
            // 
            // Message
            // 
            this.Message.Controls.Add(this.ErrorMessageTextBox);
            this.Message.Location = new System.Drawing.Point(4, 25);
            this.Message.Name = "Message";
            this.Message.Padding = new System.Windows.Forms.Padding(3);
            this.Message.Size = new System.Drawing.Size(420, 286);
            this.Message.TabIndex = 0;
            this.Message.Text = "Message";
            this.Message.UseVisualStyleBackColor = true;
            // 
            // ErrorMessageTextBox
            // 
            this.ErrorMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorMessageTextBox.Location = new System.Drawing.Point(3, 3);
            this.ErrorMessageTextBox.Multiline = true;
            this.ErrorMessageTextBox.Name = "ErrorMessageTextBox";
            this.ErrorMessageTextBox.Size = new System.Drawing.Size(414, 280);
            this.ErrorMessageTextBox.TabIndex = 0;
            // 
            // Detail
            // 
            this.Detail.Controls.Add(this.ErrorDetailTextBox);
            this.Detail.Location = new System.Drawing.Point(4, 25);
            this.Detail.Name = "Detail";
            this.Detail.Padding = new System.Windows.Forms.Padding(3);
            this.Detail.Size = new System.Drawing.Size(420, 286);
            this.Detail.TabIndex = 1;
            this.Detail.Text = "Error Detail";
            this.Detail.UseVisualStyleBackColor = true;
            // 
            // ErrorDetailTextBox
            // 
            this.ErrorDetailTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorDetailTextBox.Location = new System.Drawing.Point(3, 3);
            this.ErrorDetailTextBox.Multiline = true;
            this.ErrorDetailTextBox.Name = "ErrorDetailTextBox";
            this.ErrorDetailTextBox.Size = new System.Drawing.Size(414, 280);
            this.ErrorDetailTextBox.TabIndex = 1;
            // 
            // ErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 315);
            this.Controls.Add(this.tabControl1);
            this.Name = "ErrorDialog";
            this.Text = "ErrorForm";
            this.tabControl1.ResumeLayout(false);
            this.Message.ResumeLayout(false);
            this.Message.PerformLayout();
            this.Detail.ResumeLayout(false);
            this.Detail.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Message;
        private System.Windows.Forms.TextBox ErrorMessageTextBox;
        private System.Windows.Forms.TabPage Detail;
        private System.Windows.Forms.TextBox ErrorDetailTextBox;
    }
}