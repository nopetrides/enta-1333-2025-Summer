# Grid base pathfinding with units DEMO

# Key features

- Camera pan/zoom over a 2D/3D grid

- A* algorithm for unit navigation

- ScriptableObjects for unit types

- Army grouping (player vs. enemy) with distinct materials

- Point(Drag)-and-click selection and movement commands

- Debug toggles (path visualization)

- Runtime unit spawning via hotkeys

# Controls & Interactions
1. Camera Controls
Pan:
W / A / S / D keys or Middle mouse button and drag

Zoom:
Mouse scroll wheel (scroll up to zoom in, scroll down to zoom out)

Vetical Zoom:
F / R Keys

Rotate:
Q / E Keys

2. Unit Selection & Movement
Select Unit:
Left-click or drag on units that belongs to your army

Move Command:
Right-click on a target grid cell → selected unit computes a path and moves along it

Cancel Selection:

Left click on empty space (or other units)

3. Debug Toggles

Press X to show/hide full A* pathfinding visualization(Default is hided)
Press H to kill all units in game scene

4. Runtime Spawning
BackQuote (`)
Spawn all enemy units (uses _enemyArmySO, Team = Enemy)

1 (Alpha1)
Spawn all player units (uses _playerArmySO, Team = Player)

2 (Alpha2)
Spawn Player Spearman army (uses _spearManArmySO, Team = Player)

3 (Alpha3)
Spawn Player Mounted Knight army (uses _mountedKnightArmySO, Team = Player)

4 (Alpha4)
Spawn Player Worker army (uses _workerArmySO, Team = Player)

5 (Alpha5)
Spawn Player Mounted High Mage army (uses _mountedHighMageArmySO, Team = Player)

6 (Alpha6)
Spawn Player Archer army (uses _archerArmySO, Team = Player)

7 (Alpha7)
Spawn Player Crossbowman army (uses _crossbowManArmySO, Team = Player)

8 (Alpha8)
Spawn Player Commander army (uses _commanderArmySO, Team = Player)

9 (Alpha9)
Spawn Player Mage army (uses _mageArmySO, Team = Player)

0 (Alpha0)
Spawn Player High Mage army (uses _highMageArmySO, Team = Player)

# ScriptableObjects
1. ArmyComposition

- AC_Archer
- AC_Commander
- AC_CrossbowMan
- AC_Enemy
- AC_HighMage
- AC_Mage
- AC_MountedHighMage
- AC_MountedKnight
- AC_Player
- AC_SpearMan
- AC_Worker

2. UnitTypes

- Archer
- Commander
- CrossbowMan
- HighMage
- Mage
- Mounted_HighMage
- Mounted_Knight
- SpearMan
- Worker