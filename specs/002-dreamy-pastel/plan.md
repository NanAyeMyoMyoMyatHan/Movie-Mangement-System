# Implementation Plan: Dreamy Light Pastel UI Theme (Soft Violet & Soft Pink)

**Branch**: `001-dynamic-rbac` | **Date**: 2026-11-16 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-dreamy-pastel/spec.md`

## Summary

Transform the NangWorld streaming interface from the previous dark obsidian/black canvas and cream sidebar into a **Dreamy Light Pastel** aesthetic as selected by the user. The background is redesigned to a soft lavender-violet (`#FAF5FF`) and blush-pink (`#FDF2F8`) gradient canvas, the sidebar navigation is upgraded to a light soft-violet frosted panel (`#F3E8FF`), and card containers are elevated with airy frosted glass surfaces and high-contrast deep violet typography (`#2E1065`, `#4C1D95`), with `App.razor` cache busting (`?v=pastel-1.0`) ensuring instant browser updates.

## Technical Context

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: Blazor InteractiveServer, ASP.NET Core 8.0, CSS3 Variables & Flexbox/Grid  
**Storage**: N/A (Pure frontend UI & styling overhaul)  
**Testing**: Manual visual & interactive verification across browser viewports (Scenario validation in quickstart.md)  
**Target Platform**: Modern desktop, tablet, and mobile browsers (Chromium, Firefox, WebKit)  
**Project Type**: Blazor Server web application UI layer  
**Performance Goals**: 60fps smooth transitions, zero layout shifts, instantaneous theme application  
**Constraints**: WCAG AA color contrast compliance (minimum 4.5:1 ratio; achieved > 11:1 with `#2E1065` on `#FAF5FF`); zero disruption to existing RBAC permissions, routes, or database functionality  
**Scale/Scope**: Styling updates across `app.css`, `MainLayout.razor.css`, `NavMenu.razor.css`, `Index.razor.css`, `CategoryView.razor.css`, `Favorites.razor.css`, `WatchLater.razor.css`, `drama-detail.css`, and cache-busting in `App.razor`

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Status | Notes |
| :--- | :--- | :--- |
| **Simplicity (YAGNI)** | PASS | Implements clean CSS custom properties and scoped stylesheets without heavy 3rd-party UI libraries. |
| **Zero Regression on Existing Features** | PASS | All interactive features (real-time search, hero carousel, dynamic filter pills, toasts, video playback) are 100% preserved. |
| **Accessibility & Readability** | PASS | Deep violet typography (`#2E1065`) on pastel surfaces delivers high contrast exceeding WCAG AA standards. |
| **Clean Separation of Concerns** | PASS | Centralized design tokens in `app.css` with scoped styles for modularity. |

## Project Structure

### Documentation (this feature)

```text
specs/002-dreamy-pastel/
├── spec.md                  # Feature Specification (User scenarios, requirements)
├── plan.md                  # This Implementation Plan (/speckit-plan output)
├── research.md              # Phase 0 Architecture Decisions (/speckit-plan output)
├── data-model.md            # Phase 1 UI State & Token Schema (/speckit-plan output)
├── quickstart.md            # Phase 1 Validation & Testing Guide (/speckit-plan output)
├── contracts/               # Phase 1 Interface Contracts
│   └── theme-tokens-contract.md # CSS design tokens & component conformance contract
└── tasks.md                 # Phase 2 Tasks Breakdown (/speckit-tasks output)
```

### Source Code (affected areas)

```text
DotNetProject.Balzor/
├── Components/
│   ├── App.razor                        # Update stylesheet link tags with ?v=pastel-1.0
│   ├── Layout/
│   │   ├── MainLayout.razor.css         # Pastel canvas gradient (#FAF5FF to #FDF2F8)
│   │   ├── NavMenu.razor                # Soft violet/pink branding logo
│   │   └── NavMenu.razor.css            # Light soft-violet frosted sidebar (#F3E8FF)
│   └── Pages/
│       ├── Index.razor.css              # Airy card surfaces, deep violet text, pastel search
│       ├── CategoryView.razor.css       # Pastel hero header and airy filter chips
│       ├── Favorites.razor.css          # Pastel curated collection header and airy cards
│       ├── WatchLater.razor.css         # Pastel bookmark view and cards
│       └── Login.razor                  # Standalone soft violet & pink pastel login
└── wwwroot/
    ├── app.css                          # Global pastel token system and component resets
    └── drama-detail.css                 # Theater player controls in soft violet/pink
```

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
| :--- | :--- | :--- |
| Multi-file CSS update | Scoped CSS architecture in Blazor | Relying only on global styles causes scoped styles to override global rules unpredictably |
| Cache query string bump (`?v=pastel-1.0`) | Instant browser invalidation | Without cache busting, browsers retain stale cached stylesheets from earlier runs |
