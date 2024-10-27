using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Nanohana;

public class LoginItem
{
    public string Password { get; set; } = "";
    public string Id { get; set; } = "";
    public string Site { get; set; } = "";
    public string Memo { get; set; } = "";

    public static string GetNanohanaFilePath()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string nanohanaDir = Path.Combine(documentsPath, ".nanohana");
        string nanohanaFile = Path.Combine(nanohanaDir, "data.json");
        return nanohanaFile;
    }

    public static List<LoginItem> Load()
    {
        string jsonString = File.ReadAllText(GetNanohanaFilePath());
        List<LoginItem> loginItems = JsonSerializer.Deserialize<List<LoginItem>>(jsonString);
        return loginItems;
    }

    public static void Save(List<LoginItem> loginItems)
    {
        string jsonString = JsonSerializer.Serialize(loginItems);
        File.WriteAllText(GetNanohanaFilePath(), jsonString);
    }
}