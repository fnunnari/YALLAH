import bpy

import os
import json

from typing import List

from yallah import YALLAH_FEATURES_DIR

print("Building connection bones, to have a fully connected skeleton...")
unconnected_bones_filename = os.path.join(YALLAH_FEATURES_DIR, "ConnectBones/UnconnectedBonePairsMBLab1_6.json")

with open(unconnected_bones_filename, "r") as unconnected_bones_file:
    unconnected_bones_list = json.load(fp=unconnected_bones_file)

# Switch to skeleton edit mode
# Check if the selected object os a MESH
mesh_obj = bpy.context.active_object
assert mesh_obj.type == 'MESH'
arm_obj = mesh_obj.parent
assert arm_obj.type == 'ARMATURE'
arm = arm_obj.data
assert isinstance(arm, bpy.types.Armature)
# switches to armature-edit
bpy.ops.object.mode_set(mode='OBJECT')
bpy.context.scene.objects.active = arm_obj
bpy.ops.object.mode_set(mode='EDIT')


# Cache the list of created bones
connecting_bones = []  # type: List[str]

# For each pair, create the connecting bone
for bone_pair in unconnected_bones_list:
    b1_name, b2_name = bone_pair

    b1 = arm.edit_bones[b1_name]  # type: bpy.types.EditBone
    b2 = arm.edit_bones[b2_name]  # type: bpy.types.EditBone

    new_bone_name = b1_name + "-to-" + b2_name
    connecting_bones.append(new_bone_name)

    print("Creating {}...".format(new_bone_name))

    # Create the bone
    new_bone = arm.edit_bones.new(new_bone_name)  # type: bpy.types.EditBone

    # Connect the tail of the first with the head of the second
    new_bone.parent = b1
    new_bone.head = b1.tail
    b2.parent = new_bone
    new_bone.tail = b2.head
    # Set connected flag
    new_bone.use_connect = True

#
# Lock all transformations
# (This is done in object mode, on the PoseBones
bpy.ops.object.mode_set(mode='OBJECT')

for pbname in connecting_bones:

    print("Locking transforms for {}...".format(pbname))
    pb = arm_obj.pose.bones[pbname]  # type: bpy.types.PoseBone

    pb.lock_location = True, True, True
    pb.lock_rotation = True, True, True
    pb.lock_rotation_w = True
    pb.lock_scale = True, True, True
    pb.lock_ik_x = True
    pb.lock_ik_y = True
    pb.lock_ik_z = True

print("Connecting bones created.")
