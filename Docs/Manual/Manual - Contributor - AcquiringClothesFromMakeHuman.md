# Project YALLAH:
## Acquiring clothes from MakeHuman

After  making the dress in MakeHuman, do the following:
  * Open a new Blender scene and generate a new MBLab character and finalize it.
  * Run Setup.py otherwise you can't after applying the proxy fitting.
  * In the MakeHuman add the pose named Cmu_mb to the character.
  * From MakeHuman export the character as MHX2.
  * Using MHX2, import the model to Blender.(MHX2 is a Blender plugin which is used for directly importing from MakeHuman).
  * Check when you select the character, it is highlighted with green color , this means that it is part of a group. Hit ctrl+alt+G before you continue.
  * You can fix pose of the imported dress by applying a previously generated keyframe if you have it. Otherwise manually fix the pose of the dress to align with the character.
  * Generate a keyframe for this pose if it is the first time. This can be used for future imports.

## Dressing MBlab character

After alignment of the character's pose, do the following to fit the clothes with the character:
  * Before proxy fitting, make sure to "apply" all modifiers in all parts of the clothes.
  * Generate a new MBLab character.
  * Apply proxy fitting:
    * Check "transfer weights from body to proxy (replace existing)"
    * Leave Offset: 2 and Influence: 20
    * Select Fit Proxy
  * Before you start editing the parts which don't fit, make sure that the mesh moves with the armature. You can do it by selecting the armature in Pose mode.
  * To be able to edit the mesh, first you have to do the following:
    * Select the clothes in object mode
    * Go to the "shape keys", select "mbastlab_proxyfit", open the down arrow and select "move to top".
  * Now you can modify or delete vertices which don't fit well to the character.
