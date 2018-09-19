#it is an effort to adapt animations for normal heels to be usaed for woman character with high heels

import bpy

#adjusts orientation of bones
def adaptBones():
    obj.data.bones["foot_R"].select = True
    bpy.context.scene.mblab_rot_offset_0 = -0.27
    obj.data.bones["foot_R"].select = False
    bpy.ops.pose.select_all(action='DESELECT')

    obj.data.bones["foot_L"].select = True
    bpy.context.scene.mblab_rot_offset_0 = -0.27
    obj.data.bones["foot_L"].select = False
    bpy.ops.pose.select_all(action='DESELECT')

    obj.data.bones["toes_R"].select = True
    bpy.context.scene.mblab_rot_offset_0 = -0.27
    obj.data.bones["toes_R"].select = False
    bpy.ops.pose.select_all(action='DESELECT')

    obj.data.bones["toes_L"].select = True
    bpy.context.scene.mblab_rot_offset_0 = 0.27
    obj.data.bones["toes_L"].select = False
    bpy.ops.pose.select_all(action='DESELECT')

    obj.data.bones["spine01"].select = True
    bpy.context.scene.mblab_rot_offset_0 = 0.03
    obj.data.bones["spine01"].select = False
    bpy.ops.pose.select_all(action='DESELECT')

    obj.data.bones["spine02"].select = True
    bpy.context.scene.mblab_rot_offset_0 = 0.03
    obj.data.bones["spine02"].select = False
    bpy.ops.pose.select_all(action='DESELECT')

#adjusts vertical position of the character in order that character's feet stay on the ground
def shiftZKeyframes(shiftZValue):
    obj = bpy.context.active_object
    action = obj.animation_data.action

    for fcu in action.fcurves:
        if (fcu.data_path == 'pose.bones["root"].location'):
            if (fcu.array_index == 2):
                for keyframe in fcu.keyframe_points:
                    keyframe.co[1] += shiftZValue

obj = bpy.context.active_object
#bpy.ops.object.mode_set(mode='POSE', toggle=False)

bpy.ops.pose.select_all(action='DESELECT')
adaptBones()
shiftZKeyframes(-2.5)


