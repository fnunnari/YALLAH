
# Manual - How to acquire clothes from Adobe Fuse

This document is a chapter of the [YALLAH Manual](Manual%20-%20Main.md) series.
This chapter explains how to create dressed characters using [Adobe FuseCC](https://www.adobe.com/products/fuse.html) and apply those dresses to YALLAH characters.

The goal is to take the clothes from Fuse character and edit them in order to make them ready to be used with the 
Manuel Bastioni Lab _Proxy Fitting_ procedure.

## Create a new basic character for Fuse

This step has been already performed and the resulting files saved.
You need to repeat this procedure only if future updates of MBLab drastically change the shape of the body for man and woman,
 or if you want to support new phenotypes (like afro, asian, elf, dwarf, ...).

* Launch Fuse and create a basic character
* Export it as OBJ. For example, for the woman we call it `FuseDefaultWoman.obj`
* Load this FuseDefaultWoman.obj in Blender.
* Instantiate a default MBLab woman.
* "Sculpt" the Fuse mesh (that you got from the obj file) to fit the proportions of the MBLab character, preserving the vertex topology.
  - It is important that no vertices are added nor removed.
  - A snapshot of a work in progress is in `FemaleNakedSculptingToBastioni.blend`
* Re-export the sculpted model into obj.
* We save(d) this as: `AcquiringClothesFuse/Woman/Nacked/FittedToBastioniNaked.obj`


## Select and export the clothes from Fuse/Mixamo

* Open Fuse and import a new character using `FemaleFittedToBastioniNaked.obj`
	- (This must be done only the first time. Then, the character definition will stay in your Fuse installation.)
* Select the clothes you want and edit their colors.
* Select File -> Export obj in order to open the Fuse OBJ export panel option.
* Set the export option as:
	- Check: Export in new Folder
	- Uncheck: Triangulate Mesh
	- Check: Remove occluded polygons
	- Uncheck: Pack Textures and UVs
	- Character Scale: 1.0
	- Save Defaults (So that next time it will not be needed)

## Upload the character to Mixamo and get the Collada files with the dressed model

* With the character loaded, **click** on `Upload To Mixamo`.
	- (Note: this step is using the obj export options that we have set before)
* Let Mixamo do the auto-rig, select the full skeleton version.
* Download as Collada (.dae)
* You will receive a .zip file containing the DAE file and its textures.
    - Why do we need Collada? Because we will need to pose the collada character in order to align the clothes with the MBLab character.

A whole set of clothes has been saved in `AcquiringClothesFuse/Woman/Clothes/`

## Create an action to fit the neutral pose of the MBLab character to the Mixamo character

TODO -- How to create `FuseToMBLabAlignmentAction.blend`


## Import and use clothes from the Mixamo collada files

Procedure tested on Blender 2.79b.

* Initialize a new MBLab character (e.g., a female F_CA01)
	- (It might be needed to adjust the vertical alignment when there are shoes. See later.)
* Finalize the character
* `POSE AND ANIMATION -> Reset pose` of the character (to be sure that we go to A-pose)

*  (A) Import a Mixamo Collada file with clothes
	- (e.g.: `fuse/Woman/FemaleFittedToBastioniClothes08/FitFittedToBastioniClothes8.dae`)
	- Click-click-click! Check the 3 import options: `Fix Leaf Bones`, `Find Bone Chains` and `Auto Connect`.
* Rename the bones with the `AcquiringClothesFuse/Scripts/mixamo_bone_name_fixer.py` script (i.e. get rid of the mixamo\_ prefix in all bones)
    * Open the script
    * Select the armature created from the Collada file (By default `Armature`)
    * Run the script
* Apply `Ctrl+A --> Rotation & Scale` to both the collada armature and all the children meshes.
    - Select altogether using shift+click
	- (This will prevent the propagation of transformation errors)
* Append `Menu -> File -> Append` the Action `FuseToMBLabAlignmentAction.blend/Action/FemaleAction`
* Set the action of the imported Dae Armature to `FemaleAction`
    - Select the armature
    - Open a `DopeSHeet ` view and show the `Action Editor`
    - Select the `FemaleAction`
* Jump to the last frame of the action (393)
* (A1) If the Collada Armature has shoes...
	- Fuse applies a vertical offset to the armature in order to align the bottom vertices of the shoes to the floor (z=0)
	- You must apply a negative vertical offset to the collada armature to have a perfect alignment (e.g., for clothes 08 loc.z=-0.02)
	- (You can check the quality of the alignment from the extremities of the arms using a frontal perspective view)
	- If you had to offset vertically, select the Collada Armature and Apply `Ctrl+A --> Location`.
		- The offset will be transferred to the children meshes. Select the children meshes and again Apply `Ctrl+A --> Location`.
* On all meshes children of the Collada Armature, `Apply` the Armature Modifier
* Select all meshes to keep, press `Alt`+`P` and select `Clear and Keep Transformation`
	- This will un-parent the clothes meshes from the Armature, but keep there global position.
* (B) Remove/Hide the unwanted armature, pose, body mesh, and unwanted clothes from the scene



## Animate clothes with MBLab

* For each of the piece of clothes that you want to animate:
  - Select the clothes mesh
  - Delete all vertex groups (they pertain to the old Collada Armature)
  - Change its parent to the `MBlab_sk...` skeleton.
  - Fit the proxy using the MBLab panel:
    - Go to the AFTER CREATION TOOLS -> PROXY FITTING
    - Character: MBLab_bd...
    - Proxy: <the clothes mesh you manually selected>
    - Offset: Use 5 as initial test value
    - Influence: Leave 20 (default)
    - (Optionally, if you plan to later delete skin vertices under the clothes) **Check** `Add vertex mask group`
    - **Check** `Transfer weight from body to proxy`
    - **Click** `Fit Proxy`
  - Now the clothes mesh will have an armature modifier and can be animated using the MBLab skeleton

(C)

You can now try to pose the MBLab skeleton and see if it animates.
If the vertex weights need to be fixed, we can paint them once for all on the naked body and then find a way to transfer the weight to each piece of clothes via script.

## Fix alignment of the Tops

(D) Apply this optional step to improve the alignment of the Tops, to better align breast and the upper back part.

Problem: After applying the animation that aligns the Mixamo skeleton to MBLab, there is a problem at the top of the back of the MBLab character. The body is still too big for the clothes.

This section explains how to 'drag' the Tops clothes into a position suitable for Proxy fitting with MBLab.

The mis-alignment can be fixed by: detaching the Tops from the body, slightly rotating the spine03 and neck bones of the character, re-enabling the skinning for the Tops, bringing back the body at its initial position.
In order to work, the shoulders and the wrists must be kept in position using temporary IKs.

Preparation:
- Prepare the character with the clothes as children of the MBLab character.
- Remove the Armature modifier for the Tops.
- Remove any existing Proxy fitting for the Tops.

Pin the wrists is space:
  - In PoseMode select bone 'lower_arm_L'
  - create an IK with new empty for bone (Add IK to Bone --> To new empty object)
  - set chain length to 2
  - the same for 'lower_arm_R'

Pin the shoulders in space:
- In PoseMode select bone 'clavicle_L'
- create an IK with new empty for bone (Add IK to Bone --> To new empty object)
- chain length to 1
- repeat for 'clavicle_R'

Rotate the body:
- Select bone 'spine3'
	- Rotate 10 degs around local-X
	- (Shortcuts `r x x` and then `ctrl+mouse` to rotate in snap mode)
- Select bone 'neck'
	- Rotate -10 degs around local-X

- Pose -> Apply Pose as Rest pose
	(The MBLab body will move. Don't care)

- Select Tops
	- Add Modifier -> Armature -> Object: `MBLab_sk...`

Reverse transform:
- Select bone 'spine3'
	- Rotate -10 degs around local-X
- Select bone 'neck'
	- Rotate 10 degs around local-X

- Select Tops
	- Apply armature modifier

Now the tops will be better aligned with the MBLab default standing skeleton.
You can save it and use with a new MBLab skeleton.

In the following we can also cleanup the scene and test directly:
- Skeleton: Pose -> Apply Pose as Rest pose
  - (The MBLab mesh will go back in position)
- Tops: Add Armature modifier
- Remove all 4 IK constraints from the bones
  - Now, you can also delete the 4 IK empties

- Happy animation testing


## Pack a scene with only clothes and save it.

* Remove all unneeded information:
	- The original Mixamo DAE skeleton and body.
	- The current MBLab skeleton and body
	- The unneeded actions
* For each piece of clothes:
	- Un-parent
	- Remove Armature modifier

* `File -> Externanl data -> Pack all into .blend`
* Save to ... e.g. `AnnaInterpreterClothes_aloneE.blend`
* Quit Blender, reload, re-save, re-quit, ... 4/5 times ... until you get rid of all unneeded textures.
(E)

The saved scene will contain only the clothes meshes.
These meshes can be used to dress a new freshly created MBLab character.
If the character's proportions are edited, the clothes can be fitted using the _Proxy Fitting_ feature of MBLab.
The details of this procedure are given in the _Authoring_ manual.
