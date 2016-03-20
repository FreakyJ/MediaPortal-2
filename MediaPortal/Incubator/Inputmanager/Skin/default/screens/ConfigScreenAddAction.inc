<?xml version="1.0" encoding="utf-8"?>
<Grid IsVisible="{Binding ShowAddAction}"
  xmlns="www.team-mediaportal.com/2008/mpf/directx"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <Grid.RowDefinitions>
    <RowDefinition Height="*"/>
  </Grid.RowDefinitions>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*"/>
  </Grid.ColumnDefinitions>
  <Grid HorizontalAlignment="Stretch" VerticalAlignment="Stretch" IsVisible="true">
    <Grid.RowDefinitions>
      <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <ListView Style="{StaticResource ContentsMenuListViewStyle}" ItemsSource="{Binding Path=ActionItems,Mode=OneTime}" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" IsVisible="true">
      <ListView.Resources>
        <!--<Command x:Key="Menu_Command" Source="{StaticResource MediaModel}" Path="Select"
    Parameters="{LateBoundValue BindingValue={Binding}}"/>-->
        <CommandBridge x:Key="Menu_Command" Command="{Binding Path=Command,Mode=OneTime}" />
      </ListView.Resources>
    </ListView>
  </Grid>
</Grid>