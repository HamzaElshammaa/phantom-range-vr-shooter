<div align="center">

# Phantom Range — VR Tactical Shooting & Training Simulation 🎯🥽

### **Immersive 6DoF Virtual Reality Shooting Range Built with Unity & XR Interaction Toolkit**

[![Unity](https://img.shields.io/badge/Unity-VR_Engine-000000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-Scripting-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![XR Interaction Toolkit](https://img.shields.io/badge/XR_Interaction_Toolkit-OpenXR-FF6B6B?style=for-the-badge)](#)

*A Virtual Reality tactical shooting range and combat training simulation featuring 6DoF grab mechanics, physical projectile ballistics, dynamic target courses, and spatial audio feedback.*

</div>

---

## ✨ Key VR Features

- 🔫 **6DoF Weapon Handling & Ballistics:** Realistic two-handed and single-handed XR grab interactables, trigger-driven firing mechanics, muzzle flash VFX, and recoil feedback.
- 🎯 **Dynamic Training & Combat Scenarios:** Multi-zone target ranges with stationary, moving, and reactive targets that track player accuracy and reaction speed.
- 🕹️ **Comfort-First VR Locomotion:** Supports both continuous thumbstick locomotion and snap-turn / teleportation providers via Unity's **XR Interaction Toolkit** and **OpenXR**.
- 📄 **Design & Research Documentation (`docs/`):** Includes the complete *Phantom Range VR Project* specification (`PhantomRange_VR_Project.docx`), practical task breakdown, and Mixed/Virtual Reality research report.

---

## 📂 Repository Structure

```text
├── Assets/                 # Scenes, VR weapon prefabs, C# gameplay scripts, audio & materials
├── Packages/               # Unity Package Manager dependencies (XR Interaction Toolkit, OpenXR)
├── ProjectSettings/        # XR Plug-in Management, physics layers & input action bindings
└── docs/                   # Phantom Range design document, practical task & XR research paper
```

---

## 🚀 Getting Started

1. Clone this repository and add the folder in **Unity Hub**.
2. Open the main training range scene under `Assets/Scenes/`.
3. Connect an OpenXR-compatible headset (Meta Quest 2/3/Pro via Quest Link, SteamVR, or use the in-editor **XR Device Simulator**).
4. Press **Play** in the Unity Editor or build an Android APK for standalone Meta Quest deployment.
