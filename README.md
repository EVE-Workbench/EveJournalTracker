# EVE Workbench Agent (EWB Tracker)

EWB Tracker is a companion tool for the [EVE Journal](https://evejournal.com/).  
This website allows you to track events, missions, rewards and bounties directly from your EVE Online game logs.

> [!IMPORTANT]
> This app is in early development. If you have trouble using it, please join our [Discord](https://discord.gg/dA3kHUv) for support.

## ✨ Features

- 📊 Track bounties through game logs  
- 🔫 DPS metrics  
- 📝 Create bounty runs and push them to the EWB Eve Journal  
- 👥 Multi-character support  

## 🚀 Getting Started

Download the latest [release](https://github.com/EVE-Workbench/EveJournalTracker/releases/latest), then follow the steps for your platform.

### Windows

1. Download `EWBAgent-win-x64.zip`.
2. Extract it to a folder of your choice.
3. Run **`EWBAgent.exe`**.

### Linux

The app ships as an **AppImage**, which runs on virtually any distribution without installation.

1. Download `EWBAgent-x86_64.AppImage`.
2. Make it executable: `chmod +x EWBAgent-x86_64.AppImage`
3. Run it: `./EWBAgent-x86_64.AppImage`

> A `EWBAgent-linux-x64.tar.gz` (self-contained folder) is also provided if you prefer to extract and run `./EWBAgent` yourself.

### First run

Open the **Settings** menu:

- Add your EVE Workbench **Access Token** (create one [here](https://evejournal.com/my-account/personal-access-tokens)).
- Verify your **game log folder** — on Linux the app auto-detects Steam/Proton locations, or use the **Auto-detect** button.
- Optionally rebind the **shortcuts** (start a bounty run, open EVE Journal): click **Record** and press a key combo or a mouse button (mouse 3/4/5). On Windows keyboard shortcuts work system-wide; on Linux/macOS and for mouse buttons they work while the window is focused.

## 💾 Where your data is stored

Your settings, characters and bounty data live **outside** the application folder, so they are never touched by an update:

| Platform | Location |
|----------|----------|
| Windows  | `%APPDATA%\EveJournalTracker\` (e.g. `C:\Users\<you>\AppData\Roaming\EveJournalTracker\`) |
| Linux    | `~/.config/EveJournalTracker/` |
| macOS    | `~/.config/EveJournalTracker/` |

This folder contains `eve_tracker.db` (settings + character info) and `dps-overlay.json` (DPS overlay layout).

## 🔄 Updating the Application

Just download and run the latest release — your data lives in the folder above and is **not** affected by updating.

- **Windows:** extract the new ZIP over (or replace) your old folder.
- **Linux:** replace the old AppImage with the new one.

> Upgrading from an older version that kept `eve_tracker.db` next to the executable? The app **automatically copies** that database into the new location on first launch, so your data carries over. The old file is left untouched; you can delete it afterwards.

## 📸 Screenshot

![Screenshot from 2025-08-29](https://cdn.imgpile.com/f/uAOIYrX_xl.png)

## 🗺️ Roadmap / Future Plans

- ✅ Cross-platform UI on [Avalonia](https://avaloniaui.net/) (Windows + Linux; macOS to follow)  
- Add mining support  
- Optional location tracking to map travels  
- Make keyboard shortcuts configurable

💡 Have ideas? Let us know what you’d like to see in future releases!  


## 📜 License

This project is licensed under the [MIT License](LICENSE).


## 💬 Contact

Join our [Discord](https://discord.gg/dA3kHUv) for feedback and support.
