# Phase 0 Research: Dreamy Light Pastel UI Theme Architecture

**Feature**: `002-dreamy-pastel` | **Date**: 2026-11-16

---

## 1. Architecture Decisions

### Decision 1: Pastel Color Palette Token Hierarchy

- **Decision**: Define a comprehensive CSS Custom Property token set anchored on soft lavender-violet (`#FAF5FF`, `#EDE9FE`, `#DDD6FE`) and blush-pink (`#FDF2F8`, `#FCE7F3`, `#FBCFE8`), with deep violet typography (`#2E1065`, `#4C1D95`).
- **Rationale**: The user explicitly requested removing the cream sidebar and pitch-black background in favor of a "Dreamy Light Pastel" theme. Replacing black with a light pastel canvas requires high-contrast typography to maintain WCAG AA compliance (4.5:1 minimum; deep violet `#2E1065` on `#FAF5FF` achieves > 11:1).
- **Alternatives Considered**:
  - *Keep dark mode with purple tint*: Rejected by user in favor of Dreamy Light Pastel.
  - *Pure white background (#FFFFFF)*: Too harsh and sterile; soft lavender-to-blush gradient provides the requested dreamy, aesthetic warmth.

### Decision 2: Sidebar Navigation Bar Styling

- **Decision**: Style `.myflix-sidebar` and `.petale-sidebar` with light soft-violet frosted acrylic background (`#F3E8FF` / `#EDE9FE`), a delicate violet border (`rgba(167, 139, 250, 0.25)`), and rich violet nav links (`#4C1D95`), with active items highlighted in the vibrant soft violet-to-pink gradient pill (`linear-gradient(135deg, #8B5CF6 0%, #EC4899 100%)`).
- **Rationale**: Replaces the cream/almond `#F4EFEA` / `#EFE6DC` sidebar seen in the user's screenshot, harmonizing the left navigation with the dreamy pastel main content.
- **Alternatives Considered**:
  - *White sidebar*: Lacks character and does not deliver the requested soft violet brand identity.

### Decision 3: Card Surface & Component Elevation

- **Decision**: Use airy frosted glass cards (`rgba(255, 255, 255, 0.9)` / `#FFFFFF`) with soft violet borders (`rgba(167, 139, 250, 0.25)`) and multi-layer diffuse shadows (`0 8px 24px -4px rgba(139, 92, 246, 0.08)`).
- **Rationale**: Keeps movie and drama poster artwork vibrant while elevating cards off the pastel canvas without heavy dark outlines.
- **Alternatives Considered**:
  - *Flat solid cards*: Less modern; frosted glass provides depth and aligns with modern streaming platform design.

### Decision 4: CSS Cache Busting in `App.razor`

- **Decision**: Update all stylesheet `<link>` tags in `App.razor` to include `?v=pastel-1.0`.
- **Rationale**: Browsers aggressively cache `.css` files. In the user's previous run, `?v=clay` kept stale styling cached. Version bumping guarantees immediate visual updates upon page reload.

---

## 2. Token Specification Table

| Token Variable | Hex / Expression | Purpose |
| :--- | :--- | :--- |
| `--color-canvas-lavender` | `#FAF5FF` | Base canvas lavender tint |
| `--color-canvas-pink` | `#FDF2F8` | Base canvas blush pink tint |
| `--color-canvas-gradient` | `linear-gradient(145deg, #FAF5FF 0%, #F5EEFD 40%, #FDF2F8 100%)` | Full-page background canvas |
| `--color-sidebar-bg` | `#F3E8FF` | Light soft-violet navigation bar |
| `--color-sidebar-border` | `rgba(167, 139, 250, 0.28)` | Sidebar right border |
| `--color-card-bg` | `rgba(255, 255, 255, 0.92)` | Airy card container background |
| `--color-card-border` | `rgba(167, 139, 250, 0.22)` | Airy card border |
| `--text-primary` | `#2E1065` | Main headings, titles, active text |
| `--text-secondary` | `#4C1D95` | Navigation labels, subtitles |
| `--text-muted` | `#6B5B95` | Metadata, year, genres, durations |
| `--brand-gradient` | `linear-gradient(135deg, #8B5CF6 0%, #EC4899 100%)` | Action buttons, active pills, badges |
| `--brand-violet` | `#8B5CF6` | Soft violet primary accent |
| `--brand-pink` | `#EC4899` | Soft pink secondary accent |
| `--brand-glow` | `0 4px 18px rgba(139, 92, 246, 0.25)` | Pill & button elevation glow |
