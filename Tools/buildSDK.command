#!/usr/bin/env bash

UNITY_EXE=/Applications/Unity/Unity.app/Contents/MacOS/Unity

DIR=`dirname "$0"`
echo Running from $DIR
cd "$DIR"


# timecode=`date "+%y%m%d-%H%M"`
TIMECODE=`date "+%y%m%d"`

RELEASE_DIR=YALLAH_SDK-$TIMECODE

echo "Making temporary directory '$RELEASE_DIR'"
mkdir $RELEASE_DIR


# Zip the Blender addon
echo "Copying the Blender add-on..."
rsync -av --exclude '.DS_Store' --exclude '*.pyc' --exclude '__pycache__' ../BlenderScripts/addons/yallah $RELEASE_DIR

# Zip the Manual
echo "Copying the docs..."
rsync -av --exclude '.DS_Store' ../Docs/Manual $RELEASE_DIR

# Build the Unity Package
# See: https://docs.unity3d.com/Manual/CommandLineArguments.html
echo "TODO - Building the Unity Package..."
PKG_NAME=YALLAH_Unity-$TIMECODE.unitypackage
$UNITY_EXE -quit -batchmode -nographics -logFile unity_log.txt -exportPackage Assets/YALLAH $PKG_NAME  -projectPath ../UnityProjects/YallahTestbed
mv ../UnityProjects/YallahTestbed/$PKG_NAME $RELEASE_DIR

echo "Zipping..."
mkdir ../Releases
zip -r ../Releases/$RELEASE_DIR.zip $RELEASE_DIR/

echo "Remove temporary directory..."
rm -rf $RELEASE_DIR

echo "Done."

