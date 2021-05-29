using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Courier_service
{
    public partial class AskStringForm : Form
    {
        public AskStringForm()
        {
            InitializeComponent();
        }

        public AskStringForm(string question)
        {
            this.Text = "Введите " + question;

            InitializeComponent();
            textLabel.Text = "Введите " + question;
        }

        public string Answer { get; set; }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (textBox.Text != "")
            {
                Answer = textBox.Text;
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.None;
            }
            Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
