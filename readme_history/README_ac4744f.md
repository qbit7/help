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
