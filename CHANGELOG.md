# Changelog
All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- Configurable shortcuts, managed from Settings: a list of available actions (start a bounty run, open EVE Journal) each with a **Record** button to bind your own combination, and a **Clear** button. Supports keyboard combos and mouse buttons (mouse 3/4/5), e.g. `Ctrl+Shift+Mouse4`. Keyboard shortcuts still work system-wide on Windows; mouse buttons work while the window is focused.
- System-wide (window-unfocused) keyboard shortcuts on Linux via the desktop's GlobalShortcuts portal (KDE Plasma). The app registers its actions with the desktop; assign keys under System Settings → Shortcuts. No extra permissions required.
- Setting to choose what is loaded at startup: load the full current gamelog session (default), or only new lines from the moment the agent starts. The live DPS meter always ignores old lines either way, so loading a session no longer sends a spike through the graph.
- Per-character DPS meter: a 📊 button next to each character in the list opens a pop-out DPS overlay scoped to just that character (the main meter still shows the combined fleet DPS). Multiple character overlays can be open at once.

### Fixed
- Fixed the security-status colour not showing next to a character's system in the character list
- Clearer "show offline characters" control: replaced the duplicated label and cut-off button with a single labelled toggle switch (Shown/Hidden)
- Character list is now sorted with online characters first, then offline
- Per-character bounty and the "last bounty" counter now update live again; previously they stayed at 0 because the values were bound through `Run` elements, which Avalonia does not refresh on change, and were updated off the UI thread
- Bounty amounts are now parsed regardless of the EVE client's language (handles comma, dot, space and apostrophe thousands separators)
- Only the current play session is loaded at startup (the log files still being written), instead of replaying every character's entire gamelog history

### Removed
- Removed the legacy WPF UI project (`EWB-Tracker`); the application is now Avalonia-only and cross-platform

## v0.2.0 - Cross-platform Avalonia release

### Added
- New Avalonia UI cross-platform application (UI.avalonia)
- AsyncImageLoader.Avalonia package for loading character avatar images from HTTP URLs
- Global keyboard shortcuts support (works even when window doesn't have focus):
  - Ctrl+Shift+N: Start new bounty run directly (no popup)
  - Ctrl+Shift+J: Open EVE Journal in browser
  - Uses Windows Win32 API (RegisterHotKey) with dedicated message-only window
  - Falls back to local keyboard events on non-Windows platforms
- Clickable hyperlink in Settings to open Personal Access Token page in browser
- Custom window controls with proper hover states and styling
- DPS meter rebuilt as a lightweight custom graph control with a pop-out, always-on-top overlay (draggable, resizable, adjustable opacity, remembers its position/size)
- Cross-platform EVE gamelog auto-detection (Steam/Proton including Flatpak and extra Steam libraries, plus Wine prefixes), with an "Auto-detect log location" button in Settings
- Automatic correction of the saved log path at startup when it no longer exists (e.g. a different Steam/Proton prefix)
- Manual resize grips on all edges/corners for the borderless window
- Alt+F4 closes the window
- Makefile with run/build/release/restore/clean targets for the Avalonia client

### Changed
- Migrated from WPF to Avalonia for cross-platform support
- Borderless window now uses `SystemDecorations="None"` for consistent custom chrome on all platforms (removes the duplicate native title bar that showed on Linux)
- Maximize now fits the screen's working area instead of full screen, so it no longer overlaps the taskbar/panel
- Reworked the DPS meter to a sliding-window tracker with wall-clock sampling and EMA smoothing at ~30fps; removed the LiveChartsCore dependency
- Store the database and overlay layout in a per-user data folder (`%APPDATA%\EveJournalTracker` on Windows, `~/.config/EveJournalTracker` on Linux/macOS) so updates can no longer overwrite them; an existing database next to the executable is copied across automatically on first launch
- Made the access token field in settings a password box to hide the token
- Update dotnet version to 9.0.10
- Silenced long-standing nullable/style build warnings so launching from a console stays clean (proper cleanup tracked separately)

### Fixed
- Fixed a crash / noisy `TaskCanceledException` on exit (Linux) by cancelling the file-watcher loop and stopping the host with a final save during shutdown
- Fixed the file watcher starting twice, which double-counted log events and logged "Log directory doesn't exist" twice
- Fixed Avalonia data binding feedback loop causing online status to flip between True/False
  - Changed Online property bindings to use `Mode=OneWay` in AccountView and CharacterAvatarControl
- Fixed character avatar images not loading by implementing AsyncImageLoader
- Fixed custom font not loading by reordering resource loading in App.axaml
- Fixed race condition in CheckOnlineJob by adding re-entrancy guard
- Fixed online status detection working correctly across all pages
- Fixed window control icons being misaligned by properly centering TextBlocks
- Fixed global hotkeys not working by creating a dedicated message-only window
  - Uses proper Windows message loop with WndProc callback
  - Hotkeys now work reliably even when application is in the background
- Fixed infinite API calls to universe/ids endpoint in CheckOnlineJob
  - Removed automatic character fetching loop that was running every 5 seconds
- Fixed character names showing as "Char-{ID}" when initial fetch failed
  - CheckOnlineJob now retries fetching character names on every check until successful

---

## v0.1.1 - Settings and UI Improvements

### Added
- Added a button to open the Evejournal website directly from the app and assign a hotkey to it (default: Ctrl+Shift+J)
- Added a quick link in settings to create a new personal access token on the Evejournal website

### Changed
- Changed settings text to clarify we need an access token, not an API key

### Fixed
- Fixed an issue where you couldn't use your scroll wheel to scroll through the settings page

---

## v0.1.0 - Initial Release

### Added
- Initial setup of the EWB Agent for reading EVE Online game log files
- Bounty run tracking
- Bounty per system tracking
- Integration with the EWB API for bounty tracking in the EWB EVE Journal
- Multi-character support
- Log viewer
- Settings for Configurable game log location and character log override