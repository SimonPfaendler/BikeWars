# BikeWars

**BikeWars** is an action-packed, "Vampire Survivors-like" rogue-lite top-down game developed in C# with the MonoGame framework. Face off against waves of enemies, manage chaotic combat, and try to survive as long as you can whether you're on a bike or on foot!


<div align="center">

  <video src="https://github.com/user-attachments/assets/6d6ea56c-393c-499c-95c3-b4fb973c09a3" width="700" autoplay loop muted playsinline></video>

  <br><br>

</div>

This project was built as a software traineeship project by a student group using Scrum and Agile development methodologies.

## Features
- **Rogue-lite Action:** Survive against endless waves of enemies (like Police Officers and Cars). Collect XP to activate the in-game Level-Up system.
- **Weapon Arsenal:** Fight back using a variety of projectiles like Bananas, Bottles, and Guns—each with dynamic weapon statistics.
- **Dynamic Movement:** Switch seamlessly between walking and riding a bike, complete with full 360-degree gaze direction and varied movement logic.
- **Game Modes:** 
  - **Singleplayer:** Survive on your own.
  - **Local Multiplayer:** Team up with a friend in 2-player co-op! The game continues until both players are taken down.
- **Custom Physics:** Includes custom `TerrainCollider` and `SpatialHash` engine functionalities to handle efficient collision detection and environment boundaries.

## Technology Stack
- **Language:** C# (.NET 8.0)
- **Engine Framework:** [MonoGame](https://www.monogame.net/) (DesktopGL)
- **Additional Libraries:** MonoGame.Extended (v5.2.0)
- **Architecture:** Custom Game Engine approach (Managers for GameObjects, Collisions, Spawning, Combat, etc.)

## How to Run (Locally)
1. Ensure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed on your machine.
2. Clone the repository.
3. Once pulled, open your terminal (or command prompt) in the project root containing `BikeWars.csproj`.
4. Restore dependencies and run the game:
   ```bash
   dotnet restore
   dotnet run
   ```
*(Note: If you're building the project for the first time, dotnet tools for MonoGame content compilation might be restored automatically).*

## Development & Methodology
This game was built from scratch inside an academic software traineeship program. We focused heavily on clean code and robust architecture, including:
- **Agile/Scrum Workflow:** Sprints, backlog management, and continuous feature integration.
- **Code Quality Tools:** SonarQube was utilized to continuously enforce code standards and reduce technical debt.
- **Collaborative Engineering:** Handled complex merges, such as unifying the collision logic and decoupling game mode states for multiplayer integration.

## Contributors
- Developed by the **sopra01** agile student group.
