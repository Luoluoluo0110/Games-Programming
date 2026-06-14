# Bonecrypt

A first-person shooter dungeon crawler built in **Unity 2022.3 (LTS)** with **C#**.
Fight your way through a series of rooms, clear each wave of enemies to open the
door ahead, and defeat the boss to escape the crypt.

## Gameplay

- **Move** with `WASD`, **look** with the mouse, **shoot** with the left mouse button (`Fire1`).
- Each room locks you in with a wave of enemies the moment you enter it.
- Clear every enemy in a room and its exit door opens automatically.
- Reach and clear the **boss room** to win.
- If your health drops to zero, you lose.
- After winning or losing, press `R` to restart the level.

## Controls

| Action      | Input                |
|-------------|----------------------|
| Move        | `W` `A` `S` `D`      |
| Look        | Mouse                |
| Shoot       | Left mouse button    |
| Restart     | `R` (after win/lose) |

## How It Works

The game is split into nine single-responsibility scripts under
[`Assets/Scripts/`](Assets/Scripts/):

| Script | Responsibility |
|--------|----------------|
| [`GameManager.cs`](Assets/Scripts/GameManager.cs) | Global game state machine (Playing / Win / Lose). Pauses the game with `Time.timeScale`, shows end screens, and reloads the level on restart. Implemented as a singleton. |
| [`PlayerController.cs`](Assets/Scripts/PlayerController.cs) | First-person movement and mouse-look. Uses a `CharacterController` with simple gravity and clamps the player to the level bounds. |
| [`PlayerHealth.cs`](Assets/Scripts/PlayerHealth.cs) | Tracks the player's hit points. Raises `OnHealthChanged` / `OnDeath` events and triggers the lose state at zero HP. |
| [`WeaponController.cs`](Assets/Scripts/WeaponController.cs) | Fires projectiles while the fire button is held, rate-limited by a cooldown. Plays optional SFX, muzzle flash and animation. |
| [`Projectile.cs`](Assets/Scripts/Projectile.cs) | A single bullet. Flies straight (no gravity), damages any enemy it hits, and self-destructs after a lifetime. |
| [`EnemyCube.cs`](Assets/Scripts/EnemyCube.cs) | Enemy AI (regular mobs and boss). Uses a `Rigidbody` to chase and face the player, deals contact damage on a cooldown, and raises `OnDeath` when killed. |
| [`RoomManager.cs`](Assets/Scripts/RoomManager.cs) | One per room. A trigger volume spawns the enemy wave on first entry, tracks living enemies, and opens the exit door once the room is cleared. The boss room triggers the win. |
| [`HUDController.cs`](Assets/Scripts/HUDController.cs) | On-screen UI (singleton). Updates the health bar via events, shows the room number, and toggles the victory / defeat panels. |
| [`LevelExitPortal.cs`](Assets/Scripts/LevelExitPortal.cs) | A trigger placed past the boss door that finishes the level when the player walks into it. |

### A round of play, step by step

1. The player enters a room → `RoomManager` spawns that room's enemy wave.
2. Enemies chase the player each physics step and deal contact damage on a cooldown.
3. The player shoots → a `Projectile` spawns → on impact it damages the `EnemyCube`.
4. When an enemy's HP hits zero it raises `OnDeath`; `RoomManager` removes it from the living list.
5. When the room's living list is empty, the exit door opens.
6. Clearing the boss room calls `GameManager.TriggerWin()` → victory screen.
7. If the player's HP reaches zero first, `GameManager.TriggerLose()` → defeat screen.

### Design notes

- **Singletons** (`GameManager`, `HUDController`) give any script a single global access point without passing references around.
- **Events / observer pattern** keep modules decoupled: `PlayerHealth` and `EnemyCube` broadcast changes; the HUD and `RoomManager` subscribe, so neither side needs to know about the other.
- **Physics-driven enemies** zero their velocity every `FixedUpdate` and move with `MovePosition`, so knockback from being shot can't shove them off course or through walls.

## Getting Started

1. Install **Unity 2022.3.62f1** (or a matching 2022.3 LTS release) via Unity Hub.
2. Clone this repository and open the project folder in Unity Hub.
3. Open the scene at [`Assets/Scenes/Main.unity`](Assets/Scenes/Main.unity).
4. Press **Play** in the Unity Editor.

## Project Structure

```
Assets/
├── Scenes/        Main.unity — the single playable scene
├── Scripts/       Gameplay C# scripts (see table above)
├── Prefabs/       Player, EnemyCube, BossCube, projectile, rooms
├── Materials/     Materials used by the level and entities
├── Editor/        Editor-only tooling
└── ThirdParty/    Third-party assets
```

## Built With

- **Unity 2022.3 LTS** — game engine
- **C#** — gameplay scripting

## Credits & Asset Licenses

The 3D art and animations in this project come from **KayKit** asset packs by
**Kay Lousberg** ([www.kaylousberg.com](https://www.kaylousberg.com)). They live under
[`Assets/ThirdParty/`](Assets/ThirdParty/).

| Asset pack | Used for | Author | License |
|------------|----------|--------|---------|
| KayKit Dungeon | Level / environment models and textures | Kay Lousberg | [CC0 1.0 (Public Domain)](https://creativecommons.org/publicdomain/zero/1.0/) |
| KayKit Character Pack: Skeletons (1.1 FREE) | Enemy / boss characters and animations | Kay Lousberg | [CC0 1.0 (Public Domain)](https://creativecommons.org/publicdomain/zero/1.0/) |

These assets are released under the **Creative Commons Zero (CC0 1.0)** license, which
means they are free to use in personal, educational and commercial projects. Crediting
the author is not mandatory, but is included here as a thank-you:

> 3D assets by **Kay Lousberg** — [www.kaylousberg.com](https://www.kaylousberg.com)

The original license text shipped with the Skeletons pack is preserved at
[`Assets/ThirdParty/KayKit_Skeletons_1.1_FREE/License.txt`](Assets/ThirdParty/KayKit_Skeletons_1.1_FREE/License.txt).

### Project code

All gameplay code under [`Assets/Scripts/`](Assets/Scripts/) was written for this project.
