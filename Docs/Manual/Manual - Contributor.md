
# Manual - Contributing to the YALLAH project

This document is a chapter of the [YALLAH Manual](Manual%20-%20Main.md) series.
In this chapter we describe how to develop new features and functionalities that cleanly integrates with existing ones.

The software architecture of ???, together with a set of guidelines, allows developers to implement new features which work on the same virtual character.
  * The software architecture aims at supporting different real-time game engines.
  * (Under investigation) An automated mechanism to prevent or manage conflicts between functionalities.


## Introduction
TODO


## File organization
TODO (Hierarchy)

```
yallah/                         - the addon main directory
  __init__.py
  data/                         - dir for binary data (JSON datafiles, pictures, ...)
  features/                     - dir for the features 
    Test/                       - An example Test feature
      Setup.py                  - The mandatory Setup.py file that each feature dir must contain 
      ... other files
    Camera/
    EyeGaze/
    ...
  mblab_tools.py
  shape_key_utils.py
  vertex_utils.py
```

## Guidelines to implement a new feature
Each feature will have its own directory in the YALLAH_FEATURES_DIR.
Each feature will keep its data binary files under its own directory. 

The Setup.py file of a feature is supposed to be invoked in these conditions:
* Editor is in `Object` mode
* The MBLab character's MESH has been already finalized
* Active object is the character's MESH
* The only selected objects is the character's MESH

Advantages of this organization:
* Each Feature can be applied on its own by executing the Setup.py file in the editor. Easier to debug.
* It is possible to update the code of a Feature and re-apply without re-loading Blender.

## Relationship with MBLab

### Thing to change when you update to a new version of MBLab

* EyeGaze setup.
  - Blender: The script to Setup the eye bones contains the indices of two vertex for each of the eye balls.
  The vertices correspond to the center of the eyeball and to the center of the cornea.
  These vertices are programmatically selected before calling the "select linked" operator.
  If you switch MBLab version, check that the vertices still have the same index.
  - Unity: the script EyeHeadGazeLogic.cs contains the name of the blendshapes used to control the movement of the eyes.
  

* Test-to-speech phonemes.
  The MaryTTS script uses a table mapping marytts phoneme information into visemes (which are implemented as blendshapes).
  In the Haxe code `MaryTTSBlendsequencer.hx` this map is implemented as a Map: `static var PHONEMES_MAP: Map<String,String> = [...] ;`.
  If, between versions of MBLab, the list of the phonemes changes, this map has to be updated accordingly.
  

 

## Design patterns
TODO

## An example
TODO

## Develop Multi-Platform Features Using Haxe and Meta-Programming
TODO


## Mary TTS

The sequence table is then parsed by the script `Scripts/haxe_lib/MaryTTSBlendSequencer`.
The main script to parse the realized durations and compute the weight for the viseme blendshapes of the character is implemented in Haxe:
`SharedHaxeClasses/MaryTTSBlendSequencer.hx`.

The MakeFile `SharedHaxeClasses/MakeFile` contains the directives to translate the Haxe code into both Python and C# and to update the code in the projects:
* run `make` to translate the code.
* run `make install` to copy the generated files into the Blender and Unity projects.

For each target platform, a _wrapper_ will take care of the platform-specific stuff and invoke the Sequencer.
The main duties of a wrapper are: handle HTTP connections to retrieve wav and realized durations, call the update function at high frequency, update the blendshape values of the character.
