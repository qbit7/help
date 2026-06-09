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
			<Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>

            Consolas
###### кнопка
```
<Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="10">
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
```
### Создание каптчи
#### Основа для неё

