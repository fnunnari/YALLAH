#!/usr/bin/env bash

UNITY_EXE=/Applications/Unity/Hub/Editor/2020.3.14f1/Unity.app/Contents/MacOS/Unity


WIKI_DIR=../../YALLAH.wiki

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
# Check if Unity exe is relly there
echo "Checking for Unity in '$UNITY_EXE' ..."
if [ -f $UNITY_EXE ]; then
   echo "Found."
else
   echo "Unity executable not found. Please, edit variable UNITY_EXE and retry."
   exit 10
fi


#
# Check if The Wiki directory exists
echo "Checking for Wiki in '$WIKI_DIR' ..."
if [ -d $WIKI_DIR ]; then
   echo "Found."
else
   echo "Wiki directory not found. Please, edit variable WIKI_DIR and retry."
   exit 10
fi


#
# Make directories
echo "Please, enter the version numer (E.g., \"1.3\", \"1.0RC1\")"
read VER

TIMECODE=`date "+%y%m%d"`
RELEASE_DIR=YALLAH_SDK-$VER-$TIMECODE
echo "Making temporary directory '$RELEASE_DIR'..."
mkdir $RELEASE_DIR
ABS_RELEASE_DIR=$(get_abs_filename $RELEASE_DIR)
echo "Release dir (abs path): '$ABS_RELEASE_DIR'..."

#
# Check if a release with the same name is already there
echo "Checking if the archive '$RELEASE_DIR.zip' is already there ..."
if [ -f ../Releases/$RELEASE_DIR.zip ]; then
   echo "Already created in ../Releases/. Please, remove to create a new one."
   exit 10
else
   echo "not yet."
fi


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
pushd $WIKI_DIR
git archive --format zip --output $ABS_RELEASE_DIR/Wiki.zip master
popd

#
# README.md
echo "Copying README and Changelog..."
cp ../README.md $ABS_RELEASE_DIR
cp ../Changelog.md $ABS_RELEASE_DIR

#
# Zipping everything
echo "Zipping $RELEASE_DIR ..."
mkdir -p ../Releases
zip -r ../Releases/$RELEASE_DIR.zip $RELEASE_DIR/

echo "Remove temporary directory..."
rm -rf $RELEASE_DIR

echo "Done."

