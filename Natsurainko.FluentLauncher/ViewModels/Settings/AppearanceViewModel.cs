﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.XamlHelpers.Converters;
using System.IO;
using System.Threading.Tasks;
using Windows.UI;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AppearanceViewModel : SettingsViewModelBase, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;

    public AppearanceViewModel(SettingsService settingsService, NotificationService notificationService)
    {
        _settingsService = settingsService;
        _notificationService = notificationService;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.DisplayTheme))]
    private int displayTheme;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.BackgroundMode))]
    [NotifyPropertyChangedFor(nameof(CanUseImageThemeColor))]
    private int backgroundMode;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseSystemAccentColor))]
    [NotifyPropertyChangedFor(nameof(CurrentThemeColor))]
    private bool useSystemAccentColor;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.MicaKind))]
    private int micaKind;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.ImageFilePath))]
    [NotifyPropertyChangedFor(nameof(ImageFileExists))]
    [NotifyPropertyChangedFor(nameof(CanUseImageThemeColor))]
    private string imageFilePath;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.SolidSelectedIndex))]
    private int solidSelectedIndex;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CustomThemeColor))]
    [NotifyPropertyChangedFor(nameof(CurrentThemeColor))]
    private Color? customThemeColor;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CustomBackgroundColor))]
    private Color? customBackgroundColor;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseBackgroundMask))]
    private bool useBackgroundMask;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.UseHomeControlsMask))]
    private bool useHomeControlsMask;

    public Color CurrentThemeColor => UseSystemAccentColor ? (Color)App.Current.Resources["RawSystemAccentColor"] : CustomThemeColor.GetValueOrDefault();

    public bool AcrylicIsSupported => DesktopAcrylicController.IsSupported();

    public bool MicaIsSupported => MicaController.IsSupported();

    public bool ImageFileExists => File.Exists(ImageFilePath);

    public bool CanUseImageThemeColor => BackgroundMode == 3 && ImageFileExists;

    private Flyout backgroundColorFlyout;
    private Flyout themeColorFlyout;

    [RelayCommand]
    private void Loaded(object args)
    {
        var button = args.As<Button, object>().sender;

        if (button.Tag.ToString() == "backgroundColor")
            backgroundColorFlyout = button.Flyout as Flyout;
        else themeColorFlyout = button.Flyout as Flyout;
    }

    [RelayCommand]
    private void SelectColorConfirm(Button button)
    {
        if (button.Tag.ToString() == "backgroundColor")
            backgroundColorFlyout.Hide();
        else themeColorFlyout.Hide();
    }

    [RelayCommand]
    private void RadioButtonChecked(int index)
        => BackgroundMode = index;

    [RelayCommand]
    private async Task UseImageThemeColor()
    {
        CustomThemeColor = await DominantColorHelper.GetColorFromImageAsync(ImageFilePath);
        var converter = (ColorHexCodeConverter)Application.Current.Resources["ColorHexCodeConverter"];

        _notificationService.NotifyWithoutContent(
            $"Image theme color successfully set, {converter.Convert(CustomThemeColor, null, null, null)}",
            icon: "\uE73E");
    }

    /// <summary>
    /// 神金 Command 无法被触发
    /// </summary>
    public void BrowserImage()
    {
        var openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = false;
        openFileDialog.Filter = "Png Image File|*.png|JPG Image File|*.jpg|BMP Image File|*.bmp|All Files|*.*";

        if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            ImageFilePath = openFileDialog.FileName;
    }
}
