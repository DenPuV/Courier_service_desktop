using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
//using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;
using Courier_service.Models;
using System.Data.Common;

namespace Courier_service
{
    public partial class Form1 : Form
    {
        NpgsqlConnection npgSqlConnection = null;
        int currentOrderId = -1;
        string currentCourierUserId = "";
        string currentExtClientOGRN = "";

        public Form1()
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

        private void buttonPage1_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 1;
            pageLabel.Text = "Список курьеров";
            loadCouriers();
        }//Настраницу с курьерами

        private void buttonPage2_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 0;
            pageLabel.Text = "Список заказов";
            orderDateTimePicker.Value = DateTime.Now;
            includeDate.Checked = false;
            loadOrders();
        }//На страницу с заказами

        private void buttonPage3_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 3;
            pageLabel.Text = "Список компаний клиентов";
            loadExtClients();
        }//На страницу api клиентов

        private void buttonPage4_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 2;
            pageLabel.Text = "Регистрация курьера";
        }//На страницу регистрации курьера






        void loadOrders()
        {
            
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            bool addAnd = false;

            command.CommandText = @"SELECT o.""Id"", ""Date"", ""Status"", ""FName"", ""SName"", ""Phone"" FROM ""Orders"" AS o JOIN ""Contact"" AS c ON(o.""ContactId"" = c.""Id"")";
            string where = String.Empty;
            if (orderIdTextBox.Text != "" || phoneTextBox.Text != "" || fnameTextBox.Text != "" || snameTextBox.Text != "" || includeDate.Checked || statusComboBox.SelectedIndex > 0)
            {
                if (orderIdTextBox.Text != "") 
                {
                    if(where == "") where = "WHERE ";
                    where += @"o.""Id"" = '" + orderIdTextBox.Text + "' ";
                    addAnd = true;
                }
                if (includeDate.Checked) 
                {
                    if(where == "") where = "WHERE ";
                    if(addAnd) where += "AND ";
                    NpgsqlDate date = new NpgsqlDate(orderDateTimePicker.Value.Date);
                    where += @"o.""Date""::timestamp::date = '" + date + "' ";
                    addAnd = true;
                }
                if (fnameTextBox.Text != "")
                {
                    if(where == "") where = "WHERE ";
                    if (addAnd) where += "AND ";
                    addAnd = true;
                    where += @"c.""FName"" = '" + fnameTextBox.Text + "' ";
                }
                if (snameTextBox.Text != "")
                {
                    if(where == "") where = "WHERE ";
                    if (addAnd) where += "AND ";
                    addAnd = true;
                    where += @"c.""SName"" = '" + snameTextBox.Text + "' ";
                }
                if (phoneTextBox.Text != "")
                {
                    if (where == "") where = "WHERE ";
                    if (addAnd) where += "AND ";
                    addAnd = true;
                    where += @"c.""Phone"" = '" + phoneTextBox.Text + "' ";
                }
                if (statusComboBox.SelectedIndex > 0)
                {
                    if (where == "") where = "WHERE ";
                    if (addAnd) where += "AND ";
                    addAnd = true;
                    where += @"o.""Status"" = '" + statusComboBox.Text + "' ";
                }
                command.CommandText += where;
            }


            try
            {


                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command.CommandText, npgSqlConnection);
                System.Data.DataTable set = new System.Data.DataTable();
                adapter.Fill(set);
                set.Columns[0].ColumnName = "№";
                set.Columns[1].ColumnName = "Дата";
                set.Columns[2].ColumnName = "Статус";
                set.Columns[3].ColumnName = "Имя";
                set.Columns[4].ColumnName = "Фамилия";
                set.Columns[5].ColumnName = "Телефон";
                ordersDataGrid.DataSource = set;
                searchCountLabel.Text = "Найдено записей: " + set.Rows.Count;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                //npgSqlConnection.Close();
            }

        }//Поиск заказов
        async void loadOrderDetails(string id)
        {
            if (id == "" || id == null) return;
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            DbDataReader reader = null;
            command.CommandText = @"SELECT o.""Id"", c.""FName"", c.""SName"", c.""Phone"", o.""Date"", o.""Status"", " +
                                  @"r.""StartName"", r.""FinishName"", p.""Description"", p.""Weight"", o.""Price"" " +
                                  @"FROM ""Orders"" AS o " + 
                                  @"JOIN ""Contact"" AS c ON(o.""ContactId"" = c.""Id"") " +
                                  @"JOIN ""Route"" AS r ON(o.""RouteId"" = r.""Id"") " +
                                  @"JOIN ""Package"" AS p ON(o.""PackageId"" = p.""Id"") " +
                                  @" WHERE o.""Id"" = '" + id + "'";

            string idTemp = string.Empty;
            try
            {
                npgSqlConnection.Open();
                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    orderInfoGroupBox.Text = "Информация о заказе №" + reader["Id"].ToString();
                    idTemp = reader["Id"].ToString();
                    currentOrderId = Convert.ToInt32(idTemp);
                    fnameLabel.Text = reader["FName"].ToString();
                    snameLabel.Text = reader["SName"].ToString();
                    phoneLabel.Text = reader["Phone"].ToString();
                    orderDateLabel.Text = reader["Date"].ToString();
                    orderStatusLabel.Text = reader["Status"].ToString();
                    routeFromLabel.Text = reader["StartName"].ToString();
                    routeToLabel.Text = reader["FinishName"].ToString();
                    packageDescriptionLabel.Text = reader["Description"].ToString();
                    weightLabel.Text = reader["Weight"].ToString();
                    priceLabel.Text = reader["Price"].ToString() + " ₽";
                }
                redactBtn.Enabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                npgSqlConnection.Close();
                loadCourier(idTemp);
            }
        }//Полная информация о заказе
        async void loadCourier(string id)
        {
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            DbDataReader reader = null;
            command.CommandText = @"SELECT * FROM ""Courier"" AS c " +
                                  @"JOIN ""Delivery"" AS d ON(d.""CourierId"" = c.""Id"") " +
                                  @"JOIN ""Orders"" AS o ON(d.""OrderId"" = o.""Id"") " +
                                  @"WHERE o.""Id"" = '" + id + "'";

            try
            {
                npgSqlConnection.Open();
                reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        courierLabel.Text = reader["FName"].ToString() + Environment.NewLine +
                                            reader["Sname"].ToString() + Environment.NewLine +
                                            reader["Phone"].ToString();
                    }
                }
                else courierLabel.Text = "Не назначен";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                npgSqlConnection.Close();
            }
        }//Получение информации о курьера заказа

        async void loadCouriers()
        {

            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            DbDataReader reader = null;

            command.CommandText = @"SELECT * FROM ""Courier"" WHERE ""Deleted"" = FALSE";
            string where = String.Empty;
            if (courierFNameTextBox.Text != "" || courierSNameTextBox.Text != "" || courierPatronymicTextBox.Text != "" || courierPhoneTextBox.Text != "")
            {
                if (courierFNameTextBox.Text != "")
                {
                    if (where == "") where = "WHERE ";
                    where += @"""FName"" = '" + courierFNameTextBox.Text + "' ";
                }
                if (courierSNameTextBox.Text != "")
                {
                    if (where == "") where = "WHERE ";
                    where += @"""SName"" = '" + courierSNameTextBox.Text + "' ";
                }
                if (courierPatronymicTextBox.Text != "")
                {
                    if (where == "") where = "WHERE ";
                    where += @"""Patronymic"" = '" + courierPatronymicTextBox.Text + "' ";
                }
                if (courierPhoneTextBox.Text != "")
                {
                    if (where == "") where = "WHERE ";
                    where += @"""Phone"" = '" + courierPhoneTextBox.Text + "' ";
                }
                command.CommandText += where;
            }


            try
            {
                npgSqlConnection.Open();
                reader = await command.ExecuteReaderAsync();
                courierDataGrid.Rows.Clear();
                while (reader.Read())
                {
                    courierDataGrid.Rows.Add(reader["Id"], reader["SName"], reader["FName"], reader["Patronymic"], reader["Phone"]);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                npgSqlConnection.Close();
            }

        }//Поиск курьеров
        async void loadCourierDetails(string id)
        {
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            DbDataReader reader = null;
            command.CommandText = @"SELECT * FROM ""Courier"" WHERE ""Id"" = '" + id + "'";

            try
            {
                npgSqlConnection.Open();
                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    courierSNameLabel.Text = reader["SName"].ToString();
                    courierFNameLabel.Text = reader["FName"].ToString();
                    courierPatronymicLabel.Text = reader["Patronymic"].ToString();
                    courierPhoneLabel.Text = reader["Phone"].ToString();
                    currentCourierUserId = reader["AspUserId"].ToString();
                }

                fireBtn.Enabled = true;
                recPasswordBtn.Enabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                npgSqlConnection.Close();
            }
        }//Полная информаця о курьере

        bool validateNewCourierData()
        {
            return (emailTextBox.Text != "" && passwordTextBox.Text != "" && nameTextBox.Text != "" && famnameTextBox.Text != "" &&
                    patrBox.Text != "" && phoneBox.Text != "");
        }//Проверка данных курьера при регистрации
        void registerCourier()
        {
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            NpgsqlCommand command2 = npgSqlConnection.CreateCommand();
            if (validateNewCourierData())
            {
                string aspUserId = Hasher.NewId;
                command.CommandText = @"INSERT INTO ""AspNetUsers""(""Id"", ""UserName"", ""NormalizedUserName"", " +
                                      @"""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"", ""PhoneNumber"", ""TwoFactorEnabled"", " +
                                      @"""LockoutEnabled"", ""AccessFailedCount"", ""PhoneNumberConfirmed"") " +
                                      @"VALUES ('" + aspUserId + "', '" + emailTextBox.Text + "', '" + emailTextBox.Text.ToUpper() + "', TRUE, '" + Hasher.HashPassword(passwordTextBox.Text) + "', " +
                                      "'" + Hasher.SecurityStamp + "', '" + Hasher.ConcurrencyStamp + "', '" + phoneBox.Text + "', false, false, 0, TRUE)";

                command2.CommandText = @"INSERT INTO ""Courier""(""FName"", ""SName"", ""Patronymic"", ""Phone"", ""AspUserId"") "+
                                       @"VALUES ('" + nameTextBox.Text + "', '" + famnameTextBox.Text + "', '" + patrBox.Text + "', " +
                                       @"'" + phoneBox.Text + "', '" + aspUserId + "')";

                try
                {
                    npgSqlConnection.Open();
                    if (command.ExecuteNonQuery() + command2.ExecuteNonQuery() != 2) MessageBox.Show("Курьер не зарегистрирован!");
                    else MessageBox.Show("Курьер зарегистрирован!");
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
        }//Регистрация нового курьера
        async void loadExtClients()
        {
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            DbDataReader reader = null;

            command.CommandText = @"SELECT * FROM ""Clients"" WHERE ""Deleted"" = false AND ""AspName"" IS NULL";


            try
            {
                npgSqlConnection.Open();
                reader = await command.ExecuteReaderAsync();
                extClientDataGrid.Rows.Clear();
                while (reader.Read())
                {
                    extClientDataGrid.Rows.Add(reader["FName"], reader["SName"], reader["Patronymic"], reader["Id"]);
                }
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
        async void fireCourierAsync()
        {
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            command.CommandText = @"DELETE FROM ""AspNetUsers"" WHERE ""Id"" = '" + currentCourierUserId + "'";

            try
            {
                npgSqlConnection.Open();
                await command.ExecuteNonQueryAsync();
                MessageBox.Show("Курьер уволен!");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                npgSqlConnection.Close();
                loadCouriers();
            }
        }
        async void recoverPassword(string password)
        {
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            command.CommandText = @"UPDATE ""AspNetUsers"" SET ""PasswordHash"" = '" + Hasher.HashPassword(password) + @"' WHERE ""Id"" = '" + currentCourierUserId + "'";

            try
            {
                npgSqlConnection.Open();
                await command.ExecuteNonQueryAsync();
                MessageBox.Show("Пароль изменен!");
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
        async void closeApi()
        {
            NpgsqlCommand command = npgSqlConnection.CreateCommand();
            command.CommandText = @"UPDATE ""Clients"" SET ""Deleted"" = TRUE WHERE ""SName"" = '" + currentExtClientOGRN + "'";

            try 
            {
                npgSqlConnection.Open();
                if (await command.ExecuteNonQueryAsync() > 0)
                    MessageBox.Show("Доступ " + currentExtClientLabel.Text + " к API закрыт");
                else
                    MessageBox.Show("Доступ не закрыт!");
            }
            catch 
            {
                MessageBox.Show("Доступ не закрыт!");
            }
            finally 
            {
                npgSqlConnection.Close();
                loadExtClients();
            };
        }


        private void orderSearchButton_Click(object sender, EventArgs e)
        {
            loadOrders();
        }//Начинает поиск заказов
        private void ordersDataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var cellVal = ordersDataGrid.Rows[ordersDataGrid.CurrentCell.RowIndex].Cells[0].Value;
            if (cellVal != null)
                loadOrderDetails(cellVal.ToString());
        }//Начинает загружать данные о заказе
        private void button1_Click(object sender, EventArgs e)
        {
            registerCourier();
        }//Регистрирует курьера

        private void redactBtn_Click(object sender, EventArgs e)
        {
            if (currentOrderId >= 0)
            {
                redactOrderForm redactOrderForm = new redactOrderForm(currentOrderId);
                if (redactOrderForm.ShowDialog() == DialogResult.OK)
                {
                    loadOrderDetails(currentOrderId.ToString());
                }
            }
        }//Открывает форму редактирования заказа

        private void searchCourierBtn_Click(object sender, EventArgs e)
        {
            loadCouriers();
        }//Начинает поиск курьеров

        private void courierDataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var cellVal = courierDataGrid.Rows[courierDataGrid.CurrentCell.RowIndex].Cells[0].Value;
            if (cellVal != null)
                loadCourierDetails(cellVal.ToString());
        }//Начинает загрузку информации о курьерах

        private void newExtClient_Click(object sender, EventArgs e)
        {
            newExtClientForm form = new newExtClientForm();
            if (form.ShowDialog() == DialogResult.OK) { }
        }

        private void fireBtn_Click(object sender, EventArgs e)
        {
            if (currentCourierUserId != "")
            {
                fireCourierAsync();
            }
        }

        private void recPasswordBtn_Click(object sender, EventArgs e)
        {
            AskStringForm form = new AskStringForm("пароль");

            if (form.ShowDialog() == DialogResult.OK)
            {
                recoverPassword(form.Answer);
            }
        }

        private void extClientDataGrid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var cellVal = extClientDataGrid.Rows[extClientDataGrid.CurrentCell.RowIndex].Cells[1].Value;
            if (cellVal != null)
            {
                currentExtClientLabel.Text = "Название: " + extClientDataGrid.Rows[extClientDataGrid.CurrentCell.RowIndex].Cells[0].Value.ToString();
                currentExtClientPhoneLabel.Text = "Телефон: " + extClientDataGrid.Rows[extClientDataGrid.CurrentCell.RowIndex].Cells[2].Value.ToString();
                currentExtClientKeyLabel.Text = "API ключ: " + extClientDataGrid.Rows[extClientDataGrid.CurrentCell.RowIndex].Cells[3].Value.ToString();
                currentExtClientOGRN = cellVal.ToString();
                closeApiBtn.Enabled = true;
            }
        }

        private void closeApiBtn_Click(object sender, EventArgs e)
        {
            closeApi();
        }
    }
}
