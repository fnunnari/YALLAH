
targets: haxe_lib.cs

# See: https://haxe.org/manual/compiler-usage-flags.html
# net-target                : <name> Sets the .NET target. Defaults to "net".
#                             xbox, micro (Micro Framework), compact (Compact
#                             Framework) are some valid values
# net-ver                   : <version:20-45> Sets the .NET version to be
#                             targeted

# See also here: https://github.com/HaxeFoundation/HaxeManual/wiki/Haxe-C%23
haxe_lib.cs: MaryTTSBlendSequencer.hx EyeBlinker.hx
	haxe -cp . MaryTTSBlendSequencer EyeBlinker glm -D no-root -cs haxe_lib.cs
	touch haxe_lib.cs


run:
	haxe -cp . -main MaryTTSBlendSequencer --interp
	haxe -cp . -main EyeBlinker --interp


install:
	mkdir -p ../UnityProjects/YallahTestbed/Assets/YALLAH/Scripts/haxe_lib
	cp -r haxe_lib.cs/src/* ../UnityProjects/YallahTestbed/Assets/YALLAH/Scripts/haxe_lib


clean:
	rm haxe_lib.py
	rm -rf haxe_lib.cs
