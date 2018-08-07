using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

	// The object to follow
	public GameObject targetPlayer ;

	// The offset to keep from the object to follow
	public Vector3 offset = new Vector3 (0, 2, -6);

	// Will store the current velocity for the SmoothDamp
	private Vector3 curVel = new Vector3() ;

	// Use this for initialization
	void Start () {		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 player_pos = this.targetPlayer.transform.position;
		Vector3 target_pos = player_pos + offset;

		// Update the follower's position using the SmoothDamp
		this.transform.position = Vector3.SmoothDamp(this.transform.position, target_pos, ref curVel, 0.4f);
	}
}
