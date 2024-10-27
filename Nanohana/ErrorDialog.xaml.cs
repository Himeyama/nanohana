using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;

namespace Nanohana;

public sealed partial class ErrorDialog : Page
{
    public ErrorDialog()
    {
        InitializeComponent();
    }

    public static async void Show(MainWindow mainWindow, string message)
    {
        ContentDialog dialog = new ContentDialog();
        dialog.XamlRoot = mainWindow.Content.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = new ErrorDialog();
        dialog.PrimaryButtonText = "OK";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = message;
        await dialog.ShowAsync();
    }
}