### Помощь с БД
#### Пример создяние Таблиц для БД с связями
```

-- 1. Таблица филиалов
CREATE TABLE branches (
    id INT IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    city VARCHAR(50) NOT NULL,
    address VARCHAR(255) NOT NULL
);

-- 2. Таблица комнат (Лвл 3: Составной первичный ключ)
CREATE TABLE rooms (
    branch_id INT,
    room_no INT,
    room_type VARCHAR(50) NOT NULL, -- 'Переговорная', 'Кабинет', 'Open-space'
    capacity INT NOT NULL,
    price_per_hour DECIMAL(10, 2) NOT NULL,
    
    -- Составной ПК: номер комнаты уникален только внутри конкретного филиала
    PRIMARY KEY (branch_id, room_no),
    -- Связь с филиалом: если закрывается филиал, удаляются и его комнаты
    FOREIGN KEY (branch_id) REFERENCES branches(id) ON DELETE CASCADE
);

-- 3. Таблица резидентов (Лвл 4: Иерархия, таблица ссылается сама на себя)
CREATE TABLE members (
    id INT IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    phone VARCHAR(20),
    registration_date DATE NOT NULL,
    invited_by_id INT, -- ID резидента, который пригласил данного человека
    
    -- Рекурсивный внешний ключ: ссылается на id в этой же таблице.
    -- Если пригласившего удалят, у приглашенного поле просто станет NULL.
    FOREIGN KEY (invited_by_id) REFERENCES members(id) ON DELETE NO ACTION);

-- 4. Таблица бронирований (Связь с резидентом и составная связь с комнатой)
CREATE TABLE bookings (
    id INT IDENTITY PRIMARY KEY,
    member_id INT NOT NULL,
    branch_id INT NOT NULL,
    room_no INT NOT NULL,
    booking_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    
    -- Запрещаем удалять резидента, если у него есть бронирования
    FOREIGN KEY (member_id)
REFERENCES members(id)
ON DELETE NO ACTION,
    -- Составной внешний ключ: ссылается сразу на два поля таблицы rooms!
    FOREIGN KEY (branch_id, room_no) REFERENCES rooms(branch_id, room_no) ON DELETE CASCADE
);

-- 5. Таблица дополнительных услуг
CREATE TABLE services (
    id INT IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL
);

-- 6. Дополнительные услуги в бронировании (Связь Многие-ко-многим)
CREATE TABLE booking_services (
    booking_id INT,
    service_id INT,
    quantity INT NOT NULL DEFAULT 1,
    
    PRIMARY KEY (booking_id, service_id),
    -- Каскадно удаляем услуги из бронирования, если удалено само бронирование или услуга
    FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE CASCADE,
    FOREIGN KEY (service_id) REFERENCES services(id) ON DELETE CASCADE
);
```

### Помощь с стилями
#### подключение БД app config
```
	<connectionStrings>
		<add name="test_DB"
			 connectionString="                   "
			 providerName="System.Data.SqlClient"/>
	</connectionStrings>
```
Также и установить библиотеки:
```
EntityFramework
```
```
System.Configuration.ConfigurationManager
```
потом везде в коде для вызова БД: 
```
readonly string connectionString = ConfigurationManager.ConnectionStrings["test_DB"].ConnectionString;
```

#### стили
##### общие
```
<Style TargetType="StackPanel">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
        </Style>
        <Style TargetType="TextBlock">
            <Setter Property="FontSize" Value="16"/>
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="FontFamily" Value="Consolas"/>
        </Style>
        <Style TargetType="Button">
            <Setter Property="FontSize" Value="16"/>
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="FontFamily" Value="Consolas"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}"
                        CornerRadius="5" Padding="10,5">
                            <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center"/>
                        </Border>

                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter Property="Opacity" Value="0.85"/>
                            </Trigger>

                            <Trigger Property="IsPressed" Value="True">
                                <Setter Property="Opacity" Value="0.7"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
<Style TargetType="TextBox">
    <Setter Property="FontSize" Value="16"/>
    <Setter Property="FontFamily" Value="Consolas"/>
    <Setter Property="Padding" Value="10,5"/>
    <Setter Property="Background" Value="White"/>
    <Setter Property="BorderBrush" Value="#CCCCCC"/>
    <Setter Property="BorderThickness" Value="1"/>

    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="TextBox">
                <Border x:Name="Border"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="5">

                    <ScrollViewer x:Name="PART_ContentHost"/>
                </Border>

                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="Border"
                                Property="BorderBrush"
                                Value="#999999"/>
                    </Trigger>

                    <Trigger Property="IsKeyboardFocused" Value="True">
                        <Setter TargetName="Border"
                                Property="BorderBrush"
                                Value="#4A90E2"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### Создание каптчи
#### Основа для неё
```
Title="Авторизация" 
        WindowStartupLocation="CenterScreen"
        WindowState="Maximized"
        Height="1080" 
        MinHeight="650"
        Width="1920"
        MinWidth="450"
```

```
<Grid x:Name="captchaGrid" Visibility="Visible">
            
            <StackPanel Height="500">
                <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>
        
                <TextBlock Text="Чтобы продолжить, Вам необходимо пройти каптчу" TextWrapping="Wrap" FontSize="19"
                           Grid.Row="0" Margin="0,20,0,15"/>

                <Border Background="Red" Grid.Row="1" Padding="10"
                        HorizontalAlignment="Center"
                        VerticalAlignment="Center"
                        CornerRadius="10">
                        <StackPanel Height="220" Width="350">

                            <Grid x:Name="cptchaImageBoxGrid">
                                <Image Name="MainImage" Stretch="Fill"/>

                                <Canvas Name="PuzzleCanvas">
                                    <Image Name="PuzzlePiece" RenderTransformOrigin="0.5,0.5">
                                        <Image.RenderTransform>
                                            <TranslateTransform x:Name="puzzleTransform"/>
                                        </Image.RenderTransform>
                                    </Image>
                                </Canvas>
                            </Grid>
                        </StackPanel>    
                    </Border>
                <StackPanel Grid.Row="2" Margin="0,20,0,0">
                    <TextBlock Text="Передвиньте пазл:" FontSize="14" Margin="0,0,0,5"/>
                    <Slider x:Name="PuzzleSlider" Minimum="0" Maximum="100" Value="0"
                            ValueChanged="PuzzleSlider_ValueChanged"/>
                </StackPanel>

                    <Button x:Name="VerifyButton" Grid.Row="3" Content="Проверить" Margin="0,20,0,0" Click="VerifyButton_Click_1"/>
                    <TextBlock x:Name="captchaStatusTextBlock" Grid.Row="4" Foreground="Red" TextWrapping="Wrap" Margin="0,20,0,0"/>
                </Grid>
            </StackPanel>
        </Grid>
```
```
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace
{
    public partial class MainWindow : Window
    {
        private double targetPosition;
        private double tolerance = 5.0;
        private Random random = new Random();

        // Фиксированные размеры, привязанные к вашей разметке
        private const double CaptchaWidth = 350;
        private const double CaptchaHeight = 220;
        private const double PuzzleSize = 60;

        private int attemptsLeft = 3; // Количество попыток для прохождения

        public MainWindow()
        {
            InitializeComponent();

            // Инициализируем капчу ТОЛЬКО после полной загрузки окна
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCaptcha();
        }

        private void InitializeCaptcha()
        {
            try
            {
                // Теперь загружается картинка с 3 новыми фигурами
                var mainImage = CreateSampleImage();
                MainImage.Source = mainImage;

                CreatePuzzlePiece(mainImage);

                PuzzleSlider.Value = 0;

                // Включаем элементы управления обратно при новой генерации
                PuzzleSlider.IsEnabled = true;
                VerifyButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации капчи: {ex.Message}");
            }
        }

        private BitmapImage CreateSampleImage()
        {
            int width = (int)CaptchaWidth;
            int height = (int)CaptchaHeight;

            var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                // 1. Рисуем красивый светлый фон
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromRgb(240, 248, 255)), null, new Rect(0, 0, width, height));

                // ==========================================
                // ФИГУРА 1: КРУГ (Красный)
                // ==========================================
                var circleCenter = new Point(width * 0.25, height * 0.4); // Левая часть экрана
                drawingContext.DrawEllipse(Brushes.Crimson, null, circleCenter, 35, 35);

                // ==========================================
                // ФИГУРА 2: ТРЕУГОЛЬНИК (Синий)
                // ==========================================
                var triangleGeometry = new StreamGeometry();
                using (var ctx = triangleGeometry.Open())
                {
                    // Задаем вершины треугольника (Центральная верхняя часть экрана)
                    ctx.BeginFigure(new Point(width * 0.5, height * 0.2), true /* заполнено */, true /* замкнуто */);
                    ctx.LineTo(new Point(width * 0.4, height * 0.6), true, false);
                    ctx.LineTo(new Point(width * 0.6, height * 0.6), true, false);
                }
                triangleGeometry.Freeze();
                drawingContext.DrawGeometry(Brushes.RoyalBlue, null, triangleGeometry);

                // ==========================================
                // ФИГУРА 3: ШЕСТИУГОЛЬНИК (Зеленый)
                // ==========================================
                var hexagonGeometry = new StreamGeometry();
                double hexCenterX = width * 0.75; // Правая часть экрана
                double hexCenterY = height * 0.4;
                double radius = 40; // Размер шестиугольника

                using (var ctx = hexagonGeometry.Open())
                {
                    // Вычисляем 6 точек по окружности (шаг 60 градусов)
                    ctx.BeginFigure(new Point(hexCenterX + radius, hexCenterY), true, true);
                    for (int i = 1; i < 6; i++)
                    {
                        double angle = i * Math.PI / 3;
                        ctx.LineTo(new Point(hexCenterX + radius * Math.Cos(angle), hexCenterY + radius * Math.Sin(angle)), true, false);
                    }
                }
                hexagonGeometry.Freeze();
                drawingContext.DrawGeometry(Brushes.ForestGreen, null, hexagonGeometry);
            }

            renderTarget.Render(drawingVisual);

            // Конвертируем в BitmapImage для корректного отображения и вырезания
            var bitmap = new BitmapImage();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            return bitmap;
        }

        private void CreatePuzzlePiece(BitmapImage mainImage)
        {
            double maxX = CaptchaWidth - PuzzleSize - 20;
            double maxY = CaptchaHeight - PuzzleSize - 20;

            double startX = 20;
            double startY = random.Next(20, (int)maxY);

            // Цель генерируем так, чтобы она не вылезала за границы
            targetPosition = random.Next((int)(PuzzleSize + 50), (int)maxX);

            // Вырезаем строго там же, где будет рамка (координата targetPosition)
            var puzzleBitmap = new CroppedBitmap(mainImage,
                new Int32Rect((int)targetPosition, (int)startY, (int)PuzzleSize, (int)PuzzleSize));

            PuzzlePiece.Source = puzzleBitmap;
            PuzzlePiece.Width = PuzzleSize;
            PuzzlePiece.Height = PuzzleSize;

            puzzleTransform.X = startX;
            puzzleTransform.Y = startY;

            // Рисуем рамку И замазываем дыру под ней
            DrawPuzzleOutlineAndHole(targetPosition, startY, PuzzleSize);
        }

        private void DrawPuzzleOutlineAndHole(double x, double y, double size)
        {
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                // 1. Рисуем серый квадрат (дыру) на месте вырезанного пазла
                drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(x, y, size, size));

                // 2. Рисуем красную рамку вокруг этой дыры
                var pen = new Pen(Brushes.Red, 2);
                drawingContext.DrawRectangle(null, pen, new Rect(x, y, size, size));
            }

            var renderTarget = new RenderTargetBitmap((int)CaptchaWidth, (int)CaptchaHeight, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);

            var combinedVisual = new DrawingVisual();
            using (var drawingContext = combinedVisual.RenderOpen())
            {
                drawingContext.DrawImage(MainImage.Source, new Rect(0, 0, CaptchaWidth, CaptchaHeight));
                drawingContext.DrawImage(renderTarget, new Rect(0, 0, CaptchaWidth, CaptchaHeight));
            }

            var finalRender = new RenderTargetBitmap((int)CaptchaWidth, (int)CaptchaHeight, 96, 96, PixelFormats.Pbgra32);
            finalRender.Render(combinedVisual);

            MainImage.Source = finalRender;
        }

        private void PuzzleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PuzzlePiece == null || puzzleTransform == null) return;

            double minX = 20;
            double maxX = CaptchaWidth - PuzzlePiece.Width - 20;

            // Плавно двигаем пазл в зависимости от положения слайдера
            double newX = minX + (PuzzleSlider.Value / 100.0) * (maxX - minX);
            puzzleTransform.X = newX;
        }

        // Единственный и правильный метод обработки клика
        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            double currentPosition = puzzleTransform.X;

            if (Math.Abs(currentPosition - targetPosition) <= tolerance)
            {
                // УСПЕХ
                captchaStatusTextBlock.Text = "✓ Капча пройдена успешно! Доступ разрешен.";
                captchaStatusTextBlock.Foreground = Brushes.Green;

                PuzzleSlider.IsEnabled = false;
                VerifyButton.IsEnabled = false;
            }
            else
            {
                // ОШИБКА
                attemptsLeft--;

                if (attemptsLeft > 0)
                {
                    captchaStatusTextBlock.Text = $"Неверно! Осталось попыток: {attemptsLeft}. Капча обновлена.";
                    captchaStatusTextBlock.Foreground = Brushes.Red;

                    InitializeCaptcha();
                }
                else
                {
                    // ПОПЫТКИ ЗАКОНЧИЛИСЬ
                    captchaStatusTextBlock.Text = "Доступ заблокирован! Слишком много неудачных попыток.\nЧтобы повторить, откройте приложение заново.";
                    captchaStatusTextBlock.Foreground = Brushes.Red;

                    PuzzleSlider.IsEnabled = false;
                    VerifyButton.IsEnabled = false;
                }
            }
        }
    }
}
```

### Пример работы с БД в коде
#### создание таблицы xaml
```
<DataGrid x:Name="usersDB"
          AutoGenerateColumns="False"
          CanUserAddRows="False">

    <DataGrid.Columns>

        <DataGridTextColumn Header="ID"
                            Binding="{Binding id}"
                            Width="70"/>

        <DataGridTextColumn Header="Роль"
                            Binding="{Binding role_name}"
                            Width="150"/>

        <DataGridTextColumn Header="Логин"
                            Binding="{Binding login}"
                            Width="150"/>

        <DataGridTextColumn Header="ФИО"
                            Binding="{Binding full_name}"
                            Width="250"/>

        <DataGridTextColumn Header="Email"
                            Binding="{Binding email}"
                            Width="200"/>

        <DataGridTextColumn Header="Телефон"
                            Binding="{Binding phone}"
                            Width="150"/>

        <DataGridTextColumn Header="Статус"
                            Binding="{Binding is_active}"
                            Width="120"/>

        <DataGridTextColumn Header="Дата создания"
                            Binding="{Binding created_at}"
                            Width="180"/>

    </DataGrid.Columns>

</DataGrid>
```
#### заполнение созданной таблицы
```
        private void loadUsersData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string getUsersDataQ = @"SELECT 
                                    u.id, 
                                    r.name AS role_name,
                                    u.login, 
                                    u.full_name, 
                                    u.email, 
                                    u.phone, 
                                    CASE 
                                        WHEN u.is_active = 1 THEN 'Активен' 
                                        ELSE 'Не активен' 
                                    END AS is_active, 
                                    u.created_at 
                                FROM users u
                                INNER JOIN roles r ON u.role_id = r.id;";

                SqlDataAdapter adapter = new SqlDataAdapter(getUsersDataQ, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                usersDB.ItemsSource = table.DefaultView;
            }
        }
```
### пример получения выюранной строки
```
private void userEditingBTN_Click(object sender, RoutedEventArgs e)
        {
            if (usersDB.SelectedItem == null)
            {
                сообщение об ошибке так как ничего не выбрал
                return;
            }
			считывание и запись
			DataRowView row = (DataRowView)usersDB.SelectedItem;
            int selectedUserIdFromDBGrid = Convert.ToInt32(row["id"]);
            string selectedUserLoginFromDBGrid = row["login"]?.ToString() ?? "";
```

#### пример авторизации
```
private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox1.Text;
            string password = Password.Password;

			using(SqlConnection conn = new SqlConnection(connectionString))
            {

                conn.Open();

                string getLoginQ = @"SELECT Login, Password, Role 
                                    FROM users
                                    where Login = @login";
                SqlCommand cmd = new SqlCommand(getLoginQ, conn);
                cmd.Parameters.AddWithValue("@login", login);
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    ShowError("Пользователь не найден");
                    return;
                }
                else
                {
                    string Login = reader["Login"].ToString();
                    string Password = reader["Password"].ToString();
                    string Role = reader["Role"].ToString();
                    if (Password != password)
                    {
                        ShowError("Пароль неверный");
                        return;
                    }
                    else
                    {
                        switch (Role)
                        {
                            case "Администратор":
                                adminWorkSpace adminWorkSpace = new adminWorkSpace(Login, Password, Role, connectionString);
                                adminWorkSpace.Show();
                                this.Close();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
		}
```

### Каптча с картинками 4шт
красота
```
<Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <TextBlock Text="Соберите картинку, перетащив кусочки на правильные места" 
                   FontSize="18" FontWeight="Bold" Margin="0,0,0,20" HorizontalAlignment="Center"/>

        <Canvas x:Name="MainCanvas" Grid.Row="1" Background="#F9F9F9" ClipToBounds="True">

            <Canvas Canvas.Left="20" Canvas.Top="20" Width="320" Height="320">
                <Rectangle Width="320" Height="320" Stroke="#B0B0B0" StrokeThickness="2" 
                           StrokeDashArray="4 4" RadiusX="5" RadiusY="5"/>
                <TextBlock Text="Перемешанные кусочки" Width="320" Height="320"
                           VerticalAlignment="Center" HorizontalAlignment="Center" 
                           TextAlignment="Center" LineHeight="320" Foreground="#888888"/>
            </Canvas>

            <Border Canvas.Left="420" Canvas.Top="20" Width="320" Height="320" 
                    BorderBrush="Crimson" BorderThickness="2" CornerRadius="5" Background="White">
                <UniformGrid Rows="2" Columns="2" x:Name="TargetGrid">
                    <Border BorderBrush="#E0E0E0" BorderThickness="1" Tag="0"/>
                    <Border BorderBrush="#E0E0E0" BorderThickness="1" Tag="1"/>
                    <Border BorderBrush="#E0E0E0" BorderThickness="1" Tag="2"/>
                    <Border BorderBrush="#E0E0E0" BorderThickness="1" Tag="3"/>
                </UniformGrid>
            </Border>
        </Canvas>

        <Button x:Name="VerifyButton" Grid.Row="2" Content="Проверить" 
                Width="150" Height="35" Margin="0,15,0,0" FontSize="14" Click="VerifyButton_Click"/>

        <TextBlock x:Name="StatusTextBlock" Grid.Row="3" Text="Осталось попыток: 3" 
                   Margin="0,10,0,0" HorizontalAlignment="Center" FontSize="14" FontWeight="SemiBold"/>
    </Grid>
```

лог
```
    public partial class MainWindow : Window
    {
        private readonly Random random = new Random();
        private int attemptsLeft = 3;

        // =========================================================================
        // ЗАГРУЗКА ЧЕРЕЗ COMPONENT (PACK URI)
        // Замени "moroz" ниже на точное название твоего проекта (Assembly Name),
        // если оно отличается. Обычно оно совпадает с namespace.
        // =========================================================================
        private readonly string[] ImageResources = new string[]
        {
    "pic/1.png",
    "pic/2.png",
    "pic/3.png",
    "pic/4.png"
        };

        

        // Переменные для Drag & Drop
        private Image draggingPiece = null;
        private Point clickOffset;

        // Список для хранения созданных элементов Image
        private readonly List<Image> puzzlePieces = new List<Image>();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        // Вместо старого массива с pack:// пишем просто относительные пути к ресурсам внутри проекта
        private void InitializeGame()
        {
            // Очищаем холст от старых картинок при перезапуске
            foreach (var piece in puzzlePieces)
            {
                MainCanvas.Children.Remove(piece);
            }
            puzzlePieces.Clear();

            try
            {
                double partSize = 160;

                // Автоматически получаем имя текущей сборки проекта, чтобы не прописывать его вручную
                string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                // Создаем 4 элемента Image из ресурсов
                for (int i = 0; i < ImageResources.Length; i++)
                {
                    // Формируем правильный Pack URI на лету с реальным именем проекта
                    string fullPackUri = $"pack://application:,,,/{assemblyName};component/{ImageResources[i]}";

                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(fullPackUri, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();

                    Image img = new Image
                    {
                        Source = bmp,
                        Width = partSize,
                        Height = partSize,
                        Tag = i,
                        Cursor = Cursors.Hand
                    };

                    // Подписываем на события мыши для перетаскивания
                    img.MouseLeftButtonDown += Piece_MouseLeftButtonDown;
                    img.MouseMove += Piece_MouseMove;
                    img.MouseLeftButtonUp += Piece_MouseLeftButtonUp;

                    puzzlePieces.Add(img);
                    MainCanvas.Children.Add(img);
                }

                // Перемешиваем и раскидываем кусочки в ЛЕВОЙ зоне
                foreach (var piece in puzzlePieces)
                {
                    double randX = random.Next(25, 175);
                    double randY = random.Next(25, 175);

                    Canvas.SetLeft(piece, randX);
                    Canvas.SetTop(piece, randY);
                    Panel.SetZIndex(piece, 10);
                }

                StatusTextBlock.Text = $"Осталось попыток: {attemptsLeft}";
                StatusTextBlock.Foreground = Brushes.Black;
                VerifyButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ресурсов: {ex.Message}");
            }
        }

        #region Логика перетаскивания мышью (Drag & Drop)

        private void Piece_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggingPiece = sender as Image;
            if (draggingPiece != null)
            {
                clickOffset = e.GetPosition(draggingPiece);
                draggingPiece.CaptureMouse();
                Panel.SetZIndex(draggingPiece, 999);
            }
        }

        private void Piece_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingPiece != null && draggingPiece.IsMouseCaptured)
            {
                Point currentMousePos = e.GetPosition(MainCanvas);

                double newX = currentMousePos.X - clickOffset.X;
                double newY = currentMousePos.Y - clickOffset.Y;

                if (newX < 0) newX = 0;
                if (newY < 0) newY = 0;
                if (newX > MainCanvas.ActualWidth - draggingPiece.Width) newX = MainCanvas.ActualWidth - draggingPiece.Width;
                if (newY > MainCanvas.ActualHeight - draggingPiece.Height) newY = MainCanvas.ActualHeight - draggingPiece.Height;

                Canvas.SetLeft(draggingPiece, newX);
                Canvas.SetTop(draggingPiece, newY);
            }
        }

        private void Piece_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggingPiece != null)
            {
                draggingPiece.ReleaseMouseCapture();
                Panel.SetZIndex(draggingPiece, 10);

                Point pieceCenter = new Point(
                    Canvas.GetLeft(draggingPiece) + draggingPiece.Width / 2,
                    Canvas.GetTop(draggingPiece) + draggingPiece.Height / 2
                );

                foreach (Border zone in TargetGrid.Children)
                {
                    Point zonePos = zone.TranslatePoint(new Point(0, 0), MainCanvas);
                    Rect zoneRect = new Rect(zonePos.X, zonePos.Y, zone.ActualWidth, zone.ActualHeight);

                    if (zoneRect.Contains(pieceCenter))
                    {
                        Canvas.SetLeft(draggingPiece, zonePos.X);
                        Canvas.SetTop(draggingPiece, zonePos.Y);
                        break;
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

                Point zonePos = targetZone.TranslatePoint(new Point(0, 0), MainCanvas);
                double currentX = Canvas.GetLeft(piece);
                double currentY = Canvas.GetTop(piece);

                if (Math.Abs(currentX - zonePos.X) < 10 && Math.Abs(currentY - zonePos.Y) < 10)
                {
                    correctCount++;
                }
            }

            if (correctCount == 4)
            {
                StatusTextBlock.Text = "✓ Капча успешно пройдена!";
                StatusTextBlock.Foreground = Brushes.Green;
                VerifyButton.IsEnabled = false;

                foreach (var piece in puzzlePieces) piece.IsEnabled = false;
            }
            else
            {
                attemptsLeft--;
                if (attemptsLeft > 0)
                {
                    StatusTextBlock.Text = $"✗ Неверно собран пазл. Осталось попыток: {attemptsLeft}";
                    StatusTextBlock.Foreground = Brushes.Red;
                    InitializeGame();
                }
                else
                {
                    StatusTextBlock.Text = "❌ Доступ заблокирован!";
                    StatusTextBlock.Foreground = Brushes.DarkRed;
                    VerifyButton.IsEnabled = false;
                    foreach (var piece in puzzlePieces) piece.IsEnabled = false;
                }
            }
        }
    }
}
```
