# QuickPalette

A lightweight Windows command palette starter built with native WPF and .NET 8.

## Build without installing a compiler

1. Create a GitHub repository and upload this folder's contents.
2. Open **Actions** in the repository.
3. Select **Build QuickPalette** and click **Run workflow** (or push to `main`).
4. Open the completed workflow run and download the `QuickPalette-win-x64` artifact.
5. Extract the ZIP and run `QuickPalette.exe`.

## Current features

- Global hotkey (default `Alt+Space`)
- Start Menu shortcut indexing and app launch
- Built-in Windows Settings links
- Default web search and configurable `!bang` providers
- Basic user command definitions
- Hide app and suggestion entries through JSON settings
- Keyboard navigation, dark rounded WPF palette

## Configuration

Settings are stored at:

`%AppData%\\QuickPalette\\settings.json`

Example:
```json
{
  "Hotkey": "Ctrl+Space",
  "HiddenApps": ["Example App"],
  "HiddenItems": ["Windows Update"],
  "Commands": [
    { "Name": "Open Paint", "Command": "mspaint.exe", "Keywords": "draw;image" }
  ],
  "Bangs": {
    "g": "https://www.google.com/search?q={query}",
    "yt": "https://www.youtube.com/results?search_query={query}",
    "gh": "https://github.com/search?q={query}"
  }
}
```

## Notes

This is a working starter, not a feature-complete PowerToys replacement. App discovery currently reads Start Menu shortcuts; full installed-app indexing, live system settings indexing, fuzzy scoring, plugin extensions, richer settings UI, robust command argument parsing, and audio/music service integrations are suitable next milestones.
