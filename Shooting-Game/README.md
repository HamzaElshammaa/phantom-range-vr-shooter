# VR Shooting Game Project

A comprehensive VR shooting experience built with Unity and the XR Interaction Toolkit. This project features multiple scenes with different gameplay mechanics, interactive weapons, scoring systems, and tutorial modes.

## 📋 Project Overview

This VR shooting game provides an immersive training and combat experience with realistic weapon mechanics, scoring systems, and educational content. The project leverages Unity's XR Interaction Toolkit to deliver smooth VR interactions using controllers.

## 🎮 Scenes

### 1. **Training Range** (`TrainingRange.unity`)

The primary training environment where players practice their shooting skills with progressive challenges.

**Features:**

- **Target Shooting**: Multiple targets to practice accuracy
- **Progressive Challenges**: Hit-based goals that must be met to progress
- **Quiz System**: Educational quiz questions after completing challenges
- **Score Tracking**: Real-time score and accuracy statistics
- **Scoreboard UI**: Visual feedback on performance metrics
- **Scene Transitions**: Teleport-based exit mechanics

**Interactions:**

- Grab and fire the weapon using VR controllers
- Aim at targets and shoot for accuracy
- Complete hit goals to unlock scene transitions
- Answer quiz questions to validate learning
- Teleport to exit the scene

---

### 2. **Combat Zone** (`CombatZone.unity`)

A more intense combat-focused environment for advanced players.

**Features:**

- **Combat Scenarios**: Dynamic enemy engagement
- **Weapon Handling**: Advanced weapon mechanics
- **HUD Display**: Real-time combat statistics
- **Audio Atmosphere**: Immersive sound design

---

## 🎯 Key Interactions

### Weapon System (`GrabbableGunFire.cs`)

- **Grab Interaction**: Pick up the gun using XR grab interactable
- **Firing Mechanism**: Press trigger (VR controller) to fire
- **Hitscan Shots**: Instant hit detection with raycast
- **Fire Cooldown**: Balanced firing rate (0.15s default)
- **Audio Feedback**: Gun fire sound effects
- **Muzzle Origin**: Accurate shot origin from gun muzzle

**Usage:**

1. Grab the gun with your VR controller
2. Aim using natural controller movement
3. Pull the trigger to fire
4. Receive instant hit feedback

---

### Target System (`ScoreTarget.cs`, `TargetHit.cs`)

- **Hitbox Detection**: Targets register hits from projectiles
- **Visual Feedback**: Respond to successful hits
- **Score Integration**: Contribute to overall score
- **Hit Validation**: Track accurate shots for scoring

---

### Training Challenge (`TrainingRangeChallenge.cs`)

- **Hit Goals**: Set number of successful hits required to pass
- **Accuracy Tracking**: Monitor shot accuracy percentage
- **Performance Statistics**: Display shots fired vs. hits
- **Quiz Validation**: Multiple-choice questions on shooting safety
- **Scene Unlock**: Complete challenge to unlock exit

**Challenge Flow:**

1. Shooting phase: Meet the hit goal
2. Quiz phase: Answer training questions
3. Completion: Unlock scene exit

---

### UI System (`GameHUD.cs`)

- **Score Display**: Real-time score counter
- **Timer Display**: Session/challenge timer
- **VR UI Interaction**: UI readable from VR perspective
- **TextMesh Pro Integration**: High-quality dynamic text rendering

---

### Scene Navigation (`TeleportSceneTransitionHook.cs`)

- **Teleport Mechanics**: Move between scenes seamlessly
- **Scene Loading**: Fluid scene transitions
- **Exit Planes**: Designated teleport zones

---

### Visual Feedback (`HoverGlow.cs`)

- **Interactive Highlighting**: Glow effects on grabbable objects
- **Visual Cues**: Indicate interactive elements
- **Enhanced UX**: Improve player engagement

---

## 🎮 Prefabs

### **GrabbableGun.prefab**

The main weapon prefab with:

- XR Grab Interactable component
- Firing mechanism
- Audio source for gun sounds
- Visual muzzle point

### **Bullet.prefab**

Visual representation for projectiles (if using instantiated bullets)

### **TrainingScoreboardUI.prefab**

Reusable UI scoreboard for displaying:

- Current score
- Hits percentage
- Timer countdown
- Challenge progress

### **TrainingTeleportExitPlane.prefab**

Scene exit teleport zone with collision detection

---

## 🛠️ Core Systems

### Audio System (`IntroSceneLoopAudio.cs`)

- Background music and ambience
- Gun fire sound effects
- Scene-specific audio management

### Collision Handling (`XRSimulatorCollision.cs`)

- VR simulator collision detection
- Physical interaction feedback
- Safe testing environment

### Tutorial System (`TutorialManager.cs`)

- Interactive tutorials
- On-screen guidance
- Learning progression

### Weapon Socket System (`WeaponSocketHandler.cs`)

- Socket attachment for weapons
- Hand-to-grip positioning
- Secure weapon holding

---

## 🚀 Getting Started

### Prerequisites

- Unity 2022.3 LTS or higher
- XR Interaction Toolkit package
- VR headset and controllers (or use VR Simulator)
- TextMesh Pro (auto-imported)

### Setup Instructions

1. **Open the Project**
    - Launch Unity and open the Shooting-Game folder
    - Let Unity import all assets

2. **Select a Scene**
    - Open `Assets/Scenes/TrainingRange.unity` to start with the training environment
    - Or try `Assets/Scenes/CombatZone.unity` for combat

3. **Configure VR Settings**
    - Build Settings: Choose your target VR platform
    - XR Plug-in Management: Enable your VR device (Meta, SteamVR, etc.)

4. **Play in Editor**
    - Use VR Simulator mode to test with keyboard/mouse
    - Press Play to start the scene

---

## 🎓 Educational Features

- **Safety Quiz**: Learn proper shooting range protocols
- **Performance Feedback**: Track accuracy and improvement
- **Progressive Learning**: Master basics before advanced challenges
- **Interactive Training**: Hands-on learning experience

---

## 🎨 Project Structure

```
Assets/
├── Scenes/                      # Scene files
│   ├── TrainingRange.unity     # Main training environment
│   ├── CombatZone.unity        # Combat scenario
│   └── SampleScene.unity       # General testing scene
├── Scripts/                    # C# gameplay scripts
│   ├── GrabbableGunFire.cs    # Weapon firing system
│   ├── ScoreTarget.cs         # Target hit detection
│   ├── TrainingRangeChallenge.cs # Challenge logic
│   ├── GameHUD.cs             # UI display system
│   ├── TutorialManager.cs     # Tutorial system
│   └── ...
├── Prefabs/                    # Reusable GameObjects
│   ├── GrabbableGun.prefab    # Main weapon
│   ├── Bullet.prefab          # Projectile
│   └── TrainingScoreboardUI.prefab
├── Sounds/                     # Audio assets
├── Animations/                 # Animation clips
└── Settings/                   # Project configuration
```

---

## 🎮 Gameplay Tips

1. **Accuracy Matters**: Focus on precision over speed
2. **Watch Your Timer**: Many challenges are time-based
3. **Use Natural Aiming**: Move the controller naturally to aim
4. **Trigger Control**: Smooth trigger pulls for better accuracy
5. **Answer Correctly**: Quiz answers affect your final score

---

## 🐛 Troubleshooting

| Issue                       | Solution                                                |
| --------------------------- | ------------------------------------------------------- |
| Gun won't fire              | Ensure XR controller is connected and trigger mapped    |
| Targets don't register hits | Check LayerMask settings in GrabbableGunFire            |
| Quiz doesn't appear         | Verify TrainingRangeChallenge component exists in scene |
| Scene won't transition      | Ensure TeleportSceneTransitionHook is set up correctly  |
| UI text hard to read        | Increase Canvas scale or move closer in VR              |

---

## 📦 Dependencies

- **XR Interaction Toolkit**: Core VR interaction system
- **TextMesh Pro**: Advanced UI text rendering
- **Input System**: Modern input handling for VR controllers
- **XR Management**: VR device management

---
