using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace Courier_service
{
    public partial class NewRouteForm : Form
    {
        SqliteConnection connection;

        public NewRouteForm()
        {
            InitializeComponent();
            connection = new SqliteConnection("Data Source=d:/route.db");

            try
            {
                connection.Open();
                addButton.Enabled = true;
                connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (fromTextBox.Text != "" && toTextBox.Text != "")
            {
                try
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO route ('From', 'To') VALUES ('" + fromTextBox.Text +"', '" + toTextBox.Text + "')";
                    command.ExecuteNonQuery();
                    connection.Close();
                    loadRoute();

                }
                catch (Exception er) { MessageBox.Show(er.Message); }
            }
        }

        void loadRoute()
        {

            try
            {
                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM 'route'";

                SqliteDataReader reader = command.ExecuteReader();

                DataTable t = new DataTable();
                dataGridView1.Rows.Clear();
                while (reader.Read())
                {
                    dataGridView1.Rows.Add(reader["Id"], reader["From"], reader["To"]);
                }
                connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Load error: " +e.Message);
            }
        }
    }
}
