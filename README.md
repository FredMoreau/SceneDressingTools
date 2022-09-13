# Scene Dressing Tools
A collection of *Editor Tools* making it easier to perform common scene dressing tasks.

**Installation:**
Clone the repo and use "Add package from disk..." from the *Package Manager*.
Or use "Add package from git URL..." and paste the url https://github.com/FredMoreau/SceneDressingTools.git

## Features

### Material SceneView Drag and Drop
Hold **Control (Command)** while dragging a *Material* from the *Project Browser* onto a *MeshRenderer* in the *SceneView*. This will bring a menu with **more *Material* assignment options**.

#### Assign Material
This has the same effect as dropping a material on an object.

#### Propagate to *Mesh* instances
Assigns the material to all *MeshRenderer* using the same mesh instance. This is useful to ensure all instances of a *Mesh* are using the same *Material*.

#### Propagate to meshes using *Material*
Assigns the material to all *MeshRenderer* using the same *Material*.
This is useful to replace a *Material* throughout the scene.

#### Propagate to *Mesh* instances using *Material*
Assigns the material to all *MeshRenderer* using the same mesh instance and *Material*. This is useful to ensure all instances of a *Mesh* already using a specific *Material* are updated.

#### Copy *Material* properties over *Material*
Copies the material properties from source over to target. This is useful when you want to copy materials from a Library onto an imported object with material placeholders.
**Note**: if the material you copy properties from is a *Material Variant*, the result will not inherit from the original *Material*'s *Parent*.

#### Assign a copy of *Material*
Creates a copy of the *Material* and assign the copy so that you can edit it freely without touching the original. This is useful if you want to edit the *Material* right after.
**Note**: if the material is a *Material Variant*, the result will inherit from the original *Material*'s *Parent*.

#### Assign a Variant of *Material*
Creates and assigns a *Variant* of the *Material* so that you can edit it while inheriting from the original. This is useful if you want to edit the *Material* right after.

#### Assign and Apply to Prefab
Sub Menu showing the *Nested Prefab* hierarchy.
Assigns the *Material* and apply the changes to the selected *Prefab*. This is useful to automatically propagate *Material* changes to all other *Prefab* instances.
