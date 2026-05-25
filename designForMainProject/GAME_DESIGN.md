# SPHERON — Cube Abyss (working title)

> Upgraded from the 2D "Circle vs Square" concept: the circle player becomes a 3D energy sphere,
> the square enemies become 3D cubes.
> This document defines the direction and scope of that upgrade. It is a top-level design —
> a statement of intent, not an implementation spec.

---

## Title

- **Name (working title)**: SPHERON — Cube Abyss
- **One-liner**: You are an energy sphere descending, in first person, into an abyss made of cubes — clear each room, break each boss.
- **Relation to the previous game**: A 3D sequel / remake of the 2D top-down prototype "Circle vs Square". It keeps the minimalist geometric language (sphere vs cube) but fully reworks the perspective, the way the player moves and aims, and the feel of combat.
- **Repository**: <https://github.com/Luoluoluo0110/Games-Programming.git> — the game project lives under `MainProject/`.

---

## Form: n-D Person

- **Dimension**: 3D
- **Person**: First-Person
- **Meaning**: The player *is* the sphere — the view sits at its centre, looking outward. Enemies are cubes of varying size and behaviour.
- **What this changes (vs the 2D prototype)**:
  - Visibility goes from full top-down awareness to a first-person view with blind spots.
  - Space goes from a flat plane to a volume with depth, height, and orientation.
  - Combat goes from "every enemy visible at once, kite in open space" to "enemies can be behind you — awareness and turning matter".
  - Presentation goes from flat 2D shapes to 3D forms within a navigable, lit space.

---

## Core Gameplay

A first-person, geometric-shape clear-out experience — part shooter, part dungeon crawler:

1. The player controls an energy sphere — moving, aiming, and attacking inside an enclosed room.
2. Each room contains a number of cube enemies that pursue and attack the player.
3. Clearing every cube in the current room opens the way to the next room.
4. Every few rooms there is a **Boss encounter** (one large or multi-form shape).
5. Beating the Boss advances to the next stretch of the game. If the player falls, the current section restarts.

Pacing: short rooms strung into a level, with a Boss as the closing beat of each level. The emphasis is on a clear sense of progress — advancing room by room, with Bosses as checkpoints.

---

## Core Rules

- **Player survival**: The player has health; contact with a cube or its attacks reduces it; at zero, the player is defeated.
- **Player attack**: The player can attack at range; landing a hit reduces an enemy's health.
- **Enemy survival**: Each cube has health; at zero, it is destroyed.
- **Enemy behaviour**: Basic cubes pursue the player and attack once in range.
- **Room clear**: When every cube in a room is destroyed, the exit opens.
- **Boss rules**: A Boss moves through multiple phases; each phase changes how it behaves. Beating it clears the level.
- **Fail / restart**: On defeat, the player returns to the start of the current section. A broader run-progression system can come later.
- **Win condition (initial scope)**: Clearing the final Boss ends the game with a victory state.

---

## Slice — Minimum Playable Version (MVP)

The shortest end-to-end loop, used to confirm whether "3D first-person + sphere vs cube" actually works:

- **A first-person player** that can move, look around, and attack.
- **One cube enemy type** that approaches the player and deals damage.
- **A short sequence of rooms**, ending in a single simple Boss.
- **Combat loop**: the player can take damage and be defeated; enemies can be defeated.
- **Flow loop**: clear a room → advance; beat the Boss → victory; be defeated → restart.
- **Minimal feedback**: the player can read their health, their progress, and the win/lose state.

> Visuals at this stage stay as simple as possible — placeholder shapes are enough. Whether final art is produced in-house or sourced elsewhere is left open; the slice only needs to validate feel and flow.

---

## Core Systems

The MVP needs a small set of clearly separated responsibilities. These describe *what* the game must handle — not how it is built:

- **Game flow** — overall state (playing / win / lose) and restarting.
- **Player movement & view** — moving and looking in first person.
- **Player survival** — health, taking damage, defeat.
- **Player attack** — attacking and dealing damage.
- **Enemy behaviour** — sensing the player, approaching, attacking.
- **Enemy survival** — health, hit feedback, removal on defeat.
- **Boss behaviour** — phased behaviour distinct from a basic enemy.
- **Room & level structure** — populating a room, detecting when it is cleared, linking rooms into a sequence.
- **Interface & feedback** — conveying health, progress, and win/lose state to the player.

> Each responsibility should stay independent enough to develop and test on its own. Some may be merged early if that is simpler — this list is a guide to scope, not a fixed architecture.

---

## Biggest Risk

**The biggest risk: moving from 2D top-down to 3D first-person is effectively a rewrite, and the prototype's "fun" may not survive the transition.**

- The core thrill of the 2D prototype comes from seeing every cube at once and kiting in open space. First person introduces blind spots — enemies behind the player are unseen, so the player can be hit with no warning, which reads as frustration rather than tension.
- Movement, camera, collisions, and enemy navigation all have to be reconceived for 3D space. The work is far larger than the word "upgrade" suggests.
- The minimalist geometric look is highly readable in 2D, but in 3D first person it can feel empty and short on spatial reference, leaving players disoriented.

**Mitigations**:

- Build the feel-validation slice first. If "move the sphere, attack the cubes" is not fun in 3D first person, change direction early (for example, fall back to third-person or 2.5D).
- Address the "can't see behind you" problem with clear cues — sound, a damage-direction indicator, and visible signals on enemies.
- Keep rooms small and enclosed, and use the environment and lighting to give the player a stable sense of place.

---

## Development Direction

A rough order of work, moving from "is this fun at all" toward "is this worth expanding":

1. **Feel slice** — get a first-person sphere moving and looking around in a single space.
2. **Combat loop** — add attacking, one enemy type, and mutual damage so player and enemy can defeat each other.
3. **Room loop** — a room that populates with enemies, detects a clear, and opens an exit.
4. **Flow loop** — chain a few rooms together, ending in a simple Boss.
5. **Polish the slice** — minimal interface, win/lose states, restart.
6. **Playtest & decide** — play the slice end to end, judge whether the core feel holds, then decide whether to expand.

Engine choice, tooling, version control, and asset sourcing are deliberately left out of this document. They are implementation choices to settle separately, once the direction is confirmed.

---

## Scope

### Must-have (required for the MVP — without these it does not stand up)

- A first-person player: movement, looking, and attacking.
- One cube enemy type with basic pursuit behaviour.
- A ranged attack that deals damage.
- Player health, taking damage, and defeat.
- Enemy health and being defeated.
- Room clear → advance to the next room.
- One Boss (a single form is fine).
- Minimal interface: health and progress.
- Victory / defeat states and restart.

### Should-have (add right after the MVP — decides "is it fun")

- A few cube variants with distinct behaviours (e.g. ranged, charging, exploding).
- A dash or dodge action.
- Game feel and juice: hit feedback, defeat effects, screen shake, sound.
- A cue for enemies outside the player's view.
- A handful of hand-designed levels with rising difficulty.
- Multi-phase Bosses.

### Could-have (nice to have — only with spare capacity)

- Multiple attack options or modes.
- Between-level upgrade choices for the sphere.
- A minimap or room indicator.
- A settings menu (volume, sensitivity).
- More Bosses and room themes.
- Richer environmental lighting and atmosphere.

### Cut first (drop these first — do not pursue)

- Online or multiplayer (PvP or co-op).
- Procedurally generated levels.
- Story, cutscenes, voice-over.
- Save / progression systems.
- Heavy, realistic art production.
- Gamepad or mobile support.
- Achievements, store, in-app purchases, and other meta systems.
