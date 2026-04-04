# Code Tasks: Navigation Layout and Icon Fix

## Task 1 — Create SVG Icon Assets

**Acceptance criteria:**
- `Resources/Images/icon_chat.svg` exists and contains a clean tooth/chat SVG path with a solid black fill.
- `Resources/Images/icon_patients.svg` exists and contains a clean person SVG path with a solid black fill.
- Both files are picked up automatically by the `<MauiImage Include="Resources\Images\*" />` glob.

**Steps to solve:**
1. Create `DentistAssistantAI.App/Resources/Images/icon_chat.svg` with a tooth SVG shape.
2. Create `DentistAssistantAI.App/Resources/Images/icon_patients.svg` with a person SVG shape.

**Files to review:** `DentistAssistantAI.App.csproj` (confirm glob), `Resources/Images/`

---

## Task 2 — Update AppShell.xaml (structure and styling only)

**Acceptance criteria:**
- The `<TabBar>` block and its `<ShellContent>` children are removed from `AppShell.xaml`.
- The `<Shell>` element retains existing tab bar colour properties.
- Three new flyout styling properties are added to `<Shell>`: `FlyoutBackgroundColor`,
  `Shell.FlyoutTextColor`, and `Shell.FlyoutIconImageTintColor`.
- The XAML still compiles without errors.

**Steps to solve:**
1. Open `AppShell.xaml`.
2. Delete the `<TabBar>` block (lines 14–20).
3. Add `FlyoutBackgroundColor`, `Shell.FlyoutTextColor`, and `Shell.FlyoutIconImageTintColor`
   attributes to the `<Shell>` element.

**Files to review:** `AppShell.xaml`

---

## Task 3 — Update AppShell.xaml.cs (adaptive navigation construction)

**Acceptance criteria:**
- On Windows/Desktop (`DeviceInfo.Idiom == DeviceIdiom.Desktop`):
  - Shell has `FlyoutBehavior = FlyoutBehavior.Locked`.
  - Two `FlyoutItem` entries (Chat and Patients) with correct icons and `ContentTemplate` are added.
  - `FlyoutIsPresented = true` so the sidebar starts open.
- On all other idioms (Phone, Tablet, default):
  - Shell has `FlyoutBehavior = FlyoutBehavior.Disabled`.
  - A `TabBar` with two `ShellContent` entries (Chat and Patients) with correct icons and
    `ContentTemplate` is added.
- The code-behind compiles with no errors.

**Steps to solve:**
1. Open `AppShell.xaml.cs`.
2. After `InitializeComponent()`, check `DeviceInfo.Idiom`.
3. Implement `BuildDesktopNavigation()` private method:
   - Set `FlyoutBehavior = FlyoutBehavior.Locked`.
   - Create two `FlyoutItem` objects and add `ShellContent` children.
   - Add both to `Items`.
   - Set `FlyoutIsPresented = true`.
4. Implement `BuildMobileNavigation()` private method:
   - Set `FlyoutBehavior = FlyoutBehavior.Disabled`.
   - Create a `TabBar` and add two `ShellContent` children with icons.
   - Add the `TabBar` to `Items`.
5. Call the appropriate method from the constructor.

**Files to review:** `AppShell.xaml.cs`, `AppShell.xaml`, `Views/PatientsPage.xaml.cs`, `MainPage.xaml.cs`

---

## Task 4 — Build Verification

**Acceptance criteria:**
- Project builds successfully targeting `net10.0-windows10.0.19041.0` with zero errors.
- No regressions introduced to the DI registrations or page types.

**Steps to solve:**
1. Run build via `run_build` tool.
2. Fix any compilation errors reported.

**Files to review:** build output
