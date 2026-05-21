# Tower Ascent

A small 2D top-down action-adventure built for **Unity 2022.3.62f1** (stock Unity LTS).
Climb an **eight-floor** tower: one boss per floor, each drops a relic and a key.

See [`Game Description`](Game%20Description) for the full design write-up and
[`Plan.jpg`](Plan.jpg) for the original hand-drawn concept.

## Opening the project

1. Install **Unity 2022.3.62f1** via Unity Hub (any Unity 2022.3 LTS patch version
   actually works; Hub will offer to upgrade the project on first open).
2. In Unity Hub, click **Add → Add project from disk** and select the repository root
   (`d:\work\code\Game\Games-Programming`).
3. Open the project. Unity will import all packages and regenerate `Library/`,
   `Temp/`, `.csproj`, `.sln`, etc. (all of these are git-ignored).
4. Open `Assets/Scenes/MainMenu.unity`.
5. Press **Play**. Click **Enter the Tower**.

> All GameObjects are built at runtime by
> [`Assets/Scripts/Core/Bootstrap.cs`](Assets/Scripts/Core/Bootstrap.cs).

## Controls (twin-stick)

| Input | Action |
| --- | --- |
| `WASD` / Arrow keys | Move |
| Mouse | Aim |
| **Left mouse** | Melee (hold OK, no stamina) |
| **Right mouse** | Ranged bolt (hold OK, costs stamina) |
| `F3` | Stamina potion (after Floor 1, ~4 s cooldown) |
| `Space` | Dash (after Floor 4, costs stamina) |
| `Esc` | Pause |

Melee and ranged use **separate cooldowns** — you can alternate or hold both for aggressive kiting.

## Tower layout (8 bosses)

| Floor | Arena | Boss | Reward |
| --- | --- | --- | --- |
| 1 | Slime Pit | Slime King | Stamina Potion |
| 2 | Bone Hall | Skeleton Lord | Iron Sword |
| 3 | Ember Vault | Fire Warden | Firebolt Rune |
| 4 | Shadow Deck | Shadow Knight | Dash Boots |
| 5 | Storm Gallery | **Storm Titan** | Vitality Charm |
| 6 | Frost Sanctum | **Frost Matriarch** | Grit Band |
| 7 | Iron Foundry | **Iron Colossus** | Predator Charm |
| 8 | Lich Summit | Tower Lich | Tower Amulet — **clear** |

Pick up **key + relic**, then enter the **yellow portal**.

## Project layout

```
Assets/Scripts/
├─ Core/        GameManager, TowerManager, Bootstrap, FloorConfigSO
├─ Player/      PlayerController, PlayerStats, PlayerCombat, PlayerInventory, PlayerFactory
├─ Combat/      Damageable, Projectile
├─ Enemies/     EnemyBase, BossBase, BossFactory, Bosses (eight archetypes)
├─ Items/       ItemSO, ItemLibrary, ItemDrop, FloorExit
├─ UI/          HUD, InventoryUI, Minimap, FloorIndicator, BossHealthBar, menus, Crosshair
└─ Utilities/   SpriteFactory
```

## Placeholder art

`SpriteFactory` generates sprites at runtime. Swap in PNGs under `Assets/Art/` when ready.

## Notes

- Single scene `MainMenu.unity`; no scene load between floors.
- Legacy `Input` (not the new Input System).
- No disk save; returning to the menu resets the run.
