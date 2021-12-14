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

import os
import bpy
from bpy.utils import register_class
from bpy.utils import unregister_class

from . import mblab_tools
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

bl_info = {
    "name": "YALLAH",
    "author": "Fabrizio Nunnari, Daksitha Withanage",
    "description": "Yet another low level agent handler",
    "blender": (2, 95, 0),
    "version": (2, 0, 0),
    "location": "View3D",
    "warning": "",
    "category": "Object"
}


# Package version in x.y.z string form.
YALLAH_VERSION = ".".join([str(n) for n in bl_info["version"]])

YALLAH_DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
YALLAH_FEATURES_DIR = os.path.join(os.path.dirname(__file__), "features")


class YALLAH_PT(bpy.types.Panel):
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
            layout.row().label(text="Nothing active")

        #
        # MESH selected
        elif obj.type == 'MESH':
            if not is_mblab_body(mesh_obj=obj):
                layout.row().label(text="The MESH must be a _finalized_ MBLab character")
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
            box.label(text="Setup:")
            # TODO non type object has no attribute yallah setup done ?
            if obj.yallah_setup_done:
                box.label(text="Setup already performed.")
            else:
                box.operator(SetupMBLabCharacter.bl_idname, text="Setup a MBLab character")

            # CLOTHES
            row = layout.row()
            box = row.box()
            box.label(text="Clothes:")
            # box.label(text=text=text="Load Clothes Vertex Groups")
            op = box.operator(LoadVertexGroups.bl_idname, text="Load Clothes Vertex Groups")
            op.vertex_groups_filename = os.path.join(YALLAH_DATA_DIR, clothes_mask_file)
            op.replace_existing = True

        #
        # ARMATURE selected
        elif obj.type == 'ARMATURE':

            row = layout.row()
            box = row.box()
            box.label(text="Animation:")
            box.operator(RemoveAnimationFromFingers.bl_idname, text="Remove Fingers Animation")
            box.operator(SetRelaxedPoseToFingers.bl_idname, text="Set Relaxed Fingers Keyframe")
            box.operator(ResetCharacterPose.bl_idname, text="Reset Character Pose")
            box.operator(SetDummyUserToAllActions.bl_idname)

        #
        # Unsupported type selected
        else:
            layout.label(text="Please, select an MBLab armature or body.")


#
# REGISTRATION
#

# The set of classes to (un)resister
classes = (YALLAH_PT,)


def register():
    mblab_tools.register()
    shape_key_utils.register()
    vertex_utils.register()
    anim_utils.register()

    # 2.83 api for (un)regisering
    for cls in classes:
        register_class(cls)


def unregister():
    mblab_tools.unregister()
    shape_key_utils.unregister()
    vertex_utils.unregister()
    anim_utils.unregister()

    # 2.83 api for (un)regisering
    for cls in reversed(classes):
        unregister_class(cls)


if __name__ == "__main__":
    register()
