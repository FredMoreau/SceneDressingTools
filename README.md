# Scene Dressing Tools
 A collection of *Editor Tools* making it easier to perform common scene dressing tasks.

### Install through the Package Manager
1. Go to Window/Package Manager.
2. Click on the + icon and select "Add package from git URL...".
2. Copy/paste the following url in the field and click on Add. (This requires Git installed)
- git@github.com:FredMoreau/SceneDressingTools.git
- You can also download the repo as a Zip file, unzip it somewhere like C://Unity_Custom_Packages/, then from the Package Manager select Add package from disk, and browse for the package.json file.

## Features
### Material SceneView Drag and Drop
In the **Preferences** go to the *Scene Dressing/Materials* and enable *Drag 'n' Drop Behaviour Override*. Then set the different modes.
In Unity 2021.2 and above, you can also display the Scene Dressing Overlay in Scene View for a direct access.

#### Assignment Mode

##### Assign Original
This is the default behaviour and simply assigns the dropped *Material*.

##### Assign Duplicate
This assigns a duplicate of the dropped *Material* for later editing. This is useful if you want to edit the *Material* right after.
**Note**: if the material is a *Material Variant*, the result will inherit from the original *Material*'s *Parent*.

##### Assign Variant (Unity 2022.1 and above)
Same as Duplicate, but creates a *Material Variant* instead.

##### Copy Properties
This doesn't assign a *Material*, but copies the dropped *Material*'s properties onto the target Material instead. This is useful when you want to copy materials from a Library onto an imported object with material placeholders.
**Note**: if the material you copy properties from is a *Material Variant*, the result will not inherit from the original *Material*'s *Parent*.

![preview](/Documentation~/images/copy_properties_over_target.gif)

#### GameObject Mode
Sets the behaviour when dropping a *Material* onto a *GameObject* (not a *Prefab* instance).

##### Assign
This has the same effect as dropping a material on an object.

##### Popup Menu
Brings a *Context Menu* to choose each time.

##### Assign to all *Mesh* instances
Assigns the material to all *MeshRenderer* using the same mesh instance. This is useful to ensure all instances of a *Mesh* are using the same *Material*.

![preview](/Documentation~/images/propagate_to_mesh_instances.gif)

##### Replace in all open Scenes
Assigns the material to all *MeshRenderer* using the same *Material*.
This is useful to replace a *Material* throughout the scene.

![preview](/Documentation~/images/propagate_to_materials.gif)

##### Replace on all *Mesh* instances
Assigns the material to all *MeshRenderer* using the same mesh instance and *Material*. This is useful to ensure all instances of a *Mesh* already using a specific *Material* are updated.

![preview](/Documentation~/images/propagate_instances_materials.gif)

##### Replace within Scene Root
Assigns the material to all *MeshRenderer* using the target *Material*, within children of the same top level GameObject. This is useful to replace a *Material* on all objects of one model.

#### Prefab Instance Mode
Sets the behaviour when dropping a *Material* onto a *Prefab* instance.

##### Assign
This has the same effect as dropping a material on an object. The *Material* is assigned as an override at scene level.

##### Popup Menu
Brings a *Context Menu* showing the *Nested Prefab* hierarchy.
Assigns the *Material* and apply the changes to the selected *Prefab*. This is useful to automatically propagate *Material* changes to all other *Prefab* instances.

![preview](/Documentation~/images/assign_and_apply_overrides.gif)

##### Assign to Most Inner Prefab
The *Material* is assigned and the override is applied to the *leaf* (most inner) *Prefab*.

#### Other Options

##### Auto Apply Overrides
Automatically apply Overrides to Leaf Prefabs when the assignment is the result of a *replacement*.

#### Reference Pinging

##### Material Pinging
Ctrl + Right Click on a *GameObject* in *Scene View* to "ping" its *Material*.

##### Mesh Pinging
Shift + Right Click on a *GameObject* in *Scene View* to "ping" its *Mesh*.

### Extracting Materials & Meshes
Right click on a *GameObject* or *Prefab* to extract its *Materials* and *Meshes* to the *Project* folder.

# Known Limitations
- The Overlay doesn't refresh properly when docked in a Toolbar.
- When working on a Prefab in Stage mode (Isolation), material assignments are set as overrides at Scene level.

# Future Improvements
- Customization of Key Modifiers (Ctrl, Alt, Shift) in the Preferemces
- (Re)building *Prefabs* from Instances
- Enabling preview when hovering a model with a *Material*.

