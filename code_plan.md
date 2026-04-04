# Code Plan: Navigation Layout and Icon Fix

## Problem Analysis

### Problem 1 — Windows: Navigation Bar at Top Instead of Left Sidebar

**Root cause:** `AppShell.xaml` uses a `<TabBar>` with two `ShellContent` items.
MAUI Shell renders `TabBar` as:
- Horizontal tabs at the **top** on Windows (user sees "Chat | Patients" across the header)
- Horizontal tabs at the **bottom** on Android/iOS

The design document shows:
- **Windows/Desktop:** A persistent left sidebar listing "Chat" and "Patients" items with icons.
- **Mobile:** A bottom tab bar with an icon and label per tab.

MAUI only renders a left sidebar when using `FlyoutItem` with `FlyoutBehavior = Locked`, not
when using `TabBar`. The navigation structure must therefore change, conditioned on the device idiom.

### Problem 2 — Mobile: Icons Not Showing in Tab Bar

**Root cause (three layers):**
1. Neither `ShellContent` in the current `<TabBar>` sets an `Icon` property.
2. No tab-bar icon image files exist in `Resources/Images/` (only `dotnet_bot.png` is present).
3. MAUI tab bar icons require a proper image asset (PNG or SVG). Emoji characters used in
   `Label.Text` cannot be used as `ShellContent.Icon`.

---

## Solution Architecture

### Strategy: Adaptive Navigation in AppShell.xaml.cs

`AppShell.xaml` will keep only the `<Shell>` element with its colour and style attached
properties. All navigation children (TabBar or FlyoutItems) are built programmatically in the
constructor of `AppShell.xaml.cs` based on `DeviceInfo.Idiom`.

This is the minimal-change approach that:
- Keeps Shell as the navigation root (no new layers).
- Avoids duplicating full XAML files per platform.
- Gives each idiom the exact Shell structure MAUI needs to render the correct chrome.

#### Desktop / Windows path (DeviceIdiom.Desktop)

FlyoutBehavior = Locked means the sidebar is always visible.
Two FlyoutItem entries navigate to MainPage and PatientsPage.
FlyoutIsPresented = true ensures the sidebar starts open on launch.

MAUI maps this to a WinUI NavigationView pane on the left, matching the design.

#### Mobile path (DeviceIdiom.Phone, DeviceIdiom.Tablet, and default)

FlyoutBehavior = Disabled means no flyout or hamburger is shown.
A TabBar containing two ShellContent items renders as a bottom tab bar.

### Strategy: SVG Icon Assets

Two minimal SVG files are added to `Resources/Images/`. The existing project glob
`<MauiImage Include="Resources\Images\*" />` auto-includes them at build time.
MAUI converts them to platform-appropriate raster sizes automatically.

| File | Purpose |
|------|---------|
| Resources/Images/icon_chat.svg | Tooth icon for the Chat tab and flyout item |
| Resources/Images/icon_patients.svg | Person icon for the Patients tab and flyout item |

Icon SVG fills must be opaque (for example fill="#000000") so MAUI platform tinting can
override them with the correct selected or unselected colour at runtime.

### Strategy: Flyout Sidebar Styling (Desktop)

The flyout background, text, and icon tint colours are set as attributes on the `<Shell>` element
in `AppShell.xaml` to match the existing dark theme.

| Shell property | Value |
|---|---|
| FlyoutBackgroundColor | DynamicResource CardBackground (#121826) |
| Shell.FlyoutTextColor | DynamicResource TextPrimary (white) |
| Shell.FlyoutIconImageTintColor | DynamicResource TextPrimary |

---

## Files to Change

| File | Action | Reason |
|------|--------|--------|
| AppShell.xaml | Edit: remove TabBar children; add flyout colour properties | XAML must not pre-define items that code-behind will build |
| AppShell.xaml.cs | Edit: add BuildDesktopNavigation and BuildMobileNavigation | Adaptive idiom-based navigation construction |
| Resources/Images/icon_chat.svg | Create | Tooth SVG icon for Chat tab and flyout item |
| Resources/Images/icon_patients.svg | Create | Person SVG icon for Patients tab and flyout item |

No changes are needed to the .csproj, services, view models, or page XAML files.

---

## Visual Layout Targets

Desktop (Windows):
Left pane with DentAI header, Chat item, and Patients item. Right area shows page content.

Mobile (Android / iOS):
Full-screen page content with a bottom tab bar showing Chat and Patients with icons and labels.

---

## Risks and Constraints

- FlyoutBehavior.Locked on Windows draws a WinUI NavigationView pane whose exact visual chrome
  (font, item height, hover colour) is controlled by WinUI, not MAUI styles. The sidebar will be
  functionally correct; pixel-perfect matching depends on WinUI defaults.
- FlyoutIsPresented = true on Desktop ensures the sidebar starts expanded. The user can still
  collapse it via the WinUI hamburger toggle button at the top of the pane.
- SVG fills must be solid and opaque so MAUI tinting pipeline works correctly on all platforms.
