# NutriBite — Food & Drink Nutrition Tracker

**NutriBite (食光营养助手)** is a cross-platform mobile application built with .NET MAUI for tracking food and drink intake, viewing nutrition summaries, and demonstrating mobile device hardware capabilities. It was developed as a course project for the "Food & Drink" module.

## Features

### Food & Drink Management
- **Catalog browsing** — scroll through a searchable list of food and drink items with real-time filtering.
- **Nutrition details** — view per-item calories, protein, carbs, fat, and allergy notes on a dedicated detail page.
- **Add new records** — fill in a form with name, category, description, and macro-nutrient values, with full input validation.
- **Edit records** — modify existing items inline, with pre-populated fields and the same validation rules.
- **Swipe-to-edit / swipe-to-delete** — quick actions directly from the main list, with confirmation dialogs and haptic feedback.
- **Nutrition dashboard** — a summary bar on the main page shows total items, calories, protein, and carbs across the current view.
- **Share & export** — share individual food items or generate and share a full nutrition report via the platform share sheet.

### Mobile Hardware Integration
- **Camera** — capture a food photo using the device camera, with permission handling and fallback messaging.
- **Geolocation** — fetch current coordinates and reverse-geocode to a human-readable address (country, city, street).
- **Text-to-Speech** — read nutrition summaries aloud in English, with graceful fallback when no TTS voice is installed.
- **Vibration & haptic feedback** — triggered on save, delete, validation errors, and explicit test buttons for verifiable hardware interaction.
- **Accelerometer** — live X/Y/Z readings with shake detection; start/stop monitoring from the hardware page.
- **Connectivity status** — display current network access type (Wi‑Fi, cellular, etc.) and online/offline state.
- **Device info** — show platform, model, manufacturer, OS version, and device idiom.

### Accessibility & UX
- **Dark mode / light mode** — theme picker with system-default, light, and dark options applied globally.
- **Large text mode** — toggle to scale font sizes across all pages; state is preserved during navigation.
- **Screen reader support** — `SemanticScreenReader.Announce` calls on every meaningful action (save, delete, refresh, navigation, hardware events).
- **Semantic descriptions** — each food card exposes a full accessible summary (`Name. Category. Calories. Macros. Allergy.`).
- **Validation feedback** — inline error messages plus vibration when required fields or numeric inputs are invalid.
- **Bilingual UI** — tab labels and key strings support English and Chinese via .NET resource files (`.resx`).

## Architecture

```
FoodDrinkApp/
├── Models/
│   └── FoodItem.cs              # Domain model with computed display properties
├── Services/
│   ├── DatabaseService.cs       # SQLite async CRUD (nutribite.db3)
│   ├── FoodCatalogService.cs    # Unified data layer: mockapi.io → SQLite → local fallback
│   ├── SpeechService.cs         # TTS with locale discovery, English-first, Chinese fallback
│   ├── AccessibilityService.cs  # Large-text font scaling via visual tree walk
│   ├── ShareService.cs          # Share sheet integration (single item + nutrition report)
│   ├── LocalizationService.cs   # .resx resource manager wrapper
│   └── MockApiConfig.cs         # Remote API endpoint configuration
├── Pages/
│   ├── MainPage                 # Food list with search, swipe actions, and nutrition dashboard
│   ├── FoodDetailPage           # Item detail with TTS read-aloud, share, and edit/delete actions
│   ├── AddItemPage              # Validated form for creating new food/drink records
│   ├── EditItemPage             # Pre-populated form for updating existing records
│   ├── HardwarePage             # Camera, GPS, TTS, haptics, accelerometer, connectivity, device info
│   └── SettingsPage             # Theme picker and large-text toggle
├── AppShell.xaml                 # Shell-based tab navigation (Foods, Hardware, Settings)
├── MauiProgram.cs               # MAUI app builder entry point
└── App.xaml                      # Application resource dictionary
```

### Data Flow

1. **Read path**: `FoodCatalogService` checks local SQLite first. If empty, it attempts to fetch from the configured mockapi.io endpoint. If that also fails, it seeds a built-in fallback catalog of 6 sample items (4 English, 2 Chinese).
2. **Write path**: creates/updates via mockapi.io when configured; always persists to local SQLite for offline resilience.
3. **Sync model**: the local database is the source of truth; the remote API is a best-effort mirror.

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 9.0 MAUI |
| Languages | C#, XAML |
| Targets | Android, iOS, Mac Catalyst, Windows |
| Local DB | SQLite via `sqlite-net-pcl` + `SQLitePCLRaw.bundle_green` |
| Remote API | mockapi.io (REST, JSON) |
| UI Pattern | Shell-based tab navigation, QueryProperty for parameter passing |
| Accessibility | `SemanticScreenReader`, `SemanticProperties`, visual-tree font scaling |
| Hardware APIs | `MediaPicker`, `Geolocation`, `TextToSpeech`, `Vibration`, `HapticFeedback`, `Accelerometer`, `Connectivity`, `DeviceInfo` |
| Localization | .NET `ResourceManager` with `.resx` files (en + zh-CN) |
| Logging | `Microsoft.Extensions.Logging.Debug` |

## Getting Started

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **.NET MAUI** workload installed
- .NET 9.0 SDK
- For Android: Android SDK with API 21+ emulator or device
- For Windows: Windows 10 version 1809 or later

### Build & Run

Clone the repository:

```bash
git clone https://github.com/HubuManchester/fooddrink-leileiyu159.git
cd fooddrink-leileiyu159/FoodDrinkApp
```

Build for Windows:

```powershell
dotnet build -f net9.0-windows10.0.19041.0
```

Build for Android:

```powershell
dotnet build -f net9.0-android
```

Run on Windows:

```powershell
dotnet run -f net9.0-windows10.0.19041.0
```

### Remote API Configuration

To enable mockapi.io synchronization, set the endpoint URL in `Services/MockApiConfig.cs`:

```csharp
public const string EndpointUrl = "https://your-project.mockapi.io/foods";
```

When the endpoint is configured, the app will:
- Fetch data from the remote API on first launch if the local database is empty.
- POST/PUT/DELETE records to keep the remote mirror in sync.
- Fall back to the local SQLite database when the API is unreachable.

When no endpoint is configured, the app uses the built-in local fallback catalog and SQLite exclusively.

## Scoring Coverage *(course project)*

| Criterion | Implementation |
|---|---|
| UI/UX & Accessibility | XAML pages, bottom tab navigation, dark/light theme, large-text mode, semantic descriptions, screen reader announcements |
| Mobile Hardware | Camera, geolocation, text-to-speech, vibration, haptic feedback, accelerometer, connectivity, device info |
| Functional Completeness | List, search, detail, add, edit, delete, settings, and hardware demo flows |
| Validation & Error Handling | Required-field checks, numeric validation, permission errors, hardware-unavailable prompts |
| Code Quality | Model/service separation, clear naming, reusable catalog service, focused page code-behind |
| Deployment | Cross-platform .NET MAUI app targeting Android and Windows |
| GitHub Usage | Incremental commits with descriptive messages throughout development |

## Screencast Demonstration Checklist

- Introduce the "Food & Drink" theme and the NutriBite app concept.
- Demonstrate search, detail view, and adding a new record.
- Show validation prompts when required fields are empty or numbers are invalid.
- Demonstrate camera, geolocation, text-to-speech, vibration, and haptic feedback.
- Show dark mode and large-text mode toggles.
- Walk through key source files: models, services, pages, and Android permission configuration.
- Demonstrate deployment on both Android and Windows.
- Show GitHub commit history and the README.

## License

This project is created for educational purposes as part of a university coursework module.

---

**Author:** [leileiyu159](https://github.com/leileiyu159)
