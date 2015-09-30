<?xml version="1.0" encoding="utf-8"?>
<Grid IsVisible="{Binding ShowAddAction}"
  xmlns="www.team-mediaportal.com/2008/mpf/directx"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- Model = MediaNavigationModel -->
    <Grid.Resources>
        <Model x:Key="MediaModel" Id="CC11183C-01A9-4F96-AF90-FAA046981006"/>
    </Grid.Resources>
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

        <ListView Context="{Model Id=CC11183C-01A9-4F96-AF90-FAA046981006}" Style="{StaticResource ContentsMenuListViewStyle}" ItemsSource="{Binding Path=ActionItems,Mode=OneTime}" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" IsVisible="true">
            <ListView.Resources>
                <!--<Command x:Key="Menu_Command" Source="{StaticResource MediaModel}" Path="Select"
    Parameters="{LateBoundValue BindingValue={Binding}}"/>-->
                <CommandBridge x:Key="Menu_Command" Command="{Binding Path=Command,Mode=OneTime}" />
            </ListView.Resources>
        </ListView>
    </Grid>
</Grid>