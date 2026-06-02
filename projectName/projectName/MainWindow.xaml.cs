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

namespace projectName
{
    public partial class MainWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["nameDB"].ConnectionString;
        //---------------------------------ВСЁ ЧТО СВЯЗАННО С КАПТЧЕЙ
        private readonly Random random = new Random();
        private int attemptsLeft = 3;
        private readonly string[] ImageResources = new string[]
        {
            "pic/1.png", "pic/2.png", "pic/3.png", "pic/4.png"
        };
        private Image draggingPiece = null;
        private Point startMousePosition;
        private double startTranslationX;
        private double startTranslationY;
        private readonly List<Image> puzzlePieces = new List<Image>();
        private Border[] sourceBlocks;
        //---------------------------------ВСЁ ЧТО СВЯЗАННО С КАПТЧЕЙ

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            captchaGrid.Visibility = Visibility.Visible;
            authorizationGrid.Visibility = Visibility.Collapsed;
        }
        
        //---------------------------------ВСЁ ЧТО СВЯЗАННО С КАПТЧЕЙ
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            sourceBlocks = new Border[] { SourceBlock0, SourceBlock1, SourceBlock2, SourceBlock3 };
            InitializeGame();
        }
        private void InitializeGame()
        {
            foreach (var block in sourceBlocks) block.Child = null;
            foreach (Border zone in TargetGrid.Children) zone.Child = null;
            puzzlePieces.Clear();

            try
            {
                string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                List<int> blockIndices = new List<int> { 0, 1, 2, 3 };
                ShuffleList(blockIndices);

                for (int i = 0; i < ImageResources.Length; i++)
                {
                    string fullPackUri = $"pack://application:,,,/{assemblyName};component/{ImageResources[i]}";

                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(fullPackUri, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();

                    TranslateTransform transform = new TranslateTransform();

                    Image img = new Image
                    {
                        Source = bmp,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Stretch = Stretch.Fill,
                        Tag = i,
                        Cursor = Cursors.Hand,
                        RenderTransform = transform
                    };

                    img.MouseLeftButtonDown += Piece_MouseLeftButtonDown;
                    img.MouseMove += Piece_MouseMove;
                    img.MouseLeftButtonUp += Piece_MouseLeftButtonUp;

                    puzzlePieces.Add(img);
                    sourceBlocks[blockIndices[i]].Child = img;
                }

                StatusTextBlock.Text = $"Осталось попыток: {attemptsLeft}";
                StatusTextBlock.Foreground = Brushes.Black;
                VerifyButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}");
            }
        }
        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        #region Drag & Drop
        private void Piece_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggingPiece = sender as Image;
            if (draggingPiece != null)
            {
                draggingPiece.CaptureMouse();
                startMousePosition = e.GetPosition(this);

                var transform = draggingPiece.RenderTransform as TranslateTransform;
                if (transform != null)
                {
                    startTranslationX = transform.X;
                    startTranslationY = transform.Y;
                }
            }
        }
        private void Piece_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingPiece != null && draggingPiece.IsMouseCaptured)
            {
                Point currentMousePosition = e.GetPosition(this);

                double deltaX = currentMousePosition.X - startMousePosition.X;
                double deltaY = currentMousePosition.Y - startMousePosition.Y;

                var transform = draggingPiece.RenderTransform as TranslateTransform;
                if (transform != null)
                {
                    transform.X = startTranslationX + deltaX;
                    transform.Y = startTranslationY + deltaY;
                }
            }
        }
        private void Piece_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggingPiece != null)
            {
                draggingPiece.ReleaseMouseCapture();
                var transform = draggingPiece.RenderTransform as TranslateTransform;

                if (transform != null)
                {
                    Point pieceCenterInWindow = draggingPiece.TranslatePoint(new Point(draggingPiece.ActualWidth / 2, draggingPiece.ActualHeight / 2), this);
                    bool snapped = false;

                    foreach (Border zone in TargetGrid.Children)
                    {
                        Point zonePosInWindow = zone.TranslatePoint(new Point(0, 0), this);
                        Rect zoneRect = new Rect(zonePosInWindow.X, zonePosInWindow.Y, zone.ActualWidth, zone.ActualHeight);

                        if (zoneRect.Contains(pieceCenterInWindow))
                        {
                            if (zone.Child != null && zone.Child != draggingPiece)
                                break;

                            Border parentBorder = VisualTreeHelper.GetParent(draggingPiece) as Border;
                            if (parentBorder != null) parentBorder.Child = null;

                            zone.Child = draggingPiece;

                            transform.X = 0;
                            transform.Y = 0;

                            snapped = true;
                            break;
                        }
                    }

                    if (!snapped)
                    {
                        bool returnedToSource = false;
                        foreach (var block in sourceBlocks)
                        {
                            Point blockPos = block.TranslatePoint(new Point(0, 0), this);
                            Rect blockRect = new Rect(blockPos.X, blockPos.Y, block.ActualWidth, block.ActualHeight);

                            if (blockRect.Contains(pieceCenterInWindow) && block.Child == null)
                            {
                                Border parentBorder = VisualTreeHelper.GetParent(draggingPiece) as Border;
                                if (parentBorder != null) parentBorder.Child = null;

                                block.Child = draggingPiece;
                                transform.X = 0;
                                transform.Y = 0;
                                returnedToSource = true;
                                break;
                            }
                        }

                        if (!returnedToSource)
                        {
                            transform.X = 0;
                            transform.Y = 0;
                        }
                    }
                }
                draggingPiece = null;
            }
        }
        #endregion
        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            int correctCount = 0;

            foreach (var piece in puzzlePieces)
            {
                int correctIndex = (int)piece.Tag;
                Border targetZone = TargetGrid.Children[correctIndex] as Border;

                if (targetZone.Child == piece)
                {
                    correctCount++;
                }
            }

            if (correctCount == 4)
            {
                StatusTextBlock.Text = "Авторизация успешна!";
                StatusTextBlock.Foreground = Brushes.Green;
                VerifyButton.IsEnabled = false;

                foreach (var piece in puzzlePieces) piece.IsEnabled = false;

                captchaGrid.Visibility = Visibility.Collapsed;
                authorizationGrid.Visibility = Visibility.Visible;
            }
            else
            {
                attemptsLeft--;
                if (attemptsLeft > 0)
                {
                    StatusTextBlock.Text = $"Неверно собран пазл. Осталось попыток: {attemptsLeft}";
                    StatusTextBlock.Foreground = Brushes.Red;
                    InitializeGame();
                }
                else
                {
                    StatusTextBlock.Text = "Доступ заблокирован!";
                    StatusTextBlock.Foreground = Brushes.DarkRed;
                    VerifyButton.IsEnabled = false;
                    foreach (var piece in puzzlePieces) piece.IsEnabled = false;
                }
            }
        }

        //---------------------------------ВСЁ ЧТО СВЯЗАННО С КАПТЧЕЙ





        //---------------------------------ВСЁ ЧТО СВЯЗАННО С ЛОГИНОМ
        // Добавили ключевое слово async
        private async void loginBTN_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка на то, пусты ли поля
            if (string.IsNullOrWhiteSpace(loginTextBox.Text) || string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка", MessageBoxButton.OK);
                return;
            }

            string login = loginTextBox.Text.Trim();
            string password = passwordBox.Password.Trim();

            // Отключаем кнопку, чтобы пользователь не нажал её 10 раз, пока идет проверка
            (sender as Button).IsEnabled = false;

            string query = @"
        SELECT u.id, u.login, r.name 
        FROM users u
        JOIN roles r ON u.role_id = r.id
        WHERE u.login = @login AND u.password = @password";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@password", password);

                        // Ждем открытия подключения асинхронно, не вешая интерфейс
                        await connection.OpenAsync();

                        // Ждем чтения данных асинхронно
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int userId = Convert.ToInt32(reader["id"]);
                                string userName = reader["login"].ToString();
                                string roleName = reader["name"].ToString();

                                MessageBox.Show($"Добро пожаловать, {userName}! Ваша роль: {roleName}", "Успех", MessageBoxButton.OK);

                                if (roleName == "Администратор")
                                {
                                    admin.AdminWindow adminWin = new admin.AdminWindow(userId, userName);
                                    adminWin.Show();
                                    this.Close();
                                }
                                else if (roleName == "Пользователь")
                                {
                                    user.UserWindow userWin = new user.UserWindow(userId, userName);
                                    userWin.Show();
                                    this.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK);
                                // Включаем кнопку обратно, если данные неверны
                                (sender as Button).IsEnabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка БД", MessageBoxButton.OK);
                // Включаем кнопку обратно, если упала ошибка соединения
                (sender as Button).IsEnabled = true;
            }
        }
    }
}