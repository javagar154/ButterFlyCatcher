# Realistic Butterfly Catcher — 3D Unity Game

A realistic, physics-driven 3D butterfly catching game built in **Unity 6 (6000.6.3f1)**.

![Unity 6](https://img.shields.io/badge/Engine-Unity_6_(6000.6.3f1)-blue.svg)
![Validation](https://img.shields.io/badge/Validation-33%2F33_Tests_Passed-brightgreen.svg)
![Zero Dependencies](https://img.shields.io/badge/Assets-100%25_Procedural-orange.svg)

---

## 🎮 Game Overview

Step into the shoes of a young explorer in a sunny 3D garden environment equipped with a butterfly net. Your mission: catch as many butterflies as you can before the 60-second countdown timer expires!

### Key Features
- **3D Garden Environment**: Lush green lawns, cobblestone winding pathways, flower beds with colored blossoms, perimeter wooden fences, trees, and soft golden-hour lighting.
- **Boy Explorer Character**: Third-person articulated character rig with dynamic leg strides, arm counter-swings, and an articulated net attached to the right hand.
- **5 Realistic Butterfly Species**:
  1. 🟠 **Monarch** (Vibrant Orange & Black)
  2. 🔵 **Blue Morpho** (Iridescent Electric Blue)
  3. 🟢 **Emerald Swallowtail** (Shimmering Emerald Green)
  4. 🟡 **Sulphur Yellow** (Bright Lemon Yellow)
  5. 🔴 **Crimson Rose** (Deep Ruby Crimson)
- **Autonomous Flight AI**: 3D multi-octave Perlin noise drift, continuous sinusoidal wing flapping (12–18 Hz), player avoidance evasion steering, and flower perching.
- **Spatial Catching Mechanics**: Accurate spherical radius (2.4m) and forward cone dot-product angle checks (`Vector3.Dot`) during the active swing window.
- **Feedback & Audio**: Dual-tone bell chimes, net swooshes, particle catch bursts, and world-space `+10` floating text popups.
- **Game Loop**: 60-second countdown timer, real-time score display, start menu, pause menu, and game over screen with restart functionality.
- **100% Automated Test Suite**: 33 unit/integration assertions passing in batchmode with zero errors.

---

## 🕹️ Controls

| Action | Controls |
| :--- | :--- |
| **Move Explorer** | `W`, `A`, `S`, `D` or `Arrow Keys` |
| **Sprint** | Hold `Left Shift` |
| **Swing Butterfly Net** | `Left Mouse Button` or `Spacebar` |
| **Orbit Camera** | Move `Mouse` |
| **Pause / Resume** | `Escape` or `P` |
| **Restart Game** | Click **Restart** button on Game Over or Pause screen |

---

## 🚀 How to Run in Unity Editor

1. Open **Unity Hub**.
2. Click **Open** and select this directory: `c:\Users\Javagar\OneDrive\Desktop\Butterfly`.
3. Open scene: `Assets/Scenes/GardenScene.unity`.
4. Click the **Play Button** (or press `Ctrl + P`).

---

## 🧪 Automated Testing

To run the automated validation suite from terminal:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.com" -batchmode -quit -projectPath "c:\Users\Javagar\OneDrive\Desktop\Butterfly" -executeMethod GameValidator.ValidateGamePlay -logFile "c:\Users\Javagar\OneDrive\Desktop\Butterfly\test_log.txt"
```

Result: `TEST RESULTS: 33 / 33 TESTS PASSED (100%)`.

---

## 📁 Project Structure

```text
Assets/
├── Editor/
│   ├── GameValidator.cs        # Automated batchmode verification suite (33 assertions)
│   └── SetupButterflyGame.cs   # Procedural asset & scene generator
├── Scenes/
│   └── GardenScene.unity       # Complete pre-configured gameplay scene
└── Scripts/
    ├── ButterflyAI.cs          # Multi-octave Perlin flight AI, wing flapping & evasion
    ├── ButterflyCatcher.cs     # Spatial cone net catching detection
    ├── ButterflySpawner.cs     # Multi-species dynamic population spawner
    ├── CameraFollow.cs         # Smooth 3rd-person camera with collision avoidance
    ├── CharacterAnimator.cs    # Procedural limb kinematics & net swinging
    ├── FloatingScoreText.cs    # 3D floating popup text with billboarding
    ├── GameManager.cs          # Finite state machine (Menu, Playing, Paused, GameOver)
    ├── GardenAtmosphere.cs     # Environmental lighting & airborne pollen particles
    ├── PlayerMovement.cs       # CharacterController locomotion & gravity
    ├── ScoreManager.cs         # Scoring (+10/catch), caught counts, & PlayerPrefs
    ├── SoundManager.cs         # Procedural PCM audio synthesis (chimes, whooshes)
    └── UIManager.cs            # HUD score, timer, menus, & game over display
```
