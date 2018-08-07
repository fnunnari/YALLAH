DIR=`dirname "$0"`
echo Running from $DIR
cd "$DIR"

# See: https://docs.blender.org/manual/de/dev/advanced/command_line/arguments.html
export BLENDER_USER_SCRIPTS="$DIR/../BlenderScripts"
export BLENDER_USER_CONFIG="$DIR/../BlenderConfig"
export TEMP="$DIR/../BlenderTemp"
../BlenderExe/blender.app/Contents/MacOS/blender
