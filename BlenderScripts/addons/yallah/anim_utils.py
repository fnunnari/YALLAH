import bpy


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

            print("Inserting dp {} idx {}".format(fc.data_path, fc.array_index))

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

        for actions in bpy.data.actions:
            actions.use_fake_user = True

        return {'FINISHED'}


def register():
    bpy.utils.register_class(ExportActionData)
    bpy.utils.register_class(SetDummyUserToAllActions)


def unregister():
    bpy.utils.unregister_class(ExportActionData)
    bpy.utils.unregister_class(SetDummyUserToAllActions)


