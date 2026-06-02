### Помощь с стилями
#### подключение БД app config
	<connectionStrings>
		<add name="test_DB"
			 connectionString="                   "
			 providerName="System.Data.SqlClient"/>
	</connectionStrings>
потом везде в коде: readonly string connectionString = ConfigurationManager.ConnectionStrings["test_DB"].ConnectionString;
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

                    <Button x:Name="VerifyButton" Grid.Row="3" Content="Проверить" Margin="0,20,0,0"/>
                    <TextBlock x:Name="captchaStatusTextBlock" Grid.Row="4" Text="У вас осталось столько попыток" Margin="0,20,0,0"/>
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

        public MainWindow()
        {
            InitializeComponent();
            // Важно: инициализируем капчу ТОЛЬКО после полной загрузки окна,
            // когда элементы гарантированно построили свои размеры в интерфейсе
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
                var mainImage = LoadSampleImage();
                MainImage.Source = mainImage;

                CreatePuzzlePiece(mainImage);

                PuzzleSlider.Value = 0;
                captchaStatusTextBlock.Text = "Передвиньте пазл на нужное место.";
                captchaStatusTextBlock.Foreground = Brushes.Black;

                // Включаем элементы управления обратно при новой генерации
                PuzzleSlider.IsEnabled = true;
                VerifyButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации капчи: {ex.Message}");
            }
        }

        private BitmapImage LoadSampleImage()
        {
            // Размеры генерируемого изображения теперь строго 350x220
            int width = (int)CaptchaWidth;
            int height = (int)CaptchaHeight;

            var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.LightBlue, null, new Rect(0, 0, width, height));
                drawingContext.DrawEllipse(Brushes.Red, null, new Point(width * 0.3, height * 0.4), 30, 30);
                drawingContext.DrawRectangle(Brushes.Green, null, new Rect(width * 0.6, height * 0.3, 60, 40));
                drawingContext.DrawLine(new Pen(Brushes.Blue, 3),
                    new Point(width * 0.2, height * 0.7),
                    new Point(width * 0.8, height * 0.7));
            }

            renderTarget.Render(drawingVisual);

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

            // ВАЖНО: Вырезаем строго там же, где будет рамка (координата targetPosition)
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

            // Плавно двигаем пазл в зависимости от положения слайдера (от 0 до 100)
            double newX = minX + (PuzzleSlider.Value / 100.0) * (maxX - minX);
            puzzleTransform.X = newX;
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            double currentPosition = puzzleTransform.X;

            // Проверяем, попал ли пользователь в область с учетом погрешности tolerance
            if (Math.Abs(currentPosition - targetPosition) <= tolerance)
            {
                captchaStatusTextBlock.Text = "✓ Капча пройдена успешно!";
                captchaStatusTextBlock.Foreground = Brushes.Green;

                PuzzleSlider.IsEnabled = false;
                VerifyButton.IsEnabled = false;
            }
            else
            {
                captchaStatusTextBlock.Text = "✗ Неверно. Попробуйте еще раз.";
                captchaStatusTextBlock.Foreground = Brushes.Red;

                // Автоматический сброс для новой попытки
                InitializeCaptcha();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            // Метод оставлен, но теперь он не сломает логику, так как размеры фиксированы константами
            if (IsLoaded)
            {
                InitializeCaptcha();
            }
        }
    }
}
```

