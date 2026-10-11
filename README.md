# Pulse Dashboard — WPF + Blazor Hybrid (VectoArt)

A dark-theme live dashboard. The window is **WPF**, the whole UI inside it is **Blazor** (Razor components + CSS),
and both halves share the same C# services. No third-party UI libraries.

## Run it
1. Windows 10/11, Visual Studio 2022 17.8+ (or the .NET 8 SDK), **WebView2 Runtime** (already on Windows 11).
2. Open `VectoArt.PulseDashboard.sln` → press **F5**.
3. Before/after demo: run `PulseDashboard.exe --before` (or set *Debug → Command line arguments* to `--before`)
   to see the same data in stock, unstyled WPF controls.
4. In Debug builds press **F12** inside the app to open browser DevTools.

## Licences (all free)
| Package | Licence |
|---|---|
| Microsoft.AspNetCore.Components.WebView.Wpf | MIT |
| CommunityToolkit.Mvvm (before-window only) | MIT |
| WebView2 Runtime | Free Microsoft redistributable |

Fonts: the CSS asks for Montserrat, then falls back to Segoe UI. No font or image files are shipped.

## Structure
```
PulseDashboard/
  Models/        immutable records (KpiMetric, ServerNode, DashboardSnapshot ...)
  Services/      DashboardService (live data), IFileExportService + WPF implementation
  Helpers/       ChartMath (SVG coordinates), DarkTitleBar (DWM)
  ViewModels/    BeforeViewModel (the stock-WPF "before" window only)
  Views/         MainWindow (hosts Blazor), BeforeWindow (stock controls)
  Components/    Razor: DashboardRoot, KpiCard, Sparkline, LineChart, DonutChart, ServerTable, Meter, ActivityFeed
  wwwroot/       index.html + css/theme.css (design tokens) + css/app.css
```
"Themes/" in this project is `wwwroot/css/theme.css`: Blazor themes are CSS variables, not ResourceDictionaries.

## Honest tradeoffs
- **Not compiled by the author's tooling.** This package was written without a .NET SDK available. Build it once in
  Visual Studio before you record; if a package version or a typo fails the build, fix it on camera. Real bugs are content.
- `Microsoft.AspNetCore.Components.WebView.Wpf` is referenced as `8.0.*` (latest 8.0 patch). Pin an exact version before shipping.
- Navigation is a simple state switch, not the Blazor `Router`. Fewer moving parts for a first video; add `Router` when you need URLs.
- Charts are hand-drawn SVG (about 40 lines of maths). That is the point of the video, but it is not a replacement for a full charting library.
- Data is a seeded random walk (`new Random(42)`), not a real backend. The service boundary is where a real API would plug in.
- No CSS isolation (`.razor.css`) is used: one global stylesheet keeps the theme in one place.
- Needs the WebView2 Runtime. Windows 10 machines without it will be prompted to install it.

## If the window is blank or stuck on "Starting…"
1. csproj must start with `Sdk="Microsoft.NET.Sdk.Razor"` (not `Microsoft.NET.Sdk`).
2. Do not add your own `<Content Include="wwwroot\**">` group.
3. `Selector="#app"` in `MainWindow.xaml` must match `<div id="app">` in `wwwroot/index.html`.
4. Press F12 (Debug build) and read the DevTools console.
