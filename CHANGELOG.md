# Changelog
All notable changes to this package will be documented in this file.

## [0.1.0-preview.2] - 2022-09-23

### Assign Material to Hierarchy (Hierarchy View).

*When Drag And Drop Override is on, dropping a Material on a GameObject in the Hierarchy will Assign the Material to all Material IDs of all MeshRenderers in children.*

### Material Clipboard.

*Hold Control + Shift and Right click on a GameObject in the SceneView to copy its material in the Clipboard.*
*Then Hold Control + Shift and Middle click on a GameObject in the SceneView to assign the Clipboard Material.*
*In Unity 2021.2 or newer, the Clipboard Material will show as an Overlay. Click on the thumbnail to edit the Material.*

### Extract Materials & Meshes from Model/Prefab.

*Right click on a GameObject in the Hierarchy or a Prefab in the Project View and select Scene Dressing/Extract Materials/Meshes.*

### Ping Materials and Meshes.

*Hold Control and right click on a scene object to ping its Material, hold Shift to ping the Mesh.*

### New Mode: Replace within Scene root.

*Replace Material withing all children of a same Scene root.*

### Preferences.

*Behaviour can now be customized in Preferences.*

### Overlays.

*New SceneView Overlay allows accessing parameters from the toolbar.*

### Refactoring.

*Most features were re-organized in Types Extensions classes and Utility classes.*

## [0.1.0-preview.1] - 2022-09-13

### This is the first release of *Scene Dressing Tools*.

*Overrides the Drag and Drop behaviour when a Material is dropped on a GameObject in SceneView, to allow for propagating material to instances and applying changes to prefabs.*

