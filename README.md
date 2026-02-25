# Noten (MVP+)

Noten is a lightweight Windows popup productivity panel built with **.NET 8 + WPF** and local-first JSON persistence.

## Current status
This iteration continues hardening the project for long-term solo development by expanding testable core logic and improving project/schedule workflows.

## Features currently implemented
- Popup panel with hide/show flow and tray-first behavior
- Global hotkey toggle (default `Ctrl+Space`) with parser-based configurable binding
- Single-instance startup guard (mutex)
- Tray menu:
  - Open Panel
  - Settings
  - Toggle Always On Top
  - Start with Windows
  - Exit
- Multi-project workspace model
  - Create
  - Rename
  - Duplicate
  - Delete (with confirmation)
  - Import / export project JSON
- Three tabs:
  - **Notes** (RTF editor + formatting toolbar)
  - **Lists** (multiple lists + checklist items)
  - **Schedule** (agenda table + filter: Today/Upcoming/All)
- Project-wide search (notes, lists, and schedule) with inline results
- Window placement restore with off-screen clamping
- Debounced auto-save + atomic JSON writes
- Expanded settings window (theme, startup, start-minimized, confirm-exit, data-path display, hotkey)
- Portable mode support via `NOTEN_PORTABLE=1` or `noten.portable` flag file beside executable
- Recurrence-ready schedule model (daily/weekly expansion in core service)

## Architecture
- `src/Noten.Core`
  - Models
  - JSON storage + repositories
  - Import/export validation and normalization
  - **Hotkey parser service** (pure logic)
  - **Window placement clamp service** (pure logic)
  - **Schedule filter/sort + recurrence expansion service** (pure logic)
  - **Data path resolver** (portable vs local appdata)
- `src/Noten.App`
  - WPF shell and views
  - Global hotkey + tray integration
  - Startup registration service (Windows Run key)
- `tests/Noten.Core.Tests`
  - Import/export tests
  - Repository bootstrap tests
  - Hotkey parser tests
  - Window placement clamp tests
  - Schedule filter + recurrence tests
  - Data path resolver tests
  - Project search tests

## Data location
`%LOCALAPPDATA%\Noten\`
- `appdata.json`
- `settings.json`

## Build and run
```bash
dotnet build Noten.sln
dotnet test tests/Noten.Core.Tests/Noten.Core.Tests.csproj
dotnet run --project src/Noten.App/Noten.App.csproj
```

## Publish (portable exe folder)
```bash
dotnet publish src/Noten.App/Noten.App.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Known limitations in this MVP
- Theme switching currently changes shell colors only (full resource dictionary theming pending)
- Notes link insertion uses selected text URL or a placeholder URL fallback
- List/schedule reordering drag-drop is not yet implemented
- Hotkey UI accepts text format, but no interactive key-capture widget yet

## Next priorities
1. Add keyboard-capture hotkey picker with conflict diagnostics.
2. Move to full resource dictionary theming (all controls and states).
3. Add drag/drop reordering for lists and items.
4. Add schedule row editor dialog with stronger date/time validation.
5. Add project archiving and soft-delete recovery.
6. Add optional SQLite backend + migration path from JSON.
7. Add portable mode option (store data beside executable).
8. Add encrypted export option for secure backups.
9. Improve second-launch behavior to bring running instance to front.
10. Add full-text indexing for very large project datasets.
