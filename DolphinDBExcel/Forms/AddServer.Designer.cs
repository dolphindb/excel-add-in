using DolphinDBForExcel.Ribbon;
using System;
using System.Windows.Forms;

namespace DolphinDBForExcel.Forms
{
    partial class AddServer
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
            AddinRibbon.RibbonController.Invalidate();
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
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.login1 = new DolphinDBForExcel.WPFControls.Login(WpfControl_Close);
            this.SuspendLayout();
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(532, 323);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.ChildChanged += new System.EventHandler<System.Windows.Forms.Integration.ChildChangedEventArgs>(this.elementHost1_ChildChanged_1);
            this.elementHost1.Child = this.login1;
            // 
            // LoginForm
            // 
            this.ClientSize = new System.Drawing.Size(532, 323);
            this.Controls.Add(this.elementHost1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "LoginForm";
            this.ResumeLayout(false);

        }

        private void WpfControl_Close(DialogResult result)
        {
            //Manage the result as you like 
            DialogResult = result;
            Close();
        }

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private WPFControls.Login login1;

        #endregion

        //private System.Windows.Forms.Integration.ElementHost elementHost1;
        //private DolphinDBForExcelWPFLib.Login loginWin;
    }
}