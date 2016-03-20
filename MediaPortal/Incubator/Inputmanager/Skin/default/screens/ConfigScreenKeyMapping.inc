<?xml version="1.0" encoding="utf-8"?>
<Grid IsVisible="{Binding ShowKeyMapping}"
  xmlns="www.team-mediaportal.com/2008/mpf/directx"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <Grid.RowDefinitions>
    <RowDefinition Height="*"/>
    <RowDefinition Height="Auto"/>
  </Grid.RowDefinitions>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*"/>
  </Grid.ColumnDefinitions>

  <ListView Grid.Row="0" Style="{StaticResource ContentsMenuListViewStyle}"
            ItemsSource="{Binding Path=Items,Mode=OneTime}" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" IsVisible="true"
            CurrentItem="{Binding Path=SelectedItem, Mode=TwoWay}">
    <ListView.Resources>
      <!--<Command x:Key="Menu_Command" Source="{StaticResource MediaModel}" Path="Select"
                Parameters="{LateBoundValue BindingValue={Binding}}"/>-->
      <CommandBridge x:Key="Menu_Command" Command="{Binding Path=Command,Mode=OneTime}"/>
    </ListView.Resources>
  </ListView>
  
  <Grid Grid.Row="1" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" IsVisible="true">
    <Grid.RowDefinitions>
      <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*"/>
      <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <Button Grid.Column="0" Content="[InputDeviceManager.KeyMapping.Button.Done]" 
            Style="{ThemeResource ButtonWideStyle}" Margin="10"
            HorizontalAlignment="Left" VerticalAlignment="Center"
            Command="{Command CancelMapping}"/>

    <Button Grid.Column="1"  Content="[InputDeviceManager.KeyMapping.Button.AddMapping]"
            Style="{ThemeResource ButtonWideStyle}" Margin="10"
            HorizontalAlignment="Right" VerticalAlignment="Center"
            Command="{Command AddKeyMapping}"/>
  </Grid>
</Grid>


<!--

<Include
    xmlns="www.team-mediaportal.com/2008/mpf/directx"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Source="screens\master_dialog_bare.xaml"
    >
  <Include.Resources>
        <ControlTemplate x:Key="Contents_Template">
            <DockPanel
          Context="{Model Id=CC11183C-01A9-4F96-AF90-FAA046981006}" LastChildFill="False">
                <ListView x:Name="MediaItemsList" Style="{ThemeResource MediaItemsListListViewStyle}" ItemsSource="{Binding Items}"
                  HorizontalAlignment="Stretch" VerticalAlignment="Stretch" IsVisible="true">
                    <ListView.Resources>
                        <Command x:Key="Menu_Command" Source="{StaticResource MediaModel}" Path="Select"
                        Parameters="{LateBoundValue BindingValue={Binding}}"/>
                    </ListView.Resources>
                </ListView>
            </DockPanel>
        </ControlTemplate>
    </Include.Resources>
</Include>
    -->