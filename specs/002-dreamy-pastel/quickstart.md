# Quickstart Guide: Dreamy Light Pastel UI Theme Validation

**Feature**: `002-dreamy-pastel` | **Date**: 2026-11-16

---

## 1. Prerequisites

- .NET 8 SDK installed
- Local SQL Server running (`MyFlixDb`)
- Google Chrome, Edge, or Firefox browser

---

## 2. Compilation & Startup

```powershell
# 1. Compile the solution
dotnet build DotNetProject.Balzor\DotNetProject.Balzor.csproj /t:Compile -nodereuse:false

# 2. Run the Blazor application
dotnet run --project DotNetProject.Balzor\DotNetProject.Balzor.csproj
```

Navigate to `http://localhost:5000` or the port shown in terminal output.

---

## 3. End-to-End Validation Scenarios

### Scenario 1: Verify Dreamy Pastel Background & Soft-Violet Sidebar

1. Open `http://localhost:5000/`.
2. Inspect the left sidebar:
   - Background is light soft-violet (`#F3E8FF` / `#EDE9FE`).
   - The cream/almond (`#F4EFEA`) background is completely gone.
   - Brand logo `NANGWORLD` glows with soft pink & soft violet text.
   - Active "Home" nav link has a soft violet-to-pink gradient pill with white text.
3. Inspect the main canvas:
   - Background displays a soft lavender-violet (`#FAF5FF`) and blush-pink (`#FDF2F8`) gradient.
   - Pitch-black background is completely gone.

### Scenario 2: Verify Airy Card Presentation & Typography

1. Scroll to the "Featured Asian Series", "Thai BL", and "Korean Dramas" sections.
2. Verify poster cards:
   - Cards have airy white frosted glass surfaces with soft violet borders.
   - Titles are deep rich violet (`#2E1065`), sharp and easy to read.
   - Hovering over a card shows smooth elevation and soft violet/pink shadow glow.

### Scenario 3: Verify Interactive Filter Pills & Search

1. Click on category filter pills (`✨ All`, `🔥 Trending`, `🇰🇷 Korean Drama`, `🇹🇭 Thai BL`, `🎬 Movies`).
2. Verify active pill takes on the gradient pill style with glow.
3. Type "Kingdom" into the search bar.
4. Verify matching results render cleanly on the pastel canvas.

### Scenario 4: Verify Supporting Views & Cache Invalidation

1. Click on any drama card to navigate to `/drama/{id}`.
2. Verify the drama detail page harmonizes with the soft violet/pink palette.
3. Inspect page source (`Ctrl+U`) and verify `App.razor` loads `<link rel="stylesheet" href="app.css?v=pastel-1.0" />`.
