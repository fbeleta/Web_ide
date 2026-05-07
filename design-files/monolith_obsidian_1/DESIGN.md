# Design System Specification: Core Identity & Implementation

## 1. Overview & Creative North Star: "The Obsidian Monolith"
The creative north star for this design system is **"The Obsidian Monolith."** It represents an elite, technical terminal—an interface that feels carved from a single block of dark matter, where every pixel serves a functional purpose and distractions are surgically removed. 

We are moving away from the soft, rounded "SaaS" aesthetic of the last decade. Instead, we embrace **Functional Brutalism.** By utilizing sharp 0px corners, high-contrast typography, and an uncompromising dark palette, we create a sense of digital authority. This design system breaks the "template" look through intentional asymmetry: heavy-weighted headers may sit flush-left while data-points are tucked into the far-right margins, creating a sophisticated, editorial-technical layout that feels bespoke.

---

## 2. Color Palette & The Tonal Logic
This system is strictly monochromatic with a singular, high-vibrancy "digital" spark. We prohibit any blue, navy, or warm undertones. All greys must be neutral or cool-neutral.

### Core Tokens
- **Surface (Base):** `#131313` (The void)
- **Surface Container Lowest:** `#0e0e0e` (Recessed areas)
- **Surface Container High:** `#2a2a2a` (Raised platforms)
- **Primary (Accent):** `#4edea3` (Mint Green - The interactive spark)
- **Outline Variant:** `#474747` (For "Ghost Borders")

### The "No-Line" Rule
Traditional 1px solid borders for sectioning are strictly prohibited. Boundaries must be defined through **Background Color Shifts**. To separate a sidebar from a main feed, use `surface-container-low` against the `surface` background. The eye should perceive the change in depth through the change in value, not a structural line.

### Surface Hierarchy & Nesting
Treat the UI as a series of physical layers stacked on top of one another.
- **Level 0 (Background):** `surface` (#131313)
- **Level 1 (Sectioning):** `surface-container-low` (#1c1b1b)
- **Level 2 (Active Cards):** `surface-container-highest` (#353534)
- **Interactive:** `surface-tint` (#4edea3)

---

## 3. Typography: Technical Editorial
We use **Space Grotesk** across the entire system. Its monospace-inspired architecture provides the "technical" DNA required, while its proportional spacing ensures readability at high-end editorial scales.

- **Display-LG (3.5rem):** Reserved for singular, high-impact data points or section titles.
- **Headline-SM (1.5rem):** Used for primary navigation headers. Always set to a tighter letter-spacing (-0.02em) to maintain a "dense" technical look.
- **Body-MD (0.875rem):** The workhorse for all data. 
- **Label-SM (0.6875rem):** Used for metadata, timestamps, and micro-copy. Always uppercase with +0.05em letter spacing.

The hierarchy is built on extreme contrast. Pair a `Display-LG` number with a `Label-SM` caption to create a "data-viz" aesthetic even in standard layouts.

---

## 4. Elevation & Depth: Tonal Layering
This system rejects traditional drop shadows. We communicate "lift" through color and transparency.

- **The Layering Principle:** To create a "floating" modal, do not apply a shadow. Instead, use `surface-container-highest` and wrap it in a 1px "Ghost Border" using `outline-variant` at 20% opacity.
- **Glassmorphism:** For floating overlays (like tooltips or dropdowns), use `surface-container-high` with a 70% opacity and a `backdrop-blur` of 20px. This allows the high-contrast text beneath to "ghost" through, maintaining the monolithic feel.
- **Signature Textures:** For high-priority CTAs, use a subtle linear gradient from `primary` (#ffffff) to `primary-container` (#5fedb0). This provides a "metallic" sheen that feels premium and intentional.

---

## 5. Components

### Buttons
- **Primary:** Background `primary-container` (#5fedb0), Text `on-primary-container` (#000000). Sharp 0px corners.
- **Secondary:** Transparent background, 1px "Ghost Border" (`outline-variant` @ 40%), Text `primary` (#ffffff).
- **Tertiary:** No border, no background. Underline on hover using the `primary` (#4edea3) color at 2px thickness.

### Input Fields
- **Style:** Never use "boxed" inputs. Use a "Bottom-Border Only" approach or a solid `surface-container-highest` block.
- **State:** On focus, the bottom border transitions to `primary` (#4edea3). Error states use `error` (#ffb4ab) with zero rounding.

### Cards & Lists
- **Rule:** Forbid divider lines.
- **Implementation:** Separate list items using 8px of vertical white space or a subtle hover state shift to `surface-bright`. Use the "Obsidian" depth: a card should be `surface-container-lowest` when inactive, shifting to `surface-container-high` on hover.

### Data Monoliths (Special Component)
For key metrics, use a full-width block with a `surface-container-low` background. Place the value in `display-md` and the label in `label-sm` (uppercase) in the top-left corner. This emphasizes the "technical terminal" personality.

---

## 6. Do’s and Don’ts

### Do:
- **Do** use 0px border-radius everywhere. No exceptions.
- **Do** use Mint Green (#4edea3) sparingly. It is a "laser pointer," not a "paint brush."
- **Do** lean into white space. High-end design requires "breathing room" to justify its technical complexity.
- **Do** align elements to a strict baseline grid to reinforce the "Monolith" structure.

### Don’t:
- **Don’t** use shadows to create depth. Use tonal shifts between greys.
- **Don’t** use icons with rounded terminals. Only use sharp, geometric iconography.
- **Don’t** use any color with a Blue/Navy hex code. If a grey looks "too blue," move it toward the neutral #1A1A1A.
- **Don’t** use center alignment for large blocks of text. Stick to the "Technical Editorial" flush-left grid.