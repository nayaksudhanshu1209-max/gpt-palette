namespace QuickPalette;
public sealed class PaletteItem
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Category { get; set; } = "";
    public string Icon { get; set; } = "›";
    public string Action { get; set; } = "";
    public string? Target { get; set; }
    public override string ToString() => Title;
}
public sealed class PaletteSettings
{
    public string Hotkey { get; set; } = "Alt+Space";
    public List<string> HiddenApps { get; set; } = [];
    public List<string> HiddenItems { get; set; } = [];
    public List<UserCommand> Commands { get; set; } =
    [
        new() { Name = "Open Notepad", Command = "notepad.exe", Keywords = "notepad;editor" },
        new() { Name = "Open Calculator", Command = "calc.exe", Keywords = "calculator;calc" }
    ];
    public Dictionary<string,string> Bangs { get; set; } = new()
    {
        ["g"] = "https://www.google.com/search?q={query}",
        ["yt"] = "https://www.youtube.com/results?search_query={query}",
        ["gh"] = "https://github.com/search?q={query}",
        ["w"] = "https://en.wikipedia.org/wiki/Special:Search?search={query}",
        ["r"] = "https://www.reddit.com/search/?q={query}"
    };
}
public sealed class UserCommand
{
    public string Name { get; set; } = "";
    public string Command { get; set; } = "";
    public string Keywords { get; set; } = "";
}
