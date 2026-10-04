# Applied Mathematics Activity 4 — Bézier Defense

This Unity 6 project is a code-generated tower-defense demonstration. It uses manual Bézier interpolation and transform-based movement without Unity physics, colliders, triggers, Rigidbody movement, raycasts, NavMesh, `Vector3.MoveTowards`, or built-in interpolation helpers.

Open `Assets/Scenes/Week4BezierDefense.unity` and press Play. This is the first scene in Build Settings.

### Implemented requirements

- Two spawn points lead to one shared base target.
- The cyan lane uses a quadratic Bézier curve with three control points.
- The pink lane uses a cubic Bézier curve with four control points and a double-arc path.
- Creatures move by evaluating the Bézier equations and assigning `transform.position`.
- The player controls no movable character; the towers defend the shared target from invading creatures.
- Flame, sniper, and shotgun towers fire transform-driven projectiles. One projectile hit kills a creature.
- The base starts with 20 HP. Each creature that reaches it removes 1 HP.
- The red HP bar updates immediately. The orange ghost layer pauses, then eases down with a manually implemented cubic ease-out function.
- A defeated creature creates a coin at its screen-projected death position. The coin follows a quadratic UI path to the bank.
- Each collected coin adds 10 to the bank. The displayed value counts through intermediate values using ease-out interpolation, while the bank panel punches upward and settles.
- The game displays either a wave-cleared panel or a base-destroyed panel.

The important code is located in:

- `Assets/Scripts/ManualMath.cs` — vectors, lerp, Bézier equations, easing, angles, and manual intersections
- `Assets/Scripts/BezierCreatures.cs` — two transform-based creature paths and spawners
- `Assets/Scripts/Week4Combat.cs` — transform-only towers, bullets, and swept creature-hit tests
- `Assets/Scripts/GhostHealthBar.cs` — immediate real HP and delayed eased ghost HP
- `Assets/Scripts/CoinBankUi.cs` — flying coins, eased count-up, and bank punch animation
- `Assets/Scripts/Week4GameManager.cs` — HP, creature resolution, and game state
- `Assets/Scripts/Week4LevelBootstrap.cs` — complete code-generated demonstration scene

### Gameplay video

Google Drive link: **TODO — replace this text with the Activity 4 gameplay video link.**

The video should show both creature paths, a creature damaging the base, the ghost HP animation, all three tower types killing creatures, a coin flying to the bank, the bank counting upward and punching, and the final game-state UI.

## Unity setup

1. Open this directory with Unity **6000.3.14f1** or a compatible Unity 6 release.
2. Open `Assets/Scenes/Week4BezierDefense.unity`.
3. Press Play.

If the empty scene asset needs to be recreated, use **Applied Mathematics > Build Week 4 Scene** from Unity's menu.

The runtime bootstrap scripts construct the levels, visuals, LineRenderers, UI, spawners, towers, and game managers without Inspector configuration.

VIDEO LINK
https://drive.google.com/file/d/1oWEbkb3PjvbCxq2t88Dtw99urCwpqEU2/view?usp=sharing
