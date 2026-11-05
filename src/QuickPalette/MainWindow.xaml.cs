using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace QuickPalette;
public partial class MainWindow : Window
{
    private PaletteSettings _settings = SettingsStore.Load();
    private readonly List<PaletteItem> _allItems = [];
    private HwndSource? _source;
    private bool _settingsView;

    public MainWindow()
    {
        InitializeComponent();
        BuildItems();
        Loaded += (_, _) =>
        {
            _source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            _source?.AddHook(WndProc);
            Hotkey.Register(new WindowInteropHelper(this).Handle, _settings.Hotkey);
            SearchBox.Focus();
            RefreshResults("");
        };
    }
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 0x0312 && wParam.ToInt32() == Hotkey.Id)
        {
            if (IsVisible) Hide(); else { Show(); Activate(); SearchBox.Focus(); }
            handled = true;
        }
        return IntPtr.Zero;
    }
    private void BuildItems()
    {
        _allItems.Clear();
        foreach (var dir in new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
        })
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var file in Directory.EnumerateFiles(dir, "*.lnk", SearchOption.AllDirectories))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (_settings.HiddenApps.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
                _allItems.Add(new() { Title = name, Subtitle = "Launch application", Category = "Apps",
                    Icon = "▣", Action = "app", Target = file });
            }
        }
        foreach (var c in _settings.Commands)
            _allItems.Add(new() { Title = c.Name, Subtitle = c.Keywords, Category = "Commands",
                Icon = "›", Action = "command", Target = c.Command });
        foreach (var s in new[]
        {
            ("Windows Update", "ms-settings:windowsupdate"),
            ("Display settings", "ms-settings:display"),
            ("Bluetooth settings", "ms-settings:bluetooth"),
            ("Sound settings", "ms-settings:sound"),
            ("Wi-Fi settings", "ms-settings:network-wifi"),
            ("Personalization", "ms-settings:personalization"),
            ("Installed apps", "ms-settings:appsfeatures"),
            ("Default apps", "ms-settings:defaultapps"),
            ("Windows Security", "windowsdefender:")
        })
            _allItems.Add(new() { Title = s.Item1, Subtitle = "Open Windows Settings", Category = "Settings",
                Icon = "⚙", Action = "settings", Target = s.Item2 });
    }
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshResults(SearchBox.Text);
    private void RefreshResults(string query)
    {
        if (Results is null) return;
        var q = query.Trim();
        IEnumerable<PaletteItem> items;
        if (q.StartsWith("!"))
        {
            var split = q[1..].Split(' ', 2, StringSplitOptions.TrimEntries);
            var bang = split[0].ToLowerInvariant();
            var terms = split.Length > 1 ? split[1] : "";
            if (_settings.Bangs.TryGetValue(bang, out var template))
            {
                var url = template.Replace("{query}", Uri.EscapeDataString(terms));
                items = new[] { new PaletteItem { Title = $"Search {bang}: {terms}", Subtitle = url,
                    Category = "Web", Icon = "↗", Action = "url", Target = url } };
            }
            else items = [];
        }
        else if (q.StartsWith(">"))
        {
            var term = q[1..].Trim();
            items = _settings.Commands.Where(c => Match(c.Name + " " + c.Keywords, term))
                .Select(c => new PaletteItem { Title = c.Name, Subtitle = c.Command, Category = "Commands",
                    Icon = "›", Action = "command", Target = c.Command });
        }
        else
        {
            items = _allItems.Where(x => Match(x.Title + " " + x.Subtitle + " " + x.Category, q))
                .Where(x => !_settings.HiddenItems.Contains(x.Title, StringComparer.OrdinalIgnoreCase))
                .OrderBy(x => Rank(x, q)).ThenBy(x => x.Title).Take(30);
            if (q.Length > 1)
            {
                var web = "https://www.google.com/search?q=" + Uri.EscapeDataString(q);
                items = items.Concat(new[] { new PaletteItem { Title = $"Search web for “{q}”",
                    Subtitle = "Google search", Category = "Web", Icon = "↗", Action = "url", Target = web } });
            }
        }
        Results.ItemsSource = items.ToList();
        if (Results.Items.Count > 0) Results.SelectedIndex = 0;
        CountText.Text = $"{Results.Items.Count} results";
    }
    private static bool Match(string text, string query) =>
        query.Length == 0 || text.Contains(query, StringComparison.OrdinalIgnoreCase);
    private static int Rank(PaletteItem x, string q) =>
        x.Title.Equals(q, StringComparison.OrdinalIgnoreCase) ? 0 :
        x.Title.StartsWith(q, StringComparison.OrdinalIgnoreCase) ? 1 : 2;
    private void OpenSelected()
    {
        if (Results.SelectedItem is PaletteItem item) { Launcher.Run(item); Hide(); }
    }
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { Hide(); e.Handled = true; }
        else if (e.Key == Key.Enter) { OpenSelected(); e.Handled = true; }
        else if (e.Key == Key.Down && Results.Items.Count > 0)
        { Results.SelectedIndex = Math.Min(Results.SelectedIndex + 1, Results.Items.Count - 1); e.Handled = true; }
        else if (e.Key == Key.Up && Results.Items.Count > 0)
        { Results.SelectedIndex = Math.Max(Results.SelectedIndex - 1, 0); e.Handled = true; }
    }
    private void Results_MouseDoubleClick(object sender, MouseButtonEventArgs e) => OpenSelected();
    private void Results_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    private void Window_Deactivated(object sender, EventArgs e) { /* Keep open for palette workflows. */ }
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var message = "QuickPalette settings are stored in %AppData%\\\\QuickPalette\\\\settings.json.\\n\\n" +
            "Change Hotkey, HiddenApps, HiddenItems, Commands, or Bangs in that file.\\n" +
            "Examples: hide an app by its Start Menu shortcut name; add a bang like \\"ddg\\": \\"https://duckduckgo.com/?q={query}\\".";
        MessageBox.Show(message, "QuickPalette configuration", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    protected override void OnClosed(EventArgs e)
    {
        Hotkey.Unregister(new WindowInteropHelper(this).Handle);
        _source?.RemoveHook(WndProc);
        base.OnClosed(e);
    }
}
