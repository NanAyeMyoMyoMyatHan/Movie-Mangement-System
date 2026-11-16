# Interface Contract: Dreamy Light Pastel Theme Tokens

**Contract**: `theme-tokens-contract.md` | **Feature**: `002-dreamy-pastel`

---

## 1. CSS Custom Properties Contract (`:root`)

The global stylesheet `DotNetProject.Balzor/wwwroot/app.css` exposes and conforms to the following design token contract:

```css
:root {
    /* Dreamy Light Pastel Canvas Tokens */
    --color-pastel-lavender: #FAF5FF;
    --color-pastel-lavender-subtle: #F3E8FF;
    --color-pastel-pink: #FDF2F8;
    --color-pastel-pink-subtle: #FCE7F3;

    --bg-canvas-gradient: linear-gradient(145deg, #FAF5FF 0%, #F5EEFD 45%, #FDF2F8 100%);
    --bg-sidebar-nav: #F3E8FF;
    --bg-sidebar-border: rgba(167, 139, 250, 0.28);

    --bg-card-airy: rgba(255, 255, 255, 0.92);
    --border-card-airy: 1px solid rgba(167, 139, 250, 0.22);
    --shadow-card-airy: 0 8px 24px -4px rgba(139, 92, 246, 0.08), 0 4px 12px -2px rgba(236, 72, 153, 0.04);
    --shadow-card-hover: 0 16px 36px -6px rgba(139, 92, 246, 0.16), 0 6px 18px -2px rgba(236, 72, 153, 0.08);

    /* High-Contrast Typography */
    --text-primary: #2E1065;
    --text-secondary: #4C1D95;
    --text-muted: #6B5B95;
    --text-dim: #9382B2;

    /* Brand Accents */
    --brand-violet: #8B5CF6;
    --brand-violet-soft: #A78BFA;
    --brand-pink: #EC4899;
    --brand-pink-soft: #F472B6;
    --brand-gradient: linear-gradient(135deg, #8B5CF6 0%, #EC4899 100%);
    --brand-gradient-hover: linear-gradient(135deg, #7C3AED 0%, #DB2777 100%);
    --brand-glow: 0 4px 18px rgba(139, 92, 246, 0.25);
}
```

---

## 2. Component Conformance Contract

| Component Class | Required Visual Style | Prohibited Patterns |
| :--- | :--- | :--- |
| `.petale-sidebar`, `.myflix-sidebar` | Background `#F3E8FF` (soft-violet frosted), border `rgba(167, 139, 250, 0.28)` | No cream `#F4EFEA`, `#EFE6DC`, or dark obsidian `#110D1D` |
| `html, body`, `.petale-main` | Background `linear-gradient(145deg, #FAF5FF 0%, #F5EEFD 45%, #FDF2F8 100%)` | No pitch black `#000000`, `#0B0813`, `#0F2420` |
| `.myflix-poster-card`, `.myflix-continue-card` | Surface `rgba(255, 255, 255, 0.92)`, border `rgba(167, 139, 250, 0.22)` | No dark green `#0F2420` or black card fills |
| `.myflix-card-title`, `h2` | Color `#2E1065` (deep violet) | No light gray or white text on light pastel canvas |
| `.myflix-nav-item.active`, `.myflix-filter-pill.active` | Background `linear-gradient(135deg, #8B5CF6 0%, #EC4899 100%)`, text `#FFFFFF` | No terracotta or plain gray backgrounds |
