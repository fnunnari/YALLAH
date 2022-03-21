import bpy

obj = bpy.context.active_object
assert obj is not None
assert obj.type == 'MESH'
mesh_obj = obj

# We need to remove all the modifiers (except the armature),
# otherwise the blendshapes will not be visible in Unity
modifiers_to_remove = []
for md in mesh_obj.modifiers:
    # print("md name: " + md.name)
    if not md.name.endswith("_armature"):
        modifiers_to_remove.append(md)
for md in modifiers_to_remove:
    print("Removing modifier '" + md.name + "'")
    mesh_obj.modifiers.remove(md)
