# Open World Survival

*A zero‑asset, code‑it‑yourself journey into open‑world survival with Godot + C#*

![main image](/assets/main.png)

Hey, I’m Jimmy, and this repo is my sandbox for learning all things **Godot 4** and **C#** by building 
(and breaking, and rebuilding) a bite‑sized open‑world survival game. No marketplace shortcuts here—yours 
truly crafted every line of code, every pixel, every low‑poly tree swaying in the wind. My only rule: 
**if I didn’t model it, texture it, or script it myself, it doesn’t ship**.

### Why another survival game?

Because nothing stress‑tests your skills like juggling health, hunger, crafting, day–night cycles, and a 
cranky AI boar that really wants to yeet you off a cliff. It forces me to touch **physics, UI, audio, 
state machines, procedural worlds, saving/loading, and optimization**—all in one playground.

I've been learning from these people and more along the way:

- **Zenva’s Godot Academy** – solid foundations & mini‑projects.
- **CGDive** for all your blender rigging needs!
- **Grant Abbitt** – my low-poly blender guru.
- A sprinkle of other YouTube rabbit holes and docs.

But tutorials are just launchpads. I refactor, extend, and sometimes completely re‑invent the examples, 
so the final result is mine.

### What you’ll find here

```
Open-World-Survival/
├─ engine/                     # Low‑level, engine‑agnostic code
│   ├─ core/                   # Pure‑C# helpers (math, data‑structures, etc.)
│   └─ godot/                  # Thin wrappers around Godot types (e.g., Vector3 → MyVector)
│
├─ game/                       # Everything specific to *this* game
│   ├─ scenes/                 # Visual hierarchy
│   │   ├─ levels/             # Large playable worlds / test maps
│   │   ├─ modules/            # Re‑usable mini–scenes
│   │   │   └─ interactables/
│   │   │         └─ {crafting}/
│   │   │                crafting_table.tscn
│   │   │                CraftingTableInteraction.cs
│   │   │         ...        # Doors, campfires, chests, etc.
│   │   └─ ui/
│   │       └─ common/        # Buttons, bars, modal dialogs
│   │
│   ├─ scripts/                # Gameplay & domain logic
│   │   ├─ actors/             # Player, NPCs, behaviour trees
│   │   ├─ systems/            # CombatSystem, InventorySystem, QuestSystem...
│   │   ├─ data/               # Scriptable‑object‑style Resources (.tres/.res)
│   │   └─ misc/               # One‑offs / WIP scripts
│   │
│   ├─ resources/              # Pure data assets
│   │   ├─ art/
│   │   │   ├─ meshes/
│   │   │   ├─ textures/
│   │   │   └─ materials/
│   │   ├─ audio/
│   │   │   ├─ sfx/
│   │   │   └─ music/
│   │   └─ shaders/            # .gdshader + presets
│   │
│   └─ globals/                # Autoload singletons (Signals, Config, SaveManager)
│
└─ README.md                   # You are here 👋
```

> **Key philosophy**
> • *Engine and game stay divorced*—core utilities live in `engine/`, all Godot‑specific magic in `game/`.
> • Scenes stay modular: `modules/` for re‑usable interactables, `levels/` for big builds.
> • C# lives where it belongs: per‑system, per‑actor, or as pure data objects under `data/`.

### 🕹️ First‑Person Character Controller

![character movement](/assets/character-movement.gif)

No game is fun unless moving around *feels* amazing, so I built a bespoke first‑person controller. 
Here’s the highlight reel:

| Feature                          | What it does                                                               | Why it matters in‑game                                                                          |
| -------------------------------- | -------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| **Walk / Sprint**                | BaseSpeed 4 m/s → SprintSpeed 6 m/s                                        | Seamless speed shift with head‑bob & dynamic FOV for that “wind‑in‑your‑hair” vibe.             |
| **Jump & Slide‑Jump**            | Standard 4 m vertical pop; slide‑jump catapults you 1.5× higher & faster   | Lets you chain parkour moves and keep momentum like a pro speed‑runner.                         |
| **Crouch & Power‑Slide**         | Smooth capsule swap + animation; slide locks direction and decays over 1 s | Tight corridors? Duck in. Need flair? Hit Ctrl while sprinting and *Tokyo‑Drift* round corners. |
| **Dash (with cooldown)**         | 12 m/s burst, motion‑blur overlay, and audio sting                         | Emergency gap‑closer / oh‑no‑I‑aggro’d‑the‑bear button.                                         |
| **Free‑Look**                    | Hold Alt to decouple camera from aim                                       | Perfect for scouting or admiring your handcrafted world without messing up your heading.        |
| **Air Momentum**                 | Optional in‑air steering for arcade feel                                   | Keeps the controls snappy but still rewards good ground movement.                               |
| **State Machine Under the Hood** | `normal`, `sprinting`, `crouching`, `sliding`, `dash`, `slideJump`         | Clean separation of logic → easy to extend (wall‑runs? grappling hook?)                         |

**Tech bits**

* Pure C# on top of Godot 4’s `CharacterBody3D`
* Constant‑tuned physics values (see the top of `FirstPersonController.cs`) so teammates can 
* tweak speed/J‑curves without spelunking through code
* Camera FOV & head‑bob driven by AnimationPlayers for zero‑GC micro‑stutters
* Debug overlay (fps, state, velocity) baked in for quick iteration

### 🧠 AI System

![ai](/assets/ai.gif)

My goal was to build an enemy brain that’s **modular, tweak‑able, and fun to extend** — something I can keep bolting new behaviours onto without rewriting half the game. The result is a **finite‑state machine‑driven AI stack** that lives under `game/scripts/actors/ai/`.

| Core Piece                    | What it Does                                                                                                                                                         | Why It’s Cool                                                                                                                                                                                                                                 |
| ----------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`AiController.cs`**         | CharacterBody3D that owns movement, path‑finding (NavigationAgent3D), animation blending, player sensing & high‑level flags (running / stopped / looking at player). | Keeps *all* locomotion math in one spot, so states just say **“move here, run or walk”**.                                                                                                                                                     |
| **`AiStateMachine.cs`**       | Tiny FSM wrapper that registers every child `AiState`, handles Enter/Exit, and pumps Update / PhysicsUpdate each frame.                                              | Drop‑in new states as scene children → instant plug‑and‑play.                                                                                                                                                                                 |
| **`AiState.cs` (base class)** | Gives every state access to the controller, random helpers, and optional callbacks (navigation complete).                                                            | One inheritance point means less boilerplate for new behaviours.                                                                                                                                                                              |
| Concrete States               | **`Wander`**, **`Chase`**, **`Flee`** (more coming)                                                                                                                  | • **Wander**: picks random nav‑mesh spots around “home,” waits, roams.<br>• **Chase**: path‑updates toward player, swaps to *Flee* or *Wander* on distance checks.<br>• **Flee**: selects a safe point opposite the player, runs until clear. |
| Signals & Hooks               | `NavigationAgent3D.TargetReached → AiStateMachine.NavigationComplete()`                                                                                              | Lets path‑finding events drive behaviour changes without polling.                                                                                                                                                                             |

#### Why this design works

* **Single‑responsibility** — Movement math lives in *Controller*, decision logic in *States*; 
swapping animations or nav settings doesn’t touch AI logic.
* **Author‑friendly** — New behavior = `class Taunt : AiState { … }` + drag‑drop in the scene tree.
* **Cheap** — No coroutines or reflection; just a dictionary lookup and a few Booleans each frame.
* **Extensible** — Want a *Patrol* or *Attack* state? Copy, tweak, register in `AiStateMachine.DefaultState` or transition to it from another state.

> TL;DR: I now have NPCs that wander, chase, and flee **without** spaghetti code, and I can bolt on new tricks faster than you can say “behavior tree rewrite.”

### 🏹 Inventory & Item System

![drag and drop](/assets/item-drop.gif)

Your pockets are no longer bottomless chaos!
This C#‑driven inventory module keeps every log, ingot, and legendary sword neatly wrangled so the rest of the 
game can focus on *survival* instead of spreadsheet management.

| Piece                         | What it does                                                                                                                                                                                                                                                                                       | Why it matters                                                                                                                   |
| ----------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| **`Item` (Resource)**         | • Holds `Name`, `Icon`, `MaxStackSize`, optional `WorldItemScene`.<br>• Exposes `OnUse()` hook so each item can run bespoke logic.                                                                                                                                                                 | One self‑contained asset = zero hard‑coding. Drop new items into *Godot* and they “just work.”                                   |
| **`InventorySlot` (Control)** | • Visual button with icon + quantity text.<br>• Mouse‑over tooltip, left‑click use, right‑click drop.<br>• Smart stack/merge logic + empty‑slot cleanup.                                                                                                                                           | UI/UX polish out of the box—players instantly *feel* the slotiness.                                                              |
| **`Inventory` (Node)**        | • Owns a grid of `InventorySlot`s.<br>• Adds starter loot on `_Ready`.<br>• Listens for global **“item picked up”** signal.<br>• Opens/closes with `InputAction.Inventory` and auto‑releases the mouse.<br>• Exposes helpers (`AddItem`, `RemoveItem`, `GetNumberOfItems`, `ReorganizeInventory`). | Central brain that glues game‑world pickups to on‑screen slots while staying editor‑friendly (starter items, slot layout, etc.). |

#### Quality‑of‑Life Highlights

* **Drag‑n‑drop install** – add the three scripts, hook up a `Panel` + `SlotContainer` with `InventorySlot`s, 
and you’re done.
* **Auto‑stacking & auto‑cleanup** – no more “why do I have five half‑full stacks of wood?” headaches.
* **World‑drop integration** – right‑click tosses a physical `PackedScene` in front of the player, respecting 
orientation and distance.
* **Signal‑driven** – other systems just emit `OnItemPickedUp(Item, qty)`; inventory handles the rest.
* **Self-organizing system?** – already coded. Empty a slot and the grid compacts itself like magic.

#### Why it’s different

Most beginner tutorials hard‑wire item IDs or cram logic into UI scripts.
Your setup separates *data*, *UI*, and *gameplay*:

* Data lives in **Resources** (`.tres`) — easily editable, version‑controlled, and shareable.
* UI stays dumb; slots know almost nothing about the world.
* Gameplay flows through `Inventory` → `Item.OnUse()` → whoever actually needs to care (player, crafting table, etc.).

### 🖱️ Interaction System

![interactable object](/assets/interaction.gif)

Can't make a game without *interactable* objects!
Instead of hard‑wiring logic into every single scene, I used **composition‑over‑inheritance**:

| Piece                             | What it does                                                                                                                                                                                                                       | TL;DR                                             |
| --------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------- |
| `InteractableObject` *(abstract)* | Base class every interactive prop inherits from. Exported flags let me toggle **`CanInteract`** in the editor and set a custom **`InteractPrompt`** (“Mine Ore”, “Open Chest”, etc.). All you have to implement is `OnInteract()`. | Drop‑in template for new interactions.            |
| `InteractionController`           | A `RayCast3D` on the player’s camera. Every frame it: 1) shoots a ray, 2) sees if it hit something that extends `InteractableObject`, 3) shows the prompt, 4) calls `OnInteract()` when you smash the **Accept** key.              | Central brain that keeps the UI + gameplay gluey. |

#### Why I like this pattern

* **Unlimited extensibility** – Need a fishing spot? Just create `FishingSpot.cs` → override `OnInteract()` → done.
* **Zero coupling** – The player doesn’t know (or care) what it’s hitting as long as it talks the 
`InteractableObject` interface.
* **Editor‑friendly** – Designers (…me) can tweak prompts and toggle interactivity without touching code.

```csharp
public class CraftingTable : InteractableObject
{
    public override void OnInteract()
    {
        // Pop open the crafting UI, play that sweet thunk SFX, etc.
        CraftingSystem.Instance.OpenTable(this);
    }
}
```

Throw that script on any mesh, set the prompt to **“Craft Items”**, and it *just works*—no extra wiring. 
That’s the kind of DX (Developer eXperience) I live for.

## 🛠 Crafting System

![crafting](/assets/crafting.gif)

If the **Inventory** system is the warehouse, the **Crafting** system is the production line that turns raw wood, 
iron, and arcane goo into shiny swords (or at least into something you can whack goblins with).

| Piece                                                        | Where it lives           | What it does                                                                                                     |
| ------------------------------------------------------------ | ------------------------ | ---------------------------------------------------------------------------------------------------------------- |
| `Crafting` (`Crafting.cs`)                                   | `game/systems/crafting/` | A scene‑bound controller that owns the crafting UI, listens for open/close events, and actually **mints** items. |
| `CraftingRecipe` (`CraftingRecipe.cs`)                       | `Resources` (📝 `.tres`) | A lightweight `Resource` that says *“one {Item} costs X of A, Y of B.”*                                          |
| `CraftingRecipeRequirement` (`CraftingRecipeRequirement.cs`) | Same                     | A tiny helper resource for `{Item, Amount}` pairs inside a recipe.                                               |
| `CraftingRecipeUi` (`CraftingRecipeUi.cs`)                   | `ui/modules/`            | A reusable mini‑panel (icon, text, “Craft” button) that auto‑hides the button if you don’t have the parts.       |

### How it flows 🔄

1. **Boot‑up**
   `Crafting._Ready()` grabs the player `Inventory`, spawns one `CraftingRecipeUi` per recipe, wires them up, 
then hides the window until someone actually opens the station.

2. **Open / Close**
   Global signals `OnOpenCraftingMenu` & `OnCloseCraftingMenu` keep input nice and modal: mouse captured for 
gameplay, released for menus.

3. **Eligibility check**
   `CraftingRecipeUi.UpdateRecipe()` loops through each requirement and compares the player’s stash vs. the cost, 
toggling the **Craft** button accordingly and showing a neat *“3 / 5 Iron Ore”* read‑out.

4. **Craft time!**
   When you click **Craft**, `Crafting.Craft()` atomically:

    * Removes the required ingredients from the inventory
    * Adds the finished item
    * Refreshes all visible recipe cards so the UI never lies

5. **Extending**

    * Want a **Cooking Pot** or **Alchemy Table**?
      Just drop a new `Crafting` scene, give it a unique `CraftingTypeName`, and point its `Recipes` array to 
   fresh `CraftingRecipe` resources.
    * Requirements can stack arbitrarily (e.g., *2 × Iron Ingot + 1 × Leather Strip*).
    * Because recipes are plain `Resource`s, designers can author them in‑editor without touching code.

### Why I like this pattern 🚀

* **Data‑driven:** Recipes live outside code, so balancing costs is a two‑second tweak.
* **Reusable UI:** One panel prefab, infinite stations.
* **Zero magic strings:** Everything flows through typed resources & signals.
* **Player‑friendly:** Instantly see what you can and **can’t** build—no guess‑and‑check misery.
