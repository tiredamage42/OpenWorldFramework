# OpenWorldFramework
A framework for open world style scene streaming in Unity


* Requires the Unity Tools Module (included in repository).

***
### SETUP:

the entire world is split up into scenes that represent every cell in the grid
for each (A)x(A) area.  where A is the world unit cell size specified in the settings object

for each cell, there are two scenes, LOD0 and LOD1. to load varying detail based on distance

***
### SCENE SETTINGS SCENE:

As a base for the entire world, there is a Settings Scene that is always loaded,
    named: __OpenWorldSettingsScene

this scene should always be the Active scene, as it will be used for lighting settings / fog / etc...
since other cell scenes will be loaded and unloaded additively, if we rely on settings for those, they will be lost
and this way we only have to set scene settings for one scene as opposed to trying to keep
all the cell scenes' settings consistent

directional lights / weather systems or anything of that sort that should persist throughout the entire world
should be kept in that scene

***
### SCENE NAMING CONVENTION:

for each cell in teh world, the scene name will be as follows:

(X) _ (Y) @ LOD (I) @OW_ (WorldScene)

e.g.: 
    0_1@LOD0@OW_Townsville

where: 
    (X) _ (Y) is the cell for the scene
    (I) is the LOD level for that scene, either 0 or 1
    (WorldScene) is the name of the cell
    
    note: if you want to change the name of the cell make sure to keep everything before ' @OW_ ' the way it is.
    also remember to change both lod versions' names

***
### RUNTIME:

when at a distance, LOD1 is loaded (terrain, big landmark objects)
when closer, LOD0 is loaded as well (items, navmesh stuff, etc...) 

grids around the calculated player cell are loaded and unloaded based on teh distance
the current player cell is set to the cell where the camera is currently

***
### NOTE:

-all scenes created are added to the build settings for the project automatically
-system currently only works for positive cells: (0,0) and above
