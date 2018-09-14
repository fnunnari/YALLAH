#!/usr/bin/env bash

UNITY_EXE=/Applications/Unity/Unity.app/Contents/MacOS/Unity

get_abs_filename() {
  # $1 : relative filename
  echo "$(cd "$(dirname "$1")" && pwd)/$(basename "$1")"
}

#
# The directory where this script resides
DIR=`dirname "$0"`
ABS_DIR=$(get_abs_filename $DIR)
echo Running from $ABS_DIR
cd "$ABS_DIR"


#
# Make directories
TIMECODE=`date "+%y%m%d"`
RELEASE_DIR=YALLAH_SDK-$TIMECODE
echo "Making temporary directory '$RELEASE_DIR'..."
mkdir $RELEASE_DIR
ABS_RELEASE_DIR=$(get_abs_filename $RELEASE_DIR)
echo "Release dir (abs path): '$ABS_RELEASE_DIR'..."


#
# Zip the Blender addon
echo "Copying the yallah Blender add-on..."
rsync -av --exclude '.DS_Store' --exclude '*.pyc' --exclude '__pycache__' ../BlenderScripts/addons/yallah $ABS_RELEASE_DIR

echo "Zipping the yallah Blender add-on..."
pushd "$ABS_RELEASE_DIR"
zip -r yallah.zip yallah
rm -r yallah
popd


#
# Build the Unity Package
# See: https://docs.unity3d.com/Manual/CommandLineArguments.html
echo "Building the Unity Package..."
PKG_NAME=YALLAH_Unity-$TIMECODE.unitypackage
$UNITY_EXE -quit -batchmode -nographics -logFile unity_log.txt -exportPackage Assets/YALLAH $PKG_NAME  -projectPath "$ABS_DIR/../UnityProjects/YallahTestbed"
mv ../UnityProjects/YallahTestbed/$PKG_NAME $ABS_RELEASE_DIR


#
# Wiki/Docs
echo "Archiving the Wiki..."
pushd ../../YALLAH-wiki
git archive --format zip --output $ABS_RELEASE_DIR/Wiki.zip master
popd

#
# README.md
echo "Copying the README..."
cp ../README.md $ABS_RELEASE_DIR

#
# Zipping everything
echo "Zipping $RELEASE_DIR ..."
mkdir -p ../Releases
zip -r ../Releases/$RELEASE_DIR.zip $RELEASE_DIR/

echo "Remove temporary directory..."
rm -rf $RELEASE_DIR

echo "Done."

