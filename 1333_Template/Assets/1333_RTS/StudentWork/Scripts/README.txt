A1_Grid and Pathfinding_Brief

At this stage of the project I came up with all required features so far. 

For pathfinding I altered my previous bruteforce system to a*, currently all units are using a* pathfinder as default defined in UnitBase class. 

On start ( actually on awake) GameManager  runs through ArmyPathfindingTester and initializes 2 seperate armies 1 for player and 1 for npc. 

Each armies there have their own material representation coming from UnitInstance. 

Added UnitHotkeySpawner to manually spawn units from UnitTypePrefab on desired team, I have 4 seperate units, made me use  keys 1-4 to spawn each. Tab key switches between armies (npc-player)

All spawned units  with SelectionManager are selectable (left mouse click) by single selection mode or multiple by dragging a rect and right mouse click calls command set destination to move these units.

After switching from A-B point automatic pathfinding to command base pathfinding I found initializing a path drawing in UnitInstance as a better option, I dont know if it is a good choice?

That's it, Thanks!


 

