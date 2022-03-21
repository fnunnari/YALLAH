console.log("YALLAH: Loading JS to setup the Native/Public API.");

if(!window.Module) {
  window.Module = gameInstance.Module;
}

//
// In this section we map the native C functions to JS.
// We do it thanks to the Emscripten function cwrap:
// https://kripken.github.io/emscripten-site/docs/api_reference/preamble.js.html?highlight=cwrap#cwrap
// format cwarp(function_name, return type, [param1_type. param2_type, ...])
// The name of the function must be the same we find in the native.c file.
// Types mapping:
// - void --> null
// - int, float --> number
// - char * --> string

Module.onRuntimeInitialized = function() {
  console.log("Setup Public API: Wrapping functions...");

  //
  // ANIMATION
  //
  PlayAnimationClip = Module.cwrap('PlayAnimationClip', null, ['string'])
  IsAnimationClipPlaying = Module.cwrap('IsAnimationClipPlaying', 'number', [])
  GetPlayingAnimationClip = Module.cwrap('GetPlayingAnimationClip', 'string', [])
  ListAvailableAnimationClips = Module.cwrap('ListAvailableAnimationClips', 'string', [])
  EnableAmbientAnimation = Module.cwrap('EnableAmbientAnimation', null, [])
  DisableAmbientAnimation = Module.cwrap('DisableAmbientAnimation', null, [])

  //
  // MaryTTS
  //
  MaryTTSspeak = Module.cwrap('MaryTTSspeak',null,['string']);
  IsMaryTTSspeaking = Module.cwrap('IsMaryTTSspeaking', 'number', []);

  //
  // FacialExpressions
  //
  SetCurrentFacialExpression = Module.cwrap('SetCurrentFacialExpression', null, ['string'])
  GetCurrentFacialExpression = Module.cwrap('GetCurrentFacialExpression', 'string', [])
  ClearFacialExpression = Module.cwrap('ClearFacialExpression', null, [])
  ListFacialExpressions = Module.cwrap('ListFacialExpressions', 'string', [])
  SetExpressionTransitionTime = Module.cwrap('SetExpressionTransitionTime', null, ['number'])
  GetExpressionTransitionTime = Module.cwrap('GetExpressionTransitionTime', 'number', [])

  //
  // Eye-gaze
  //
  LookAtPoint = Module.cwrap('LookAtPoint', null, ['number', 'number','number'])
  LookAtObject = Module.cwrap('LookAtObject', null, ['string'])

  
  ////
  // Camera Positioner
  //
  GoToNextShot = Module.cwrap('GoToNextShot', null, [])
  GoToPreviousShot = Module.cwrap('GoToPreviousShot', null, [])
  GetCurrentShot = Module.cwrap('GetCurrentShot', 'string', [])
  SetCurrentShot = Module.cwrap('SetCurrentShot', null, ['string'])
  SetShotFineTunePercentage = Module.cwrap('SetShotFineTunePercentage', null, ['number'])
  GetShotFineTunePercentage = Module.cwrap('GetShotFineTunePercentage', 'number', [])
  ListCameraShots = Module.cwrap('ListCameraShots', 'string', [])
  
  console.log("Setup Public API: functions wrapped.");
};
