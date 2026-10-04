# Applied Mathematics Activities 3 and 4

This Unity 6 project contains two connected assignments. Both scenes generate their gameplay objects from code and avoid Unity physics, colliders, triggers, Rigidbody movement, raycasts, NavMesh, and built-in interpolation helpers.

## Activity 4 — Bézier Defense

Open `Assets/Scenes/Week4BezierDefense.unity` and press Play. This is the first scene in Build Settings.

### Implemented requirements

- Two spawn points lead to one shared base target.
- The cyan lane uses a quadratic Bézier curve with three control points.
- The pink lane uses a cubic Bézier curve with four control points and a double-arc path.
- Creatures move by evaluating the Bézier equations and assigning `transform.position`.
- The movable Week 3 player is absent. The towers now defend the shared target from creatures.
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

### Activity 4 gameplay video

Google Drive link: **TODO — replace this text with the Activity 4 gameplay video link.**

The video should show both creature paths, a creature damaging the base, the ghost HP animation, all three tower types killing creatures, a coin flying to the bank, the bank counting upward and punching, and the final game-state UI.

## Activity 3 — Manual Turret Defense

Open `Assets/Scenes/Week3TurretDefense.unity` and press Play.

- Move with **WASD** or the **arrow keys**.
- Reach the green goal on the right.
- A projectile hit reloads the scene immediately.
- The flame turret uses a manual cone check.
- The sniper uses a manually calculated sight line.
- The shotgun uses a manual cone and calculated pellet spread.

### Activity 3 gameplay video

Google Drive link: **TODO — replace this text with the Activity 3 gameplay video link.**

## Unity setup

1. Open this directory with Unity **6000.3.14f1** or a compatible Unity 6 release.
2. Open the scene for the activity being demonstrated.
3. Press Play.

If either empty scene asset needs to be recreated, use Unity's menu:

- **Applied Mathematics > Build Week 3 Scene**
- **Applied Mathematics > Build Week 4 Scene**

The runtime bootstrap scripts construct the levels, visuals, LineRenderers, UI, spawners, towers, and game managers without Inspector configuration.
