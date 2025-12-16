# 🎮 Tick-Tack Multiplayer (Unity 2D)

A **2D multiplayer Tic-Tac-Toe style game** built in **Unity** using **Photon PUN (Photon Unity Networking)** for real-time online gameplay.

This project was created to explore **multiplayer game development concepts**, including room creation, player synchronization, and turn-based gameplay over the network.

---

## 🧩 Project Overview

- **Engine:** Unity
- **Game Type:** 2D Multiplayer
- **Networking:** Photon PUN
- **Platform:** PC (can be extended to Android/Web)
- **Game Mode:** Online Multiplayer (2 Players)

Players connect online, join a room, and play against each other in real time on the same grid.

---

## 🚀 Features

- Online multiplayer using **Photon PUN**
- Room creation and joining
- Turn-based gameplay logic
- Grid-based board interaction
- Simple and clean 2D UI
- Lightweight and easy-to-extend project structure

---

## 🕹️ Gameplay Flow

1. Player launches the game
2. Connects to Photon server
3. Creates or joins a room
4. Game board is initialized
5. Players take turns tapping on grid cells
6. Moves are sent over the network using Photon events

---

## ⚠️ Known Issue (Sync Problem)

> The main unresolved issue in this project is **input synchronization**.

- Tap/click events were **not always synchronized correctly** between both players
- In some cases, a grid cell tap was registered locally but **not reflected on the other client**
- This issue is related to **Photon state synchronization and RPC timing**

This project highlights a common multiplayer challenge and serves as a learning experience for handling **networked input and authoritative game state management**.

---

## 🛠️ Technologies Used

- Unity (2D)
- C#
- Photon PUN
- RPCs & Network Events
- Unity UI System

---

## 📂 Project Structure

```text
Assets/
 ├── Photon/
 ├── Scenes/
 ├── Scripts/
 ├── Prefabs/
 └── UI/
Packages/
ProjectSettings/

##  Possible Improvements

Implement authoritative turn control

Sync grid state using buffered RPCs

Use Photon Custom Properties for game state

Improve input validation and conflict handling

Add reconnection handling

Enhance UI/UX and animations

🧠 Learning Outcomes

Hands-on experience with Photon PUN

Understanding real-time multiplayer synchronization

Managing turn-based multiplayer logic

Debugging common networking issues in Unity

📌 Note

This project is not production-ready and was developed for learning and experimentation purposes.

👤 Author

Shammas-ul-Islam Satti

GitHub: https://github.com/Shammas-satti-00

LinkedIn: https://www.linkedin.com/in/shammas-ul-islam-660895275
