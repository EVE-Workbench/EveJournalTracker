# Changelog
All notable changes to this project will be documented in this file.

## [Unreleased]

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

### Changed
- Migrated from WPF to Avalonia for cross-platform support
- Made the access token field in settings a password box to hide the token
- Update dotnet version to 9.0.10

### Fixed
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