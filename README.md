### Помощь с стилями
#### подключение БД app config
	<connectionStrings>
		<add name="test_DB"
			 connectionString="                   "
			 providerName="System.Data.SqlClient"/>
	</connectionStrings>
потом везде в коде: readonly string connectionString = ConfigurationManager.ConnectionStrings["test_DB"].ConnectionString;
#### стили
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>

            Consolas
<Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ScrollViewer">
                <Border CornerRadius="10"
                        BorderBrush="#D6E4EE"
                        BorderThickness="1"
                        Background="White">

                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*" />
                            <ColumnDefinition Width="Auto" />
                        </Grid.ColumnDefinitions>

                        <Grid.RowDefinitions>
                            <RowDefinition Height="*" />
                            <RowDefinition Height="Auto" />
                        </Grid.RowDefinitions>

                        <ScrollContentPresenter
                            x:Name="PART_ScrollContentPresenter"
                            Grid.Row="0"
                            Grid.Column="0" />

                        <ScrollBar
                            x:Name="PART_VerticalScrollBar"
                            Grid.Row="0"
                            Grid.Column="1"
                            Orientation="Vertical"
                            Visibility="{TemplateBinding ComputedVerticalScrollBarVisibility}"
                            Value="{Binding VerticalOffset, RelativeSource={RelativeSource TemplatedParent}}"
                            Maximum="{TemplateBinding ScrollableHeight}"
                            ViewportSize="{TemplateBinding ViewportHeight}" />

                        <ScrollBar
                            x:Name="PART_HorizontalScrollBar"
                            Grid.Row="1"
                            Grid.Column="0"
                            Orientation="Horizontal"
                            Visibility="{TemplateBinding ComputedHorizontalScrollBarVisibility}"
                            Value="{Binding HorizontalOffset, RelativeSource={RelativeSource TemplatedParent}}"
                            Maximum="{TemplateBinding ScrollableWidth}"
                            ViewportSize="{TemplateBinding ViewportWidth}" />
                    </Grid>

                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
