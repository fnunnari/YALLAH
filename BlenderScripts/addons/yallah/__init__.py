# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTIBILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
# General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program. If not, see <http://www.gnu.org/licenses/>.

bl_info = {
    "name" : "YALLAH",
    "author" : "fabrizio",
    "description" : "Yet another low level agent handler",
    "blender" : (2, 83, 0),
    "version" : (0, 0, 1),
    "location" : "View3D",
    "warning" : "",
    "category" : "Object"
}
import os
import bpy 


from . import mblab_tools
from bpy.utils import register_class
from . mblab_tools import is_female
from . mblab_tools import is_male
from . mblab_tools import is_mblab_body
from . mblab_tools import SetupMBLabCharacter

from . mblab_tools import RemoveAnimationFromFingers
from . mblab_tools import SetRelaxedPoseToFingers
from . mblab_tools import ResetCharacterPose


from . vertex_utils import LoadVertexGroups
from . vertex_utils import SaveVertexGroups

from . anim_utils import SetDummyUserToAllActions
from . anim_utils import CreateAPoseAction

from . import shape_key_utils
from . import vertex_utils
from . import anim_utils



# Package version in x.y.z string form.
YALLAH_VERSION = ".".join([str(n) for n in bl_info["version"]])

YALLAH_DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
YALLAH_FEATURES_DIR = os.path.join(os.path.dirname(__file__), "features")


class Yallah_PT_Panel(bpy.types.Panel):
    bl_idname = "Yalla_PT_Panel"
    bl_label = "YALLAH_Panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    # bl_context = 'objectmode'
    bl_category = "YALLAH"

    def draw(self, context):
        layout = self.layout
        obj = context.active_object
        if obj is None:
            layout.row().label(text= "Nothing active")
        
        #Mesh
        elif obj.type == 'MESH':
            if not is_mblab_body(mesh_obj=obj):
                layout.row().label(text= "The MESH must be a _finalized_ MBLab character")
                return

            if is_female(mesh_obj=obj):
                layout.row().label(text="MBLab Female Character")
                clothes_mask_file = "ClothesMasks-F_CA01.json"
            elif is_male(mesh_obj=obj):
                layout.row().label(text="MBLab Male Character")
                clothes_mask_file = "ClothesMasks-M_CA01.json"
            else:
                layout.label("Not an MBLab supported phenotype (male or female)")
                return
        
            # SETUP
            row = layout.row()
            box = row.box()
            box.label(text= "Setup:")
            #TODO non type object has no attribute yallah setup done ?
            if obj.yallah_setup_done:
                box.label(text= "Setup already performed.")
            else:
                box.operator(SetupMBLabCharacter.bl_idname, text="Setup a MBLab character")

        #
            # CLOTHES
            row = layout.row()
            box = row.box()
            box.label(text="Clothes:")
            # box.label(text=text=text="Load Clothes Vertex Groups")
            op = box.operator(LoadVertexGroups.bl_idname, text="Load Clothes Vertex Groups")
            op.vertex_groups_filename = os.path.join(YALLAH_DATA_DIR, clothes_mask_file)
            op.replace_existing = True

        #
        # ARMATURE
        elif obj.type == 'ARMATURE':

            row = layout.row()
            box = row.box()
            box.label(text="Animation:")
            box.operator(RemoveAnimationFromFingers.bl_idname, text="Remove Fingers Animation")
            box.operator(SetRelaxedPoseToFingers.bl_idname, text="Set Relaxed Fingers Keyframe")
            box.operator(ResetCharacterPose.bl_idname, text="Reset Character Pose")
            box.operator(SetDummyUserToAllActions.bl_idname)

        #
        # No ops
        else:
            layout.label(text="Please, select an MBLab armature or body.")



classes = (Yallah_PT_Panel,)
#register, unregister = bpy.utils.register_classes_factory(classes)

def register():
    mblab_tools.register()
    shape_key_utils.register()
    vertex_utils.register()
    anim_utils.register()
    #2.83 api reference
    from bpy.utils import register_class
    for clas in classes:
        register_class(clas)

def unregister():
    mblab_tools.unregister()
    shape_key_utils.unregister()
    vertex_utils.unregister()
    anim_utils.unregister()
    #2.83 api 
    from bpy.utils import unregister_class
    for clas in reversed(classes):
        unregister_class(clas)

if __name__ == "__main__":
    register()