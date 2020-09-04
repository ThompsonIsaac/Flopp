using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomScript : MonoBehaviour {
	
	// Used to obtain values from the player
	public GameObject PlayerRotation;
	public GameObject PlayerPosition;

	// Transfer recorded transform values to the phantom
	public GameObject PhantomRotation;
	public GameObject PhantomPosition;

	// Detect if the phantom collides with the player
	public BoxCollider PhantomCollider;
	public BoxCollider PlayerCollider;

	// Stops the player on collision
	public PlayerScript PlayerScript;

	// Used to record the position of the player
	private List<Vector3> positionList = new List<Vector3>();
	private List<Vector3> rotationList = new List<Vector3>();

	private bool recording = false; // Start recording values when true
	private bool falling = false; // Lowering phantom when true
	private float fallSpeed = 3f; // How fast the phantom descends at first
	private bool collided = false; // Has the phantom already collided with the player?

    public float followTime = 3f; // How many seconds before the phantom should follow

    private float landTime; // Record time of phantom landing.

	public void Spawn () {
		recording = true;
		falling = true;
		
		Debug.Log("Spooky phantom sound");
	}

	public void Stop () {
		recording = false;
		falling = false;
	}

	// Update is called once per frame
	void Update () {
		// Lower the phantom from the sky
		if (falling) {
			Vector3 newPosition = PhantomPosition.transform.position;
			newPosition += Vector3.down * Time.deltaTime * fallSpeed; // Decrease y value of phantom
			if (newPosition.y <= 1) {
				newPosition.y = 1;
				falling = false;
                landTime = Time.time;
			}
			PhantomPosition.transform.position = newPosition; // Update phantom position
		}
		
		if (recording) { // Have the phantom follow the player
			if (!falling && Time.time - landTime > followTime) { // Send the FIRST value in the list to phantom, then pop it
                print(Time.time - landTime);
				if (falling) {falling = false;}

				PhantomPosition.transform.position = positionList[0];
				PhantomRotation.transform.eulerAngles = rotationList[0];
				positionList.RemoveAt(0);
				rotationList.RemoveAt(0);
			}

			// Always record a new keyframe from the player
			positionList.Add(PlayerPosition.transform.position);
			rotationList.Add(PlayerRotation.transform.eulerAngles);

			// Test if the phantom intersects the player
			if (PhantomCollider.bounds.Intersects(PlayerCollider.bounds) && !collided) {
				collided = true;
				PlayerScript.KillPlayer();
			}
		}
	}
}
