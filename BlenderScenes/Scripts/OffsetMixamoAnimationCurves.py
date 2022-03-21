# OffsetAnimationCurves
# Python script for Blender (Tested v2.79)
#
# This script take the current action and applies offsets to the bones rotation
# It is useful to match an imported animation with a target skeleton.
#
# It contains a database with key=the bone name: data=the quaternion to apply.
# It will run through all the keyframes of an animation and apply the given offset.
#
# Usage:
# * select the armature
# * activate the action to alter
# * execute the script.
#
# Limitations:
# * the number of keyframes must be the same, and aligned, through all the 4 component of the Quaternion
# * Works only for quaternion-driven bones animation
#
# TODO:
# * Take out the rotation offset dictionary into a separate file
# * Write a separate script to compare two skeletons (Both in T-pose) and generate the offset DB automatically.


import bpy
from mathutils import Quaternion

import math

from typing import Dict

# Check if the selected object os a MESH
#mesh_obj = bpy.context.active_object  # type: bpy.types.Object
#assert mesh_obj.type == 'MESH'
#arm_obj = mesh_obj.parent  # type: bpy.types.Object
arm_obj = bpy.context.active_object  # type: bpy.types.Object
assert arm_obj.type == 'ARMATURE'
arm = arm_obj.data  # type: bpy.types.Armature
assert isinstance(arm, bpy.types.Armature)


# DB of offsets.
# { Bone name: Quaternion, ... }
# For each bone, specify the rotation needed to align it with the reference T-pose skeleton downloaded from Mixamo.
# All rotations are in bone local space.
# Hence, when manually looking for the rotaion value,  first adjust bones closer to the root.
OFFSETS_DB_TPOSE = {

    "thigh_L": Quaternion((0,0,1), math.radians(1.8)),
    # upperarm_L x +4.80 deg, upperarm_L z +3.80 deg
    # Order: Z * Y * X
    "upperarm_L": Quaternion((0,0,1), math.radians(3.80)) * Quaternion((1,0,0), math.radians(4.80)),
    "lowerarm_L": Quaternion((0,0,1), math.radians(-3.80)) * Quaternion((1,0,0), math.radians(-14.50)),

    "thigh_R": Quaternion((0,0,1), math.radians(-1.8)),
    "upperarm_R": Quaternion((0,0,1), math.radians(-3.80)) * Quaternion((1,0,0), math.radians(4.80)),
    "lowerarm_R": Quaternion((0,0,1), math.radians(+3.80)) * Quaternion((1,0,0), math.radians(-14.50)),

}  # type: Dict[str, Quaternion]

OFFSETS_DB_APOSE = {

    "thigh_L": Quaternion((0,0,1), math.radians(-6.30)),
    "clavicle_L":  Quaternion((0,0,1), math.radians(6.0)),
    "upperarm_L": Quaternion((0,0,1), math.radians(24.0)) * Quaternion((1,0,0), math.radians(6.0)),
    "lowerarm_L": Quaternion((0,0,1), math.radians(-3.80)) * Quaternion((1,0,0), math.radians(-14.50)),

    "thigh_R": Quaternion((0,0,1), math.radians(6.30)),
    "clavicle_R":  Quaternion((0,0,1), math.radians(-6.0)),
    "upperarm_R": Quaternion((0,0,1), math.radians(-24.0)) * Quaternion((1,0,0), math.radians(6.0)),
    "lowerarm_R": Quaternion((0,0,1), math.radians(+3.80)) * Quaternion((1,0,0), math.radians(-14.50)),

}  # type: Dict[str, Quaternion]


def is_t_pose(arm_obj: bpy.types.Object) -> bool:
    """    
    @param arm_obj: The object of type ARMATURE 
    @return: True if the armature was finalized in T-position, false otherwise (A-position)
    """

    #
    # The test checks the difference of the Z (vertical) coordinates between the arm head and lower-arm tail.
    #
    shoulder_pos = arm_obj.pose.bones['upperarm_L'].bone.head_local
    wrist_pos = arm_obj.pose.bones['lowerarm_L'].bone.tail_local

    vertical_offset = (shoulder_pos - wrist_pos).z

    """
arm = bpy.context.active_object
shoulder_pos = arm.pose.bones['upperarm_L'].bone.head_local
wrist_pos = arm.pose.bones['lowerarm_L'].bone.tail_local
off = shoulder_pos - wrist_pos
# female, A-pose: off.z --> 0.2476813793182373
# female, T-pose: off.z --> 0.020379185676574707
    """
    if vertical_offset < 0.1:
        return True
    else:
        return False


def offset_bone_animation(action: bpy.types.Action, bone_name: str, offset: Quaternion) -> None:
    """Apply rotation offset to the rotation curves of the given action
       For the moment we handle only quaternions

    """

    # TODO -- Retrieve bone rotation mode and update accordingly

    curve_w = action.fcurves.find('pose.bones["{0}"].rotation_quaternion'.format(bone_name), 0)  # type: bpy.types.FCurve
    curve_x = action.fcurves.find('pose.bones["{0}"].rotation_quaternion'.format(bone_name), 1)  # type: bpy.types.FCurve
    curve_y = action.fcurves.find('pose.bones["{0}"].rotation_quaternion'.format(bone_name), 2)  # type: bpy.types.FCurve
    curve_z = action.fcurves.find('pose.bones["{0}"].rotation_quaternion'.format(bone_name), 3)  # type: bpy.types.FCurve

    assert curve_w is not None and curve_x is not None and curve_y is not None and curve_z is not None

    # STRONG ASSUMPTION!!!
    # If there is 1 keyframe for the w curve (0) there will be also for the other curves.
    n_keyframes_w = len(curve_w.keyframe_points)
    n_keyframes_x = len(curve_x.keyframe_points)
    n_keyframes_y = len(curve_y.keyframe_points)
    n_keyframes_z = len(curve_z.keyframe_points)

    assert n_keyframes_w == n_keyframes_x == n_keyframes_y == n_keyframes_z

    for i in range(n_keyframes_w):
        kf_w = curve_w.keyframe_points[i]  # type: bpy.types.Keyframe
        kf_x = curve_x.keyframe_points[i]  # type: bpy.types.Keyframe
        kf_y = curve_y.keyframe_points[i]  # type: bpy.types.Keyframe
        kf_z = curve_z.keyframe_points[i]  # type: bpy.types.Keyframe

        # All the time stamps should be the same
        timestamp = kf_w.co[0]
        print("ts {}".format(timestamp))
        # print(kf_w.co[0], kf_x.co[0], kf_y.co[0], kf_z.co[0])
        # assert kf_w.co[0] == kf_x.co[0] == kf_y.co[0] == kf_z.co[0]

        # Retrieve the current roation
        current_q = Quaternion((kf_w.co[1], kf_x.co[1], kf_y.co[1], kf_z.co[1]))
        # compute the updated rotation
        new_q = offset * current_q
        # store it back
        kf_w.co[1], kf_x.co[1], kf_y.co[1], kf_z.co[1] = new_q

    # Force timings and handles update
    curve_w.update()
    curve_x.update()
    curve_y.update()
    curve_z.update()


def apply_offsets(action: bpy.types.Action, db: dict) -> None:
    for bone_name in db.keys():
        # Retrieve the quaternion
        offset_q = db[bone_name]

        print("Applying offset {} to bone {}".format(offset_q, bone_name))
        offset_bone_animation(action=action, bone_name=bone_name, offset=offset_q)


action = arm_obj.animation_data.action
assert action is not None

print("Detecting resting pose...")
if is_t_pose(arm_obj=arm_obj):
    print("T-Pose")
    OFFSETS_DB = OFFSETS_DB_TPOSE
else:
    print("Assuming A-Pose")
    OFFSETS_DB = OFFSETS_DB_APOSE

apply_offsets(action=action, db=OFFSETS_DB)

# Force update of GUI and other properties
bpy.context.scene.frame_set(bpy.context.scene.frame_current)
