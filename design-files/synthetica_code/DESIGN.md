# Design System Document: Technical Precision & Tonal Depth

## 1. Overview & Creative North Star: "The Monolith"
This design system is built for the high-end developer environment, where focus is the ultimate currency. Our Creative North Star is **"The Monolith"**—a philosophy of architectural solidity, brutalist precision, and technical authority. 

Unlike "friendly" consumer apps, this system rejects rounded softness and the "rainbow" palette of traditional IDEs. It embraces **intentional asymmetry**, high-contrast editorial typography, and a "subtractive" aesthetic. We do not use lines to define space; we use light and shadow—the digital equivalent of a high-end, matte-black workstation.

---

## 2. Colors & Tonal Architecture
The palette is rooted in an ultra-dark spectrum. We avoid a "pure black" (#000000) base to prevent eye fatigue, opting instead for a deep obsidian sequence.

### The "No-Line" Rule
**Explicit Instruction:** Designers are prohibited from using 1px solid borders for sectioning or layout containment. Boundaries must be defined solely through background color shifts or tonal transitions.
*   **Good:** A `surface_container_low` sidebar sitting against a `surface` editor.
*   **Bad:** Using a `#3c495b` line to separate the file tree from the code view.

### Surface Hierarchy & Nesting
Treat the UI as a series of physical layers. Use the following tiers to create "nested" depth:
- **Base Layer (`surface`):** The primary editor background.
- **Sunken Layer (`surface_container_low`):** For utility panels (Terminal, File Tree).
- **Elevated Layer (`surface_container_high` / `highest`):** For floating menus, modals, or active overlays.

### The "Glass & Gradient" Rule
To elevate the experience from "standard tool" to "luxury hardware," use **Glassmorphism** for floating elements (e.g., Command Palettes). 
*   **Implementation:** Use `surface_variant` at 60% opacity with a `24px` backdrop-blur. 
*   **CTAs:** Primary buttons should use a subtle vertical gradient: `primary` (#4edea3) to `primary_dim` (#3cd096) to provide a "machined" look that flat hex codes cannot replicate.

---

## 3. Typography: Technical Editorial
We pair a technical, geometric display face with a workhorse sans-serif for maximum legibility.

- **Space Grotesk (Headings/Display):** Used for "Status" and "Context." It conveys a modern, high-tech engineering feel. Use wide letter-spacing (`0.05em`) for `label-md` to mimic blueprint annotations.
- **Inter (UI/Code):** The engine of the system. High x-height and neutral character for long-term readability in high-density environments.

**Hierarchy Note:** Use dramatic scale shifts. A `display-lg` headline should feel massive and authoritative, while UI labels remain tiny, sharp, and desaturated (`on_surface_variant`).

---

## 4. Elevation & Depth: Tonal Layering
We achieve hierarchy by "stacking" surfaces rather than applying drop shadows.

- **The Layering Principle:** Place a `surface_container_lowest` card on a `surface_container_low` section. This creates a soft, natural "lift."
- **Ambient Shadows:** For floating modals, use a "Shadow as Atmosphere."
    - **Specs:** `0px 24px 48px`, 6% opacity, using a tinted shadow color based on `surface_container_lowest` rather than pure black.
- **The "Ghost Border" Fallback:** If a border is required for accessibility (e.g., input fields), use the `outline_variant` token at **15% opacity**. Never use 100% opaque borders.

---

## 5. Components
All components must adhere to the **Zero-Radius Rule**: `0px` corner radius across all elements to maintain a precise, "pro-tool" silhouette.

### Buttons
*   **Primary:** Gradient fill (`primary` to `primary_dim`), `on_primary` text. No border.
*   **Secondary:** No fill. `Ghost Border` (15% opacity `outline_variant`). Text in `primary`.
*   **Tertiary:** Ghost style. No fill, no border. Text in `on_surface_variant`. On hover, shift background to `surface_container_high`.

### Input Fields
*   **Default:** `surface_container_low` background with a bottom-only `outline_variant` (20% opacity). 
*   **Focus:** The bottom border transforms into a 2px `primary` line. No "glow" or outer rings.

### Cards & Lists
*   **Constraint:** Forbid the use of divider lines. 
*   **Solution:** Use `16px` or `24px` of vertical whitespace. If separation is visually impossible, use a subtle background shift to `surface_container_highest` for the active list item.

### IDE-Specific Components
*   **Gutter/Line Numbers:** Text color `on_tertiary_container`. Active line number highlighted in `primary`.
*   **Breadcrumbs:** Use `label-sm` in `on_surface_variant`. Separate with a simple `/` or space, not an icon.
*   **Status Bar:** High-contrast `primary_container` background with `on_primary_container` text for the active branch/project.

---

## 6. Do’s and Don’ts

### Do:
*   **Embrace High-Quality Whitespace:** Use space as a structural element. 
*   **Use Mono-Tonal Status:** Use `error_container` (deep burgundy/charcoal) for errors rather than bright red. Only the icon or a small indicator should use the `error` (#ee7d77) token.
*   **Asymmetry:** Align meta-data (like file size or git status) to the far right, leaving significant "breathing room" between the label and the data.

### Don’t:
*   **Don't use Rounded Corners:** Any radius above 0px violates the system's professional integrity.
*   **Don't use "Rainbow" Syntax:** Avoid a dozen different colors for code. Use variations of `primary`, `secondary`, and `tertiary` to create a cohesive, focused coding environment.
*   **Don't use Heavy Borders:** If you feel a border is needed, try adjusting the background tone of the container first.