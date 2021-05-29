using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Courier_service
{
    public partial class newExtClientForm : Form
    {
        NpgsqlConnection npgSqlConnection = null;

        public newExtClientForm()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["CourierServiceConnectionString"].ConnectionString;
            npgSqlConnection = new NpgsqlConnection(connectionString);
            try
            {
                npgSqlConnection.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                this.Close();
            }
            finally { npgSqlConnection.Close(); }
        }

        void saveNewExtClient()
        {
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            if (nameTextBox.Text != "" && OGRNTextBox.Text != "" && phoneTextBox.Text != "")
            {
                command.CommandText = @"INSERT INTO ""Clients"" (""FName"", ""SName"", ""Patronymic"", ""Deleted"") VALUES "+
                                      @"('" + nameTextBox.Text + "', '" + OGRNTextBox.Text + "', '" + phoneTextBox.Text +
                                      @"', false)";

                try
                {
                    npgSqlConnection.Open();
                    if (command.ExecuteNonQuery() != 1) MessageBox.Show("Компания не зарегистрирована!");
                    else { this.DialogResult = DialogResult.OK; MessageBox.Show("Компания зарегистрирована!"); }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                finally
                {
                    npgSqlConnection.Close();
                }
            }
            else { MessageBox.Show("Введены не все данные"); }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            saveNewExtClient();
        }
    }
}
