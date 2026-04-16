# Design System Specification: Architectural Precision

## 1. Overview & Creative North Star: "The Obsidian Monolith"
This design system is a study in restrained power and technical permanence. It rejects the "softness" of modern consumer interfaces in favor of a brutalist, architectural aesthetic. The Creative North Star is **"The Digital Ledger"**—an interface that feels carved from stone, possessing the unwavering precision of a high-end technical instrument.

To move beyond generic dark modes, this system utilizes a "Hard-Edge" philosophy. We ignore the standard practice of rounded corners and soft shadows. Instead, we embrace sharp 0px radii and intentional tonal layering. We move away from traditional grids by using extreme typographic scale shifts and "ghost" boundaries to create an interface that feels like a singular, solid entity rather than a collection of floating widgets.

---

## 2. Colors: The Void and the Signal
The palette is strictly monochromatic, rooted in absolute black. This provides a high-density foundation where the only "life" comes from the Mint Green signal.

| Token | Value | Role / Intent |
| :--- | :--- | :--- |
| `surface` | #131313 | The primary canvas; a deep, neutral obsidian. |
| `surface_container_lowest` | #0e0e0e | Recessed areas, used for background "wells" or input fields. |
| `surface_container_high` | #2a2a2a | Elevated modules; used to lift content off the primary surface. |
| `primary` | #ffffff | Essential content and high-contrast typography. |
| `surface_tint` | #4edea3 | **The Signal.** Mint Green. Used exclusively for success and primary calls to action. |
| `outline_variant` | #474747 | The "Ghost Border." The only permissible stroke for structural definition. |

### The "No-Line" Rule
Designers are prohibited from using 1px solid white or high-contrast borders to section content. Boundaries must be defined through **Background Shift**. To separate a sidebar from a main view, transition from `surface` (#131313) to `surface_container_low` (#1c1b1b). Use the technical "thin, subtle border" (#2a2a2a) only when absolute containment is required for interactive elements.

### Signature Textures
While the system is brutalist, it is not flat. Use a subtle **Linear Gradient** on primary action buttons, transitioning from `surface_tint` (#4edea3) to `primary_fixed` (#006c4a) at a 135-degree angle. This provides a "machined" look that mimics technical hardware.

---

## 3. Typography: Architectural Clarity
We use **Space Grotesk** across the entire system. Its geometric construction and idiosyncratic "g" and "y" provide the necessary technical "soul."

*   **Display (L/M/S):** Used for data visualization headers or hero sections. These should be set with `-0.04em` letter spacing to feel compact and "heavy."
*   **Headline & Title:** The "Architectural Labels." Use Sentence case only. Never use All-Caps for headlines; let the weight and scale carry the authority.
*   **Body (L/M/S):** Optimized for technical legibility. Line height is set generously (1.5) to balance the density of the black background.
*   **Labels:** Small, precise metadata. These are the "serial numbers" of your UI.

---

## 4. Elevation & Depth: Tonal Layering
In this system, "Up" does not mean "Closer to the light." It means "Higher Contrast."

*   **The Layering Principle:** Depth is achieved by stacking surface tiers. A `surface_container_highest` (#353534) card placed on a `surface` (#131313) background creates a sharp, physical lift.
*   **Zero Shadows:** Traditional drop shadows are forbidden. To simulate a "floating" state (like a modal), use a 1px border of `outline_variant` and increase the background contrast to `surface_bright` (#3a3939).
*   **The "Ghost Border" Fallback:** When a component requires a boundary (e.g., a text input), use the #2a2a2a border specified in the brief. It must feel like a hairline trigger—precise and barely there.
*   **Glassmorphism:** For overlays or navigation bars, use `surface` (#131313) at 80% opacity with a `20px` backdrop blur. This ensures the "Obsidian" feel remains consistent as the user scrolls.

---

## 5. Components: The Technical Kit

### Buttons
*   **Primary:** Fill `surface_tint` (#4edea3), text `on_primary` (#002114). Sharp 0px corners.
*   **Secondary:** Ghost style. 1px border of `outline` (#919191), text `primary` (#ffffff).
*   **Tertiary:** Text only, `primary` (#ffffff) with an underline visible only on hover.

### Input Fields
*   **Style:** Background `surface_container_lowest` (#0e0e0e), bottom-border only (1px, #2a2a2a). 
*   **State:** On focus, the bottom border transforms to `surface_tint` (#4edea3).

### Cards & Lists
*   **Rule:** Forbid divider lines. Use `24px` or `32px` vertical gaps to separate list items.
*   **Nesting:** Nested data should sit on a slightly lighter surface (`surface_container_low`) to indicate hierarchy.

### The "Monolith" Data Widget (Custom Component)
For technical readouts, use a `surface_container_highest` block with a 2px left-accent border of `surface_tint`. This acts as a high-visibility container for critical system status or primary metrics.

---

## 6. Do’s and Don’ts

### Do:
*   **Embrace the Void:** Use large areas of `#000000` to create a sense of premium vastness.
*   **Align to the Edge:** Use a strict column grid, but feel free to break the "rows" to create an asymmetric, editorial feel.
*   **Check Contrast:** Ensure all `on_surface_variant` text (#c6c6c6) meets WCAG AA standards against the dark backgrounds.

### Don't:
*   **No Rounded Corners:** Any radius above `0px` is a violation of the system's architectural integrity.
*   **No Blue Tones:** Absolutely avoid any navy, slate, or cool-grey tones that aren't derived from the obsidian palette.
*   **No Heavy Shadows:** Do not use `box-shadow`. If an element needs to pop, use a tonal shift or a hairline border (#2a2a2a).
*   **No Color Bloat:** If a color isn't Black, Grey, White, or Mint Green, it does not belong in this system.