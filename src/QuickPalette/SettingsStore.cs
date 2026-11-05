using System.Text.Json;
namespace QuickPalette;
public static class SettingsStore
{
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickPalette");
    private static readonly string FilePath = Path.Combine(Folder, "settings.json");
    public static PaletteSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<PaletteSettings>(File.ReadAllText(FilePath)) ?? new();
        }
        catch { }
        return new();
    }
    public static void Save(PaletteSettings settings)
    {
        Directory.CreateDirectory(Folder);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings,
            new JsonSerializerOptions { WriteIndented = true }));
    }
}
