// #define EMSCRIPTEN_KEEPALIVE

#include <stdint.h>
#include "emscripten.h"
#include "native.h"


//
// Animation Controller
//

fnType_Vs cs_PlayAnimationClip ;
fnType_I cs_IsAnimationClipPlaying ;
fnType_S cs_GetPlayingAnimationClip ;
fnType_S cs_ListAvailableAnimationClips ;
fnType_V cs_EnableAmbientAnimation ;
fnType_V cs_DisableAmbientAnimation ;

void set_anim_callbacks(
                        fnType_Vs play_fn,
                        fnType_I is_playing_fn,
                        fnType_S get_playing_fn,
                        fnType_S list_fn,
                        fnType_V enable_ambient_fn,
                        fnType_V disable_ambient_fn
                        )
{
    cs_PlayAnimationClip = play_fn ;
    cs_IsAnimationClipPlaying = is_playing_fn ;
    cs_GetPlayingAnimationClip = get_playing_fn ;
    cs_ListAvailableAnimationClips = list_fn ;
    cs_EnableAmbientAnimation = enable_ambient_fn ;
    cs_DisableAmbientAnimation = disable_ambient_fn ;
}

void EMSCRIPTEN_KEEPALIVE PlayAnimationClip(const char * clip_name)
{
    cs_PlayAnimationClip(clip_name) ;
}

int32_t EMSCRIPTEN_KEEPALIVE IsAnimationClipPlaying()
{
    return cs_IsAnimationClipPlaying() ;
}

const char * EMSCRIPTEN_KEEPALIVE GetPlayingAnimationClip()
{
    return cs_GetPlayingAnimationClip() ;
}

const char * EMSCRIPTEN_KEEPALIVE ListAvailableAnimationClips()
{
    return cs_ListAvailableAnimationClips() ;
}

void EMSCRIPTEN_KEEPALIVE EnableAmbientAnimation()
{
    cs_EnableAmbientAnimation() ;
}

void EMSCRIPTEN_KEEPALIVE DisableAmbientAnimation()
{
    cs_DisableAmbientAnimation() ;
}


//
// MaryTTS
//

fnType_Vs cs_MaryTTSspeak ;
fnType_I cs_IsMaryTTSspeaking;

void set_marytts_callbacks(
                           fnType_Vs speak_fn,
                           fnType_I is_speaking_fn
                           )
{
    cs_MaryTTSspeak = speak_fn ;
    cs_IsMaryTTSspeaking = is_speaking_fn ;
}

void EMSCRIPTEN_KEEPALIVE MaryTTSspeak(const char* msg)
{
    cs_MaryTTSspeak(msg) ;
}

int32_t EMSCRIPTEN_KEEPALIVE IsMaryTTSspeaking()
{
    return cs_IsMaryTTSspeaking() ;
}


//
// Facial Expression
//


fnType_Vs cs_SetCurrentFacialExpression ;
fnType_S cs_GetCurrentFacialExpression ;
fnType_V cs_ClearFacialExpression ;
fnType_S cs_ListFacialExpressions ;
fnType_Vf cs_SetExpressionTransitionTime ;
fnType_F cs_GetExpressionTransitionTime ;


void set_facial_expression_callbacks(
                                     fnType_Vs set_expression_fn,
                                     fnType_S get_expression_fn,
                                     fnType_V clear_expression_fn,
                                     fnType_S list_epxresisons_fn,
                                     fnType_Vf set_expr_trans_time_fn,
                                     fnType_F get_expr_trans_time_fn
                                    )
{
    cs_SetCurrentFacialExpression = set_expression_fn ;
    cs_GetCurrentFacialExpression = get_expression_fn ;
    cs_ClearFacialExpression = clear_expression_fn ;
    cs_ListFacialExpressions = list_epxresisons_fn ;
    cs_SetExpressionTransitionTime = set_expr_trans_time_fn ;
    cs_GetExpressionTransitionTime = get_expr_trans_time_fn ;
}


void EMSCRIPTEN_KEEPALIVE SetCurrentFacialExpression(const char* expression_name)
{
    cs_SetCurrentFacialExpression(expression_name) ;
}

const char * EMSCRIPTEN_KEEPALIVE GetCurrentFacialExpression() {
    return cs_GetCurrentFacialExpression() ;
}

void EMSCRIPTEN_KEEPALIVE ClearFacialExpression() {
    cs_ClearFacialExpression() ;
}

const char * EMSCRIPTEN_KEEPALIVE ListFacialExpressions() {
    return cs_ListFacialExpressions() ;
}

void EMSCRIPTEN_KEEPALIVE SetExpressionTransitionTime(float transition_time_secs) {
    return cs_SetExpressionTransitionTime(transition_time_secs) ;
}

float EMSCRIPTEN_KEEPALIVE GetExpressionTransitionTime() {
    return cs_GetExpressionTransitionTime() ;
}


//
// Eye Gaze
//

// void LookAtPoint(float x, float y, float z)
// void LookAtObject(string target_object_name)


fnType_Vfff cs_LookAtPoint ;
fnType_Vs cs_LookAtObject ;


void set_eyegaze_callbacks (
    fnType_Vfff look_at_point_fn,
    fnType_Vs look_at_object_fn
) {
    cs_LookAtPoint = look_at_point_fn;
    cs_LookAtObject = look_at_object_fn;
}


void EMSCRIPTEN_KEEPALIVE LookAtPoint(float x, float y, float z) {
    cs_LookAtPoint(x, y, z) ;
}

void EMSCRIPTEN_KEEPALIVE LookAtObject(const char * target_object_name) {
    cs_LookAtObject(target_object_name) ;
}


//
// CAMERA
//

fnType_V cs_set_next_camera_shot_fn ;
fnType_V cs_set_previous_camera_shot_fn ;
fnType_S cs_get_camera_shot_name_fn ;
fnType_Vs cs_set_camera_shot_fn ;
fnType_Vf cs_set_shot_fine_tune_percentage ;
fnType_F cs_get_shot_fine_tune_percentage ;
fnType_S cs_list_camera_shots_fn ;


void set_camera_callbacks (
  fnType_V set_next_camera_shot_fn,
  fnType_V set_previous_camera_shot_fn,
  fnType_S get_camera_shot_name_fn,
  fnType_Vs set_camera_shot_fn,
  fnType_Vf set_shot_fine_tune_percentage,
  fnType_F get_shot_fine_tune_percentage,
  fnType_S list_camera_shots_fn
                           ) {
    cs_set_next_camera_shot_fn = set_next_camera_shot_fn;
    cs_set_previous_camera_shot_fn = set_previous_camera_shot_fn;
    cs_get_camera_shot_name_fn = get_camera_shot_name_fn;
    cs_set_camera_shot_fn = set_camera_shot_fn;
    cs_set_shot_fine_tune_percentage = set_shot_fine_tune_percentage;
    cs_get_shot_fine_tune_percentage = get_shot_fine_tune_percentage;
    cs_list_camera_shots_fn = list_camera_shots_fn ;
}


void EMSCRIPTEN_KEEPALIVE GoToNextShot() {
    cs_set_next_camera_shot_fn() ;
}

void EMSCRIPTEN_KEEPALIVE GoToPreviousShot() {
    cs_set_previous_camera_shot_fn() ;
}

const char * EMSCRIPTEN_KEEPALIVE GetCurrentShot() {
    return cs_get_camera_shot_name_fn() ;
}

void EMSCRIPTEN_KEEPALIVE SetCurrentShot(char * shot) {
    cs_set_camera_shot_fn(shot) ;
}

void EMSCRIPTEN_KEEPALIVE SetShotFineTunePercentage(float v) {
    cs_set_shot_fine_tune_percentage(v) ;
}

float EMSCRIPTEN_KEEPALIVE GetShotFineTunePercentage() {
    return cs_get_shot_fine_tune_percentage() ;
}

const char * EMSCRIPTEN_KEEPALIVE ListCameraShots() {
    return cs_list_camera_shots_fn() ;
}
