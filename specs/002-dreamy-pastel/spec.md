# Feature Specification: Dreamy Light Pastel UI Theme (Soft Violet & Soft Pink)

**Feature Branch**: `001-dynamic-rbac`  
**Created**: 2026-11-16  
**Status**: Ready for Planning  
**Input**: User description: "change the nav bar color and the background black color to soft violet and soft pink /speckit-plan" + User choice: "Dreamy Light Pastel (Gentle pastel aesthetic with soft lavender-violet #FAF5FF and blush-pink #FDF2F8 canvas, light soft-violet navbar, and airy cards)"

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Dreamy Pastel Canvas & Light Soft-Violet Navigation Bar (Priority: P1)

As a user browsing NangWorld, I want the sidebar navigation bar and overall background to be rendered in a dreamy light pastel aesthetic (soft lavender-violet and blush-pink) rather than a cream sidebar and pitch black canvas, so that the streaming platform looks cohesive, elegant, and pleasing to the eye.

**Why this priority**: Directly resolves the user's explicit request and photo feedback where the sidebar was cream/almond and the background was solid black.

**Independent Test**: Load the home page `/` and observe that:
1. The sidebar background is a light soft-violet frosted panel (`#F3E8FF` / `#EDE9FE`).
2. The page background is a gentle pastel gradient blending soft lavender (`#FAF5FF`) and blush-pink (`#FDF2F8`).
3. Zero pitch black or cream sand remains on the canvas.

**Acceptance Scenarios**:
1. **Given** a visitor lands on any page (`/`, `/favorites`, `/watch-later`, `/category/*`), **When** the page renders, **Then** the sidebar displays with light soft-violet styling and the main body displays a soft violet-and-pink pastel canvas.
2. **Given** the user navigates between routes, **Then** active nav items display a vibrant soft violet-to-pink gradient pill with crisp white text.

---

### User Story 2 - Airy Luminous Content Cards & High-Contrast Typography (Priority: P2)

As a streaming viewer, I want poster cards, hero banners, filter chips, and search inputs to be styled as airy luminous surfaces with clear dark violet typography, so that titles, ratings, and tags are crisp and effortless to read against the pastel backdrop.

**Why this priority**: A pastel background requires high-contrast typography (`#2E1065`, `#4C1D95`) and crisp card containers to maintain readability and avoid visual muddiness.

**Independent Test**: Hover over cards in the poster grid and check that text titles are crisp, rating tags are vibrant, and hover overlays maintain clarity.

**Acceptance Scenarios**:
1. **Given** drama and movie cards on the home page, **When** viewed on the pastel canvas, **Then** card surfaces are light frosted white/glass (`#FFFFFF` / `rgba(255, 255, 255, 0.88)`), borders are soft violet (`rgba(167, 139, 250, 0.28)`), and titles are deep violet (`#2E1065`).
2. **Given** category filter pills, **When** selected, **Then** the active pill transitions to the soft violet-pink gradient with glowing shadow.

---

### User Story 3 - Synchronized Modals, Video Detail & Cache Invalidation (Priority: P3)

As a user playing videos or viewing details, I want the video modal, drama detail page, and all supporting views to harmonize with the soft violet & pink palette, and I want the browser cache to immediately load the new stylesheets without needing manual cache clearing.

**Why this priority**: Prevents stale CSS from persisting in the client browser via updated cache-busting query strings.

**Independent Test**: Hard refresh the browser and verify that `App.razor` version tags (`?v=pastel-1.0`) trigger instant fresh stylesheet loading across client and admin layouts.

**Acceptance Scenarios**:
1. **Given** `App.razor`, **When** the HTML `<head>` renders, **Then** stylesheet links reference `?v=pastel-1.0`.
2. **Given** the video player modal or drama detail page, **When** opened, **Then** control bars and episode pills use soft violet and soft pink styling.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST render the navigation sidebar (`NavMenu.razor`) with a light soft-violet frosted background (`#F3E8FF` / `#EDE9FE`) and soft violet border (`rgba(167, 139, 250, 0.25)`), eliminating all cream `#F4EFEA` / `#EFE6DC`.
- **FR-002**: System MUST render the global background canvas (`html`, `body`, `.petale-layout`, `.petale-main`) as a dreamy light pastel gradient transitioning from soft lavender-violet (`#FAF5FF`) to blush-pink (`#FDF2F8`) with subtle radial ambient glow accents.
- **FR-003**: System MUST update primary text typography to rich deep violet (`#2E1065`, `#3B0764`), muted text to `#6B5B95`, and subtle text to `#9382B2` to guarantee WCAG AA contrast on light pastel surfaces.
- **FR-004**: System MUST style media poster cards and continue-watching cards with airy frosted white surfaces (`#FFFFFF` / `rgba(255, 255, 255, 0.9)`), gentle violet borders (`rgba(167, 139, 250, 0.28)`), and soft elevation shadows (`0 8px 24px rgba(139, 92, 246, 0.08)`).
- **FR-005**: System MUST preserve the luminous soft violet & soft pink gradient (`linear-gradient(135deg, #8B5CF6 0%, #EC4899 100%)`) for primary buttons, active navigation pills, and active filter chips.
- **FR-006**: System MUST update `App.razor` stylesheet link tags with fresh version query strings (`?v=pastel-1.0`) to bust browser cache immediately.
- **FR-007**: System MUST synchronize CategoryView, Favorites, WatchLater, DramaDetail, and Video Modal with the Dreamy Light Pastel theme.
