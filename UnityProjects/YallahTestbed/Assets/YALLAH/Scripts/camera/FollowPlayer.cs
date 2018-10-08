using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Small script to animate a Camera.
 * The script will moves the camera close to the character when it is facing the camera, 
 * but it moves far away when the character is facing away.
 */
public class FollowPlayer : MonoBehaviour {

    // The object to follow
    public GameObject targetObject ;

    // The camera offset when far away
    public Vector3 farAwayOffset = new Vector3 (0, 2, 6);

    // The camera offset for close-ups
    public Vector3 closeUpOffset = new Vector3(0, 1.5f, 1);

    // Will store the current velocity for the SmoothDamp
    private Vector3 dampVelocity = new Vector3() ;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
        Vector3 player_pos = this.targetObject.transform.position;

        // Check character orientation
        Vector3 char_direction = this.targetObject.transform.rotation * Vector3.forward;
        Vector3 camera_direction = this.transform.rotation * Vector3.forward;
        double dot = Vector3.Dot(char_direction, camera_direction);
        //Debug.Log(dot);

        // If the character is facing the camera, use the close-up distance, otherwise use the far-away.
        Vector3 cam_offset = dot > 0.0 ? farAwayOffset : closeUpOffset;
        Vector3 target_pos = player_pos + cam_offset;

        // Update the camera position using the SmoothDamp
        this.transform.position = Vector3.SmoothDamp(this.transform.position, target_pos, ref dampVelocity, 0.5f);
    }
}
