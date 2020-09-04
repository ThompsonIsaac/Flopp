using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraScript : MonoBehaviour {

	private bool isRotating = false;
	public static bool buttonPressed;
	private float lastX;
	private float sensitivity = 0.4f;
	public static float cameraAngle = 15f; // Resets at the beginning of each new level
	public static int orientation = 0; // Determines how controls will affect the player
	private Transform PivotPoint;
	private float rotVelocity;
	private int rotDrag = 10; // How quickly the camera slows down
	
	// Find the pivot point upon start
	void Start () {
		PivotPoint = this.gameObject.transform.parent;
		buttonPressed = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			if (SceneManager.GetActiveScene().buildIndex != 13) { // Rotation disabled on Level 13
				isRotating = true;
				lastX = Input.mousePosition.x;
				//Debug.Log("x: " + Input.mousePosition.x + ", y: " + Input.mousePosition.y);
			}
		}
		if (Input.GetMouseButtonUp(0)) {
			isRotating = false;
		}
		if (buttonPressed) { // If a control button was pressed, cancel rotation.
			isRotating = false;
		}

		if (isRotating) { // Sync exactly with player dragging
			// Rotate on the camera's parent (pivot point) based on gesture movement.
			float dX = (Input.mousePosition.x - lastX) * sensitivity; // Change in MouseX this frame
			cameraAngle += ((Input.mousePosition.x - lastX) * sensitivity);
			rotVelocity = dX;
			lastX = Input.mousePosition.x;
		}

		// Update rotation based on velocity and dragging
		Vector3 rotation = PivotPoint.eulerAngles;
		rotation.y = cameraAngle;
		PivotPoint.eulerAngles = rotation;

		// Assign control orientation
		float angle = PivotPoint.eulerAngles.y;
		if (angle < 0) {
			angle += 360;
		} else if (angle > 360) {
			angle -= 360;
		}

		if (angle < 45) {
			orientation = 0;
		} else if (angle < 135) {
			orientation = 1;
		} else if (angle < 225) {
			orientation = 2;
		} else if (angle < 315) {
			orientation = 3;
		} else {
			orientation = 0;
		}
	}
}
