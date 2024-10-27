using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;


namespace Nanohana;

public sealed partial class MainWindow : Window
{
    LoginItem loginItem = new();
    List<LoginItem> loginItems;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        SetSize();
        InitNanohanaData();
        loginItems = LoginItem.Load();

        ShowLoginItems();
        LinkButton.Content = SetTheURL.Text;
    }

    void AddLoginItem(LoginItem loginItem){
        LoginItemList.Items.Add(loginItem);
    }

    void ShowLoginItems(){
        LoginItemList.Items.Clear();
        foreach(LoginItem loginItem in loginItems){
            AddLoginItem(loginItem);
        }
    }

    void InitNanohanaData()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string nanohanaDir = Path.Combine(documentsPath, ".nanohana");
        if(!Directory.Exists(nanohanaDir))
        {
            Directory.CreateDirectory(nanohanaDir);
            File.SetAttributes(nanohanaDir, File.GetAttributes(nanohanaDir) | FileAttributes.Hidden);
        }
        string nanohanaFile = Path.Combine(nanohanaDir, "data.json");
        if(!File.Exists(nanohanaFile))
        {
            File.WriteAllText(nanohanaFile, "[]");
        }
    }

    async void SetSize()
    {
        while (Content.XamlRoot == null)
        {
            await Task.Delay(100);
        }
        IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(myWndId);
        double dpiScale = Content.XamlRoot.RasterizationScale;
        int width = (int)(960 * dpiScale);
        int height = appWindow.Size.Height; // 現在の高さを保持
        appWindow.Resize(new SizeInt32(width, height));
    }

    void CopyToClipboard(string text)
    {
        DataPackage dataPackage = new();
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
        Clipboard.Flush(); 
    }

    void CopyId(object sender, RoutedEventArgs e)
    {
        CopyToClipboard(loginItem.Id);
    }

    void CopyPassword(object sender, RoutedEventArgs e)
    {
        CopyToClipboard(loginItem.Password);
    }

    void ClickEditSite(object sender, RoutedEventArgs e)
    {
        EditSite.Visibility = Visibility.Visible;
        UpdateSite.Visibility = Visibility.Visible;
        LinkButton.Visibility = Visibility.Collapsed;
        EditSiteButton.Visibility = Visibility.Collapsed;

        if (LinkButton.Content is string linkButtonContent)
        {
            if (linkButtonContent == SetTheURL.Text)
            {
                EditSite.Text = "";
            }else
            {
                EditSite.Text = loginItem.Site;
            }
        }
    }

    void ClickUpdateSite(object sender, RoutedEventArgs e)
    {
        string site = EditSite.Text;
        if (site == "")
        {
            ShowErrorDialog(URLIsNotEntered.Text);
            return;
        }
        try{
            LinkButton.NavigateUri = new Uri(site);
            loginItem.Site = site;
            LinkButton.Content = site;
        }catch(Exception){
            ShowErrorDialog(InvalidURL.Text);
            return;
        }
        EditSite.Visibility = Visibility.Collapsed;
        UpdateSite.Visibility = Visibility.Collapsed;
        LinkButton.Visibility = Visibility.Visible;
        EditSiteButton.Visibility = Visibility.Visible;
    }

    // 文字列を指定されたファイルに書き込む関数
    static void WriteToLogFile(string filePath, string content)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(content);
        }
    }

    void CheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (RevealModeCheckBox.IsChecked == true)
        {
            InputPasswordBox.PasswordRevealMode = PasswordRevealMode.Visible;
        }
        else
        {
            InputPasswordBox.PasswordRevealMode = PasswordRevealMode.Hidden;
        }
    }

    void UpdateUI(LoginItem loginItem)
    {
        LinkButton.Content = loginItem.Site;
        try
        {
            LinkButton.NavigateUri = new Uri(loginItem.Site);
        }
        catch
        {
            LinkButton.NavigateUri = null;
        }
        UserId.Text = loginItem.Id;
        InputPasswordBox.Password = loginItem.Password;
        Memo.Text = loginItem.Memo;
    }

    void ListView_LoginItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is LoginItem _loginItem)
        {
            LoginItem loginItem = new()
            {
                Site = _loginItem.Site,
                Id = _loginItem.Id,
                Password = _loginItem.Password,
                Memo = _loginItem.Memo
            };
            UpdateUI(loginItem);
        }
    }

    void ShowErrorDialog(string message)
    {
        ErrorDialog.Show(this, message);
    }

    void Save(object sender, RoutedEventArgs e)
    {
        loginItem.Id = UserId.Text;
        loginItem.Password = InputPasswordBox.Password;
        loginItem.Site = LinkButton.Content as string;

        if(loginItem.Site == SetTheURL.Text)
        {
            ShowErrorDialog(URLIsNotSet.Text);
            return;
        }

        bool writeFlag = false;
        List<LoginItem> newLoginItems = new List<LoginItem>();
        foreach(LoginItem _loginItem in loginItems)
        {
            if(loginItem.Id == _loginItem.Id && loginItem.Site == _loginItem.Site)
            {
                // 書き換え
                writeFlag = true;
                newLoginItems.Add(loginItem);
            }else{
                newLoginItems.Add(_loginItem);
            }
        }
        if(!writeFlag)
        {
            newLoginItems.Add(loginItem);
        }
        loginItem = new(){
            Site = SetTheURL.Text
        };
        UpdateUI(loginItem);
        loginItems = newLoginItems;
        ShowLoginItems();
        LoginItem.Save(loginItems);
    }

    void OpenWebsite(object sender, RoutedEventArgs e)
    {
        string url = "https://github.com/himeyama/nanohana";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    void DeleteLoginItem(object sender, RoutedEventArgs e)
    {
        loginItem.Id = UserId.Text;
        loginItem.Password = InputPasswordBox.Password;
        loginItem.Site = LinkButton.Content as string;

        List<LoginItem> newLoginItems = new List<LoginItem>();
        foreach(LoginItem _loginItem in loginItems)
        {
            if(loginItem.Id == _loginItem.Id && loginItem.Site == _loginItem.Site)
            {
                // 何もしない
            }else{
                newLoginItems.Add(_loginItem);
            }
        }
        loginItem = new(){
            Site = SetTheURL.Text
        };
        UpdateUI(loginItem);
        loginItems = newLoginItems;
        ShowLoginItems();
        LoginItem.Save(loginItems);
    }

    void Search_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = sender as TextBox;
        string searchText = textBox.Text;
        if(searchText == "")
        {
            loginItems = LoginItem.Load();
            ShowLoginItems();
            return;
        }

        List<LoginItem> newLoginItems = new List<LoginItem>();
        List<LoginItem> allLoginItems = LoginItem.Load();
        foreach(LoginItem loginItem in allLoginItems)
        {
            if(loginItem.Site.Contains(searchText))
            {
                newLoginItems.Add(loginItem);
            }
        }
        loginItems = newLoginItems;
        ShowLoginItems();
    }

    void ClickExit(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
