using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Courier_service.Models;
using System.Configuration;

namespace Courier_service
{
    public partial class redactOrderForm : Form
    {
        NpgsqlConnection npgsqlConnection = null;
        OrderInfo order;
        int orderId = -1;

        public redactOrderForm(int id)
        {
            orderId = id;
            string connectionString = ConfigurationManager.ConnectionStrings["CourierServiceConnectionString"].ConnectionString;
            npgsqlConnection = new NpgsqlConnection(connectionString);
            InitializeComponent();
        }

        private void redactOrderForm_Load(object sender, EventArgs e)
        {
            if (orderId >= 0)
            {
                NpgsqlCommand command = npgsqlConnection.CreateCommand();
                NpgsqlDataReader reader = null;
                command.CommandText = @"SELECT o.""Id"" as ""OrderId"", c.""FName"", c.""SName"", c.""Phone"", p.""Description"", o.""Status"", c.""Id"", p.""Id"" " +
                    @"FROM ""Orders"" AS o JOIN ""Contact"" AS c ON(o.""ContactId"" = c.""Id"") JOIN ""Package"" AS p ON(o.""PackageId"" = p.""Id"")" +
                    @" WHERE o.""Id"" = " + orderId;

                try
                {
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command.CommandText, npgsqlConnection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    DataRow row = table.Rows[0];
                    orderNumber.DataBindings.Add(new Binding("Text", table, "OrderId"));
                    fnameTextBox.DataBindings.Add(new Binding("Text", table, "FName"));
                    snameTextBox.DataBindings.Add(new Binding("Text", table, "SName"));
                    phoneTextBox.DataBindings.Add(new Binding("Text", table, "Phone"));
                    descTextBox.DataBindings.Add(new Binding("Text", table, "Description"));
                    statusComboBox.DataBindings.Add(new Binding("Text", table, "Status"));


                    //npgsqlConnection.Open();
                    //reader = command.ExecuteReader();
                    //while (reader.Read())
                    //{
                    //    //ordersDataGrid.Rows.Add(reader["Id"], DateTime.Parse(reader["Date"].ToString()).ToLongDateString(), reader["Status"], reader["FName"], reader["SName"], reader["Phone"]
                    //    order = new OrderInfo()
                    //    {
                    //        OrderId = orderId,
                    //        ContactId = Convert.ToInt32(reader[0]),
                    //        PackageId = Convert.ToInt32(reader[1]),
                    //        FName = reader["FName"].ToString(),
                    //        SName = reader["SName"].ToString(),
                    //        Phone = reader["Phone"].ToString(),
                    //        Description = reader["Description"].ToString(),
                    //        Status = reader["Status"].ToString()
                    //    };

                    //}

                    //npgsqlConnection.Close();

                    //orderLabel.Text += " " + orderId;
                    //fnameTextBox.Text = order.FName;
                    //snameTextBox.Text = order.SName;
                    //phoneTextBox.Text = order.Phone;
                    //descTextBox.Text = order.Description;
                    //statusComboBox.Text = order.Status;
                        order = new OrderInfo() 
                        {
                            OrderId = Convert.ToInt32(row.ItemArray[0]),
                            FName = row.ItemArray[1].ToString(),
                            SName = row.ItemArray[2].ToString(),
                            Phone = row.ItemArray[3].ToString(),
                            Description = row.ItemArray[4].ToString(),
                            Status = row.ItemArray[5].ToString(),
                            PackageId = Convert.ToInt32(row.ItemArray[7]),
                            ContactId = Convert.ToInt32(row.ItemArray[6])
                        };
                }
                catch (Exception ex)
                {
                    //npgsqlConnection.Close();
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (fnameTextBox.Text != String.Empty && snameTextBox.Text != String.Empty && phoneTextBox.Text != String.Empty && order != null)
            {
                NpgsqlCommand commandContact = npgsqlConnection.CreateCommand();
                NpgsqlCommand commandPackage = npgsqlConnection.CreateCommand();
                NpgsqlCommand commandOrder = npgsqlConnection.CreateCommand();
                commandContact.CommandText = @"UPDATE ""Contact"" SET ""FName"" = '" + fnameTextBox.Text +
                    @"', ""SName"" = '" + snameTextBox.Text +
                    @"', ""Phone"" = '" + phoneTextBox.Text + @"' WHERE ""Id"" = " + order.ContactId;
                commandPackage.CommandText = @"UPDATE ""Package"" SET ""Description"" = '" + descTextBox.Text + @"' WHERE ""Id"" = " + order.PackageId;
                commandOrder.CommandText = @"UPDATE ""Orders"" SET ""Status"" = '" + statusComboBox.Text + @"' WHERE ""Id"" = " + order.PackageId;

                try
                {
                    npgsqlConnection.Open();
                    commandContact.ExecuteNonQuery();
                    commandPackage.ExecuteNonQuery();
                    commandOrder.ExecuteNonQuery();
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось сохранить!\n" + ex.Message);
                    this.DialogResult = DialogResult.Abort;
                }
                finally
                {
                    npgsqlConnection.Close();
                    Close();
                }
            }
            else MessageBox.Show("Введите контактные данные!");
        }
    }
}
