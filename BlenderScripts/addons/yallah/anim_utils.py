import bpy
import bpy.types

from typing import Tuple


def frame_range(action: bpy.types.Action) -> Tuple[float, float]:
    """Retrieves the first and the last frame of an action.
    Replaces the original Action.frame_range property, which is wrongly returning (1.0, 2.0)
     on animations with a single keyframe."""

    s = None
    e = None

    if action.fcurves is None:
        raise Exception("No F-Curves defined in action {}".format(action.name))

    for fc in action.fcurves:
        kfps = fc.keyframe_points

        if len(kfps) == 0:
            raise Exception("No keyframes for path {}[{}] in action {}".format(fc.data_path, fc.array_index, action.name))

        first_frame = kfps[0].co[0]
        last_frame = kfps[-1].co[0]

        if s is None:
            s = first_frame
        elif first_frame < s:
            s = first_frame

        if e is None:
            e = last_frame
        elif last_frame > e:
            e = last_frame

    return s, e


class ExportActionData(bpy.types.Operator):
    """Operator to export the animation data of the current action into a JSON file."""

    bl_idname = "yallah.export_action_data"
    bl_label = """Export the animation data of the current action into a JSON file."""

    json_filename = bpy.props.StringProperty(name="Action JSON File",
                                             description="The json file that will hold the animation data",
                                             subtype="FILE_PATH")

    @classmethod
    def poll(cls, context):
        obj = context.active_object  # type: bpy.types.Object
        if obj is not None:
            anim_data = obj.animation_data
            if anim_data is not None:
                act = anim_data.action
                if act is not None:
                    return True

        return False

    def execute(self, context):

        import json

        obj = context.active_object  # type: bpy.types.Object
        act = obj.animation_data.action
        fcurves = act.fcurves

        out_data = {}

        for fc in fcurves:  # type: bpy.types.FCurve

            print("Inserting data_path {}, array_index {}".format(fc.data_path, fc.array_index))

            if fc.data_path not in out_data:
                index_dict = {}
                out_data[fc.data_path] = index_dict
            else:
                index_dict = out_data[fc.data_path]

            points = []
            for kfpoint in fc.keyframe_points:  # type: bpy.types.Keyframe
                points.append([kfpoint.co[0], kfpoint.co[1]])  # we save it as a 2-array, not as Vector

            index_dict[fc.array_index] = points

        with open(self.json_filename, 'w') as out_file:
            json.dump(obj=out_data, fp=out_file, indent=2)

        return {'FINISHED'}


class SetDummyUserToAllActions(bpy.types.Operator):
    """Operator to set 'F' (dummy user) to all actions."""

    bl_idname = "yallah.set_dummy_user_to_all_actions"
    bl_label = "Set 'F' (dummy user) to all actions"

    @classmethod
    def poll(cls, context):
        if not (context.mode == 'POSE' or context.mode == 'OBJECT'):
            return False

        return True

    def execute(self, context):

        import bpy

        for action in bpy.data.actions:
            action.use_fake_user = True

        return {'FINISHED'}


class AddStartEndFramesToAllAnimationCurves(bpy.types.Operator):
    """Operator to force every animation curve to have a keyframe at the beginning and at the end of the
     action containing them."""

    bl_idname = "yallah.add_start_end_frames_to_all_actions"
    bl_label = "Add Start/End keyframes to all animation curves."

    @classmethod
    def poll(cls, context):
        if not (context.mode == 'POSE' or context.mode == 'OBJECT'):
            return False

        return True

    def execute(self, context):

        import bpy

        #
        # For each action in the database
        for action in bpy.data.actions:

            action_frame_start, action_frame_end = frame_range(action)
            # print("For action ", action.name, action_frame_start, action_frame_end)

            #
            # For each animation curve in the action
            for fc in action.fcurves:
                first_kf = fc.keyframe_points[0]
                first_kf_time = first_kf.co[0]
                last_kf = fc.keyframe_points[-1]
                last_kf_time = last_kf.co[0]
                # print("FCURVE range ", first_kf_time, last_kf_time)

                # If the first keyframe is set after the action start
                if first_kf_time > action_frame_start:
                    first_kf_value = first_kf.co[1]
                    fc.keyframe_points.insert(frame=action_frame_start, value=first_kf_value)

                # If the last keyframe is set before the action end
                if last_kf_time < action_frame_end:
                    last_kf_value = last_kf.co[1]
                    fc.keyframe_points.insert(frame=action_frame_end, value=last_kf_value)

        return {'FINISHED'}


class CreateAPoseAction(bpy.types.Operator):
    """Operator to set an A-Pose animation key frame."""

    A_POSE_ACTION_NAME = "A-Pose"

    bl_idname = "yallah.create_apose_action"
    bl_label = "Creates an action called " + A_POSE_ACTION_NAME\
               + " with one keyframe at position 1 with the character reset in identity position."

    @classmethod
    def poll(cls, context):
        if not (context.mode == 'POSE' or context.mode == 'OBJECT'):
            return False

        obj = context.active_object  # type: bpy.types.Object
        if not obj.type == "ARMATURE":
            return False

        return True

    def execute(self, context):

        obj = bpy.context.active_object

        # Create the action
        if CreateAPoseAction.A_POSE_ACTION_NAME not in bpy.data.actions:
            bpy.data.actions.new(CreateAPoseAction.A_POSE_ACTION_NAME)
        if obj.animation_data is None:
            obj.animation_data_create()

        obj.animation_data.action = bpy.data.actions[CreateAPoseAction.A_POSE_ACTION_NAME]

        # Keyframe position
        bpy.context.scene.frame_set(1)

        # Object
        # Clear object position and pose.
        # bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.location_clear()
        bpy.ops.object.rotation_clear()
        bpy.ops.object.scale_clear()
        obj.keyframe_insert("location", frame=1)
        obj.keyframe_insert("rotation_euler", frame=1)
        obj.keyframe_insert("scale", frame=1)

        # Bones
        bpy.ops.object.mode_set(mode='POSE')
        bpy.ops.pose.select_all(action='SELECT')
        bpy.ops.pose.transforms_clear()
        # Insert the keyframe for the bones
        bpy.ops.anim.keyframe_insert_menu(type='LocRotScale')

        # Be sure that the action stays in memory.
        bpy.data.actions[CreateAPoseAction.A_POSE_ACTION_NAME].use_fake_user = True

        # Dirty trick to avoid curve simplification
        # Add a new keyframe with a negligible delta from the first one
        action = bpy.data.actions[CreateAPoseAction.A_POSE_ACTION_NAME]
        EPSILON = 0.0001
        for fc in action.fcurves:
            first_kf = fc.keyframe_points[0]
            first_kf_time = first_kf.co[0]

            first_kf_value = first_kf.co[1]

            # If the first keyframe is set after the action start
            fc.keyframe_points.insert(frame=first_kf_time + 1, value=first_kf_value + EPSILON)

        return {'FINISHED'}


def register():
    bpy.utils.register_class(ExportActionData)
    bpy.utils.register_class(SetDummyUserToAllActions)
    bpy.utils.register_class(AddStartEndFramesToAllAnimationCurves)
    bpy.utils.register_class(CreateAPoseAction)


def unregister():
    bpy.utils.unregister_class(ExportActionData)
    bpy.utils.unregister_class(SetDummyUserToAllActions)
    bpy.utils.unregister_class(AddStartEndFramesToAllAnimationCurves)
    bpy.utils.unregister_class(CreateAPoseAction)


