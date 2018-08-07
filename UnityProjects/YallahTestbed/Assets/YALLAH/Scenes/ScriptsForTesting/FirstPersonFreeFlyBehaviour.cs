using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Simple 3D space fly navigation. Useful for first person cameras. */
public class FirstPersonFreeFlyBehaviour : MonoBehaviour {

	public bool invertRotX = false;
	public bool invertRotY = false;

	[Tooltip("Look around mouse/rotation sensitivity.")]
	public float rotationSensitivity = 100;

	[Tooltip("Keys movement/translation speed (units/sec)")]
	public float movementSpeed = 1.0f;

    void Update() {
		float delta = movementSpeed * Time.deltaTime;

		Vector3 fwd = delta * (gameObject.transform.rotation * Vector3.forward);
		Vector3 right = delta * (gameObject.transform.rotation * Vector3.right);
		Vector3 up = delta * Vector3.up; // Yes, up is up.

		if (Input.GetKey(KeyCode.D)) // RIGHT
		{
			transform.position += right;
		}
		if (Input.GetKey(KeyCode.A)) // LEFT
		{
			transform.position -= right;
		}
		if (Input.GetKey(KeyCode.F)) // DOWN
		{
			transform.position -= up;
		}
		if (Input.GetKey(KeyCode.R)) // UP
		{
			transform.position += up;
		}
		if (Input.GetKey(KeyCode.W)) // FORWARD
		{
			transform.position += fwd;
		}
		if (Input.GetKey(KeyCode.S)) // BACKWARD
		{
			transform.position -= fwd;
		}

		if (Input.GetMouseButton(0)) {  // If left mouse button is pressed
			float xAxisMovement = Input.GetAxis("Mouse X") * rotationSensitivity * Time.deltaTime;
			if(xAxisMovement != 0.0f) {
				if (invertRotX) {
					xAxisMovement *= -1;
				}
				Vector3 currentRotation = this.gameObject.transform.eulerAngles;
				Quaternion newRotation = Quaternion.Euler(currentRotation.x, (currentRotation.y + xAxisMovement), currentRotation.z);
				this.gameObject.transform.rotation = newRotation;
			}


			float yAxisMovement = Input.GetAxis("Mouse Y") * rotationSensitivity * Time.deltaTime;
			if(yAxisMovement != 0.0f) {
				if (invertRotY) {
					yAxisMovement *= -1;
				}
				Vector3 currentRotation = this.gameObject.transform.eulerAngles;
				Quaternion newRotation = Quaternion.Euler((currentRotation.x - yAxisMovement), currentRotation.y, currentRotation.z);
				this.gameObject.transform.rotation = newRotation;
			}

		}
    }
		
}
