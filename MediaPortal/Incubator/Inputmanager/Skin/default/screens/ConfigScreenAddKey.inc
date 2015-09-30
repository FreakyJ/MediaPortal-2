<?xml version="1.0" encoding="utf-8"?>

          <DockPanel
  xmlns="www.team-mediaportal.com/2008/mpf/directx"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
          Context="{Model Id=CC11183C-01A9-4F96-AF90-FAA046981006}"  IsVisible="{Binding ShowAddKey}" LastChildFill="False">
            <Label Content="[InputDeviceManager.AddKey.Header]" Color="{ThemeResource TextColor}" FontSize="36" DockPanel.Dock="Top" HorizontalAlignment="Center"/>
                <Label Content="{Binding AddKeyLabel}" Color="{ThemeResource TextColor}" FontSize="36" DockPanel.Dock="Top" HorizontalAlignment="Center"/>
                <Label Content="{Binding AddKeyCountdownLabel}" Color="{ThemeResource TextColor}" FontSize="36" DockPanel.Dock="Top" HorizontalAlignment="Center"/>
        </DockPanel>
