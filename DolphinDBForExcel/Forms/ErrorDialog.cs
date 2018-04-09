using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DolphinDBForExcel.Forms
{
    public partial class ErrorDialog : Form
    {

        public ErrorDialog()
        {
            InitializeComponent();
        }

        public static ErrorDialog CreateFrom(Exception e)
        {
            return new ErrorDialog
            {
                ErrorMessageText = e.Message,
                ErrorDetailText = e.ToString()
            };
        }

        public static ErrorDialog CreateFrom(string errorMessage,string errorDetail)
        {
            return new ErrorDialog
            {
                ErrorMessageText = errorMessage,
                ErrorDetailText = errorDetail
            };
        }

        public string ErrorMessageText
        {
            get { return this.ErrorMessageTextBox.Text; }
            set { ErrorMessageTextBox.Text = value; }
        }

        public string ErrorDetailText
        {
            get { return this.ErrorDetailTextBox.Text; }
            set { ErrorDetailTextBox.Text = value; }
        }
    }
}
