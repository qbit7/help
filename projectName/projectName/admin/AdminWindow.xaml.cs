    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using projectName.admin;
    using projectName.user;

    namespace projectName.admin
    {
        public partial class AdminWindow : Window
        {
            // Строка подключения к БД
            readonly string connectionString = ConfigurationManager.ConnectionStrings["nameDB"].ConnectionString;

            private int currentAdminId;
            private string currentAdminName;

            // Переменные для управления состоянием формы (ОБЯЗАТЕЛЬНО ДОЛЖНЫ БЫТЬ ТУТ)
            private bool isEditMode = false;
            private int selectedUserIdForEdit = -1; // Твоя ошибка CS0103 пропадет, так как переменная теперь на месте

            public AdminWindow(int id, string name)
            {
                InitializeComponent();
                this.currentAdminId = id;
                this.currentAdminName = name;

                AdminInfoTextBlock.Text = $"Панель администратора: {currentAdminName} (ID: {currentAdminId})";

                // При открытии окна сразу загружаем данные
                LoadUsersData();
                LoadRolesToComboBox();
            }

            // Вспомогательный класс-модель для вывода в DataGrid
            public class UserRow
            {
                public int Id { get; set; }
                public string Login { get; set; }
                public string Password { get; set; }
                public int RoleId { get; set; }
                public string RoleName { get; set; }
            }

            // Вспомогательный класс для ролей в Комбобоксе
            public class RoleItem
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            #region Загрузка данных из БД

            private void LoadUsersData()
            {
                List<UserRow> users = new List<UserRow>();
                string query = @"
                    SELECT u.id, u.login, u.password, u.role_id, r.name AS role_name 
                    FROM users u
                    JOIN roles r ON u.role_id = r.id";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            connection.Open();
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    users.Add(new UserRow
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Login = reader["login"].ToString(),
                                        Password = reader["password"].ToString(),
                                        RoleId = Convert.ToInt32(reader["role_id"]),
                                        RoleName = reader["role_name"].ToString()
                                    });
                                }
                            }
                        }
                    }
                    UsersDataGrid.ItemsSource = users;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
                }
            }

            private void LoadRolesToComboBox()
            {
                List<RoleItem> roles = new List<RoleItem>();
                string query = "SELECT id, name FROM roles";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            connection.Open();
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    roles.Add(new RoleItem
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Name = reader["name"].ToString()
                                    });
                                }
                            }
                        }
                    }
                    formRoleComboBox.ItemsSource = roles;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
                }
            }

            #endregion

            #region Логика кнопок Главного Экрана

            private void CreateUserBTN_Click(object sender, RoutedEventArgs e)
            {
                isEditMode = false;
                FormTitleTextBlock.Text = "Создание нового пользователя";

                formLoginTextBox.Text = "";
                formPasswordTextBox.Text = "";
                formRoleComboBox.SelectedIndex = -1;

                mainGrid.Visibility = Visibility.Collapsed;
                formGrid.Visibility = Visibility.Visible;
            }

            private void EditUserBTN_Click(object sender, RoutedEventArgs e)
            {
                UserRow selectedUser = UsersDataGrid.SelectedItem as UserRow;

                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите пользователя из таблицы для редактирования!");
                    return;
                }

                isEditMode = true;
                FormTitleTextBlock.Text = $"Редактирование пользователя [ID: {selectedUser.Id}]";
                selectedUserIdForEdit = selectedUser.Id;

                formLoginTextBox.Text = selectedUser.Login;
                formPasswordTextBox.Text = selectedUser.Password;
                formRoleComboBox.SelectedValue = selectedUser.RoleId;

                mainGrid.Visibility = Visibility.Collapsed;
                formGrid.Visibility = Visibility.Visible;
            }

            private void DeleteUserBTN_Click(object sender, RoutedEventArgs e)
            {
                UserRow selectedUser = UsersDataGrid.SelectedItem as UserRow;

                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите пользователя из таблицы для удаления!");
                    return;
                }

                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите полностью удалить пользователя '{selectedUser.Login}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    string query = "DELETE FROM users WHERE id = @id";

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@id", selectedUser.Id);
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                        }
                        MessageBox.Show("Пользователь успешно удален.");
                        LoadUsersData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                    }
                }
            }

            #endregion

            #region Логика кнопок Блока Формы

            private void SaveFormBTN_Click(object sender, RoutedEventArgs e)
            {
                if (string.IsNullOrWhiteSpace(formLoginTextBox.Text) ||
                    string.IsNullOrWhiteSpace(formPasswordTextBox.Text) ||
                    formRoleComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля формы и выберите роль!");
                    return;
                }

                string login = formLoginTextBox.Text.Trim();
                string password = formPasswordTextBox.Text.Trim();
                int roleId = Convert.ToInt32(formRoleComboBox.SelectedValue);

                string query = "";

                if (isEditMode)
                {
                    query = "UPDATE users SET login = @login, password = @password, role_id = @roleId WHERE id = @id";
                }
                else
                {
                    query = "INSERT INTO users (login, password, role_id) VALUES (@login, @password, @roleId)";
                }

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@login", login);
                            command.Parameters.AddWithValue("@password", password);
                            command.Parameters.AddWithValue("@roleId", roleId);

                            if (isEditMode)
                            {
                                command.Parameters.AddWithValue("@id", selectedUserIdForEdit);
                            }

                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show(isEditMode ? "Данные успешно обновлены!" : "Пользователь успешно создан!");

                    formGrid.Visibility = Visibility.Collapsed;
                    mainGrid.Visibility = Visibility.Visible;

                    LoadUsersData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения данных: {ex.Message}");
                }
            }

            private void CancelFormBTN_Click(object sender, RoutedEventArgs e)
            {
                formGrid.Visibility = Visibility.Collapsed;
                mainGrid.Visibility = Visibility.Visible;
            }

            #endregion
        }
    }