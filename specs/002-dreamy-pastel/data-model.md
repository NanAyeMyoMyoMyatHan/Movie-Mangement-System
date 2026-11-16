# Phase 1 Data Model: UI Theme State & Token Schema

**Feature**: `002-dreamy-pastel` | **Date**: 2026-11-16

---

## 1. UI Theme System Entities

While this feature does not alter relational SQL database tables, it defines the structured runtime design token model and reactive UI component states across the Blazor frontend.

### Design Token Hierarchy

```mermaid
classDiagram
    class GlobalCanvasTokens {
        +string CanvasGradient: "linear-gradient(145deg, #FAF5FF, #FDF2F8)"
        +string CanvasAmbientViolet: "rgba(139, 92, 246, 0.08)"
        +string CanvasAmbientPink: "rgba(236, 72, 153, 0.06)"
    }
    class SidebarTokens {
        +string Background: "#F3E8FF"
        +string BorderRight: "rgba(167, 139, 250, 0.28)"
        +string NavText: "#4C1D95"
        +string ActivePill: "linear-gradient(135deg, #8B5CF6, #EC4899)"
        +string ActiveText: "#FFFFFF"
    }
    class CardTokens {
        +string Background: "rgba(255, 255, 255, 0.92)"
        +string Border: "rgba(167, 139, 250, 0.22)"
        +string TitleText: "#2E1065"
        +string MetaText: "#6B5B95"
        +string Shadow: "0 8px 24px rgba(139, 92, 246, 0.08)"
    }
    class BrandAccentTokens {
        +string SoftViolet: "#8B5CF6"
        +string SoftPink: "#EC4899"
        +string GradientPrimary: "linear-gradient(135deg, #8B5CF6, #EC4899)"
        +string GlowShadow: "0 4px 18px rgba(139, 92, 246, 0.25)"
    }

    GlobalCanvasTokens --> SidebarTokens : harmonizes with
    GlobalCanvasTokens --> CardTokens : frames
    SidebarTokens --> BrandAccentTokens : highlights with
    CardTokens --> BrandAccentTokens : accents with
```

---

## 2. Component State Mapping

### Home View (`Index.razor`) State

| State Property | Type | Description |
| :--- | :--- | :--- |
| `SearchQuery` | `string` | Live search query typed into the pastel search bar |
| `SelectedFilter` | `string` | Active category pill (`"All"`, `"Trending"`, `"Korean Drama"`, `"Thai BL"`, `"Movies"`) |
| `CurrentHeroSlide` | `int` | Current slide index in the multi-slide hero carousel (0..3) |
| `ToastMessage` | `string?` | Floating reactive notification string |
| `IsToastVisible` | `bool` | Visibility toggle for floating feedback pill |

### Sidebar Navigation (`NavMenu.razor`) State

| State Property | Type | Description |
| :--- | :--- | :--- |
| `WatchCount` | `int` | Live count of watch-later items displayed in soft-violet badge |
| `FavoriteCount` | `int` | Live count of favorited items displayed in soft-pink badge |
| `AuthService.CurrentUser` | `User` | Active authenticated user displaying in the sidebar user pill |
