# Feature Specification: Claymorphism UI Design Transformation

**Feature Identifier**: `001-claymorphism-ui`
**Created**: 2026-11-16
**Status**: In Progress
**Input**: User request: "change my ui design into Claymorphism design"

## Overview
Transform NangWorld's entire visual design system into an authentic Claymorphism aesthetic (soft, 3D, inflated pillowy shapes with dual inner highlights/shadows and soft ambient drop shadows) across both the client-facing video streaming application and the administrative portal, while preserving brand identity, responsiveness, and zero regressions.

## Key Claymorphic Principles
1. **Inflated 3D Volume**: Generous border-radius (18px-28px for cards, 9999px for pills) with layered inner highlights (`inset 2px 3px 5px rgba(255,255,255,...)`) and inner bevel shadows (`inset -3px -4px 6px rgba(0,0,0,...)`).
2. **Soft Floating Elevation**: Multi-layered outer drop shadows simulating soft light diffuse over warm clay backdrops.
3. **Tactile Squish Feedback**: Interactive elements depress slightly on `:active` (`translateY(2px) scale(0.98)`) with dynamic shadow inversion to simulate physical clay pressure.
4. **Color Harmony**: Warm clay cream/almond (`#F4EFEA`, `#EFE6DC`), terracotta clay (`#DE7149`), deep forest jade clay (`#183B36`, `#0F2420`).

## Functional Scope
- **Client Application (`app.css`)**: Sidebar navigation, top search bar, hero banner, movie & drama poster cards, continue watching cards, action buttons, category filter chips, modals.
- **Admin Portal (`admin.css`)**: Admin sidebar, stat cards, admin tables, action pills, modals, inputs.
- **Drama Detail Experience (`drama-detail.css`)**: Theater card container, video player shell, episode rows, credits box.
