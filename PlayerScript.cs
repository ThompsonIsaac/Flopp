using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

	static public bool Died; // Whether to do the entrance GUI on a scene
	public GameScript EventHandler;

	// Allows manipulation of player position
	// Separation prevents rotating from changing the player's movement axis
	public Transform Position; // Refers to the player's PARENT
	public Transform Rotation; // Refers to the player

	// Allow for fragging player upon death
	public GameObject FragDoll;
	private bool doFrag = false;
	private float expForce = 75f;

	// Allow arrow buttons to control the player
	public Button UpBtn;
	public Button RightBtn;
	public Button DownBtn;
	public Button LeftBtn;

	// Function that restarts scene, used in an invoke
	void RestartScene() {SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
	
	// Has the game ended?
	private bool gameOver = false;

	// How long it will take the block to move
	private float moveTime = 0.3f;

	// 0 means not moving, 1-4 indicate ongoing directional movement, 5 means phantom stopped movement
	public int moving = 0;

	// 0 means standing, 1 means laying sideways, 2 means laying forward to back
	private int orientation = 0;
	private int newOrientation; // Used for storing predicted orientation

	// How far the block has moved
	private float step = 0;

	// Store block position before each movement
	private Vector3 oldPosition;
	private Vector3 oldRotation;
	private Vector3 newPosition;
	private Vector3 newRotation;

	// Store the name of an event tile we landed on
	private string tileName;

	// Store tiles which will collapse after the player leaves them
	private GameObject fallTile1;
	private GameObject fallTile2;

	// Used to detect which tiles the player will land on
	private Collider[] tileFinder;

	// Allow the phantom to stop the player
	public void KillPlayer() {
		moving = 5;
		Died = true;
		Frag(); // Blow up the player
		Invoke("RestartScene", 2);
	}

	// START ONESHOT - Connects touch interface with player controls

	void MovePlayerOnClick(int direction) {
		if (!gameOver && moving == 0) {
			moving = FixOrientation(direction);
			PreMove();
		}
	}
	void Start () {
		UpBtn.GetComponent<Button>().onClick.AddListener(delegate{MovePlayerOnClick(1);});
		RightBtn.GetComponent<Button>().onClick.AddListener(delegate{MovePlayerOnClick(2);});
		DownBtn.GetComponent<Button>().onClick.AddListener(delegate{MovePlayerOnClick(3);});
		LeftBtn.GetComponent<Button>().onClick.AddListener(delegate{MovePlayerOnClick(4);});
	}

	int FixOrientation (int direction) { // Correct input direction to how camera is rotated
		int newDirection = direction + CameraScript.orientation;
		if (newDirection > 4) {
			newDirection -= 4;
		}
		return newDirection;
	}

	// Explode the player into a million pieces
	void Frag () {
		// Replace player object with frag pieces
		this.GetComponent<BoxCollider>().enabled = false;
		this.GetComponent<MeshRenderer>().enabled = false;
		
		GameObject SceneFrag;

		if (orientation == 0) {
			SceneFrag = Instantiate(FragDoll, Position.transform.position, Rotation.transform.rotation);
		} else if (orientation == 1) {
			SceneFrag = Instantiate(FragDoll,
				Position.transform.position - Vector3.left, Rotation.transform.rotation);
		} else {
			SceneFrag = Instantiate(FragDoll,
				Position.transform.position - Vector3.forward, Rotation.transform.rotation);
		}

		foreach (Transform child in SceneFrag.GetComponent<Transform>()) {
			child.gameObject.GetComponent<Rigidbody>().AddForce(Random.Range(-expForce, expForce),
				Random.Range(-expForce, expForce), Random.Range(-expForce, expForce),
				ForceMode.Impulse);
		}
	}

	// PRE-MOVE SEQUENCE ONESHOT
	// Much of this code is determining what will happen to the player after it is done moving

	void PreMove () {
		// Store position and rotation into variables that can be changed
		oldPosition = Position.position;
		oldRotation = Rotation.eulerAngles;
		newPosition = oldPosition; // Store old position into a variable which will be modified

		// Predict future orientation
		if (orientation == 0) { // If standing:
			if (moving == 1 || moving == 3) { // If moving forward or backwards:
				newOrientation = 2; // Lay forward to backwards
			} else { // If moving left or right:
				newOrientation = 1; // Lay left to right
			}
		} else if (orientation == 1) { // If laying left to right:
			if (moving == 2 || moving == 4) { // If moving left or right:
				newOrientation = 0; // Stand
			}
		} else { // If laying forward to backwards:
			if (moving == 1 || moving == 3) { // If moving forward or backwards:
				newOrientation = 0; // Stand
			}
		}

		// Determine which tiles the cube will fall on, and if it will result in death
		// Determining whether this is a roll or not is redundant later in the code
		if (orientation == 0 || (orientation == 1 && (moving == 2 || moving == 4)) ||
			           			(orientation == 2 && (moving == 1 || moving == 3))) { // If not rolling:
			if      (moving == 1) {newPosition.z += 1.5f;}
			else if (moving == 2) {newPosition.x += 1.5f;}
			else if (moving == 3) {newPosition.z -= 1.5f;}
			else if (moving == 4) {newPosition.x -= 1.5f;}
		} else { // If rolling:
			if      (moving == 1) {newPosition.z += 1f;}
			else if (moving == 2) {newPosition.x += 1f;}
			else if (moving == 3) {newPosition.z -= 1f;}
			else if (moving == 4) {newPosition.x -= 1f;}
		}
		
		// Find tiles the player will be touching
		newPosition.y = -0.2f;
		tileFinder = Physics.OverlapBox(newPosition, new Vector3(0.25f,0.1f,0.25f));

		if (tileFinder.Length == 0 || (newOrientation != 0 && tileFinder.Length == 1)) { // If landed on air,
			gameOver = true;
			doFrag = true; // Do frag animation in PostMove
			Died = true; // Don't repeat the entrance GUI.
			Invoke("RestartScene", 2); // This time starts BEFORE player starts moving
		} else {

			// If the player will land on a special tile and activate it, store its name for later.
			
			if (tileFinder.Length == 1) { // If the player will land STANDING:
				if (tileFinder[0].tag != "Tile") { // Any special tile can be recorded.
					tileName = tileFinder[0].name;

					// End the level if the player stepped on the goal or a weak tile.
					if (tileFinder[0].tag == "Goal") {
						Died = false;
						gameOver = true;
					} else if (tileFinder[0].tag == "Weak") {
						gameOver = true;
						Died = true;
						Invoke("RestartScene", 2);
					}
				} else {
					tileName = null;
				}
			} else { // If the player will land sideways on a special tile:
				// High pressure tiles and goals are not recorded
				if (tileFinder[0].tag != "Tile" && tileFinder[0].tag != "HiPres"
												&& tileFinder[0].tag != "Goal"
												&& tileFinder[0].tag != "Weak") {
					tileName = tileFinder[0].name;
				} else if (tileFinder[1].tag != "Tile" && tileFinder[1].tag != "HiPres"
													   && tileFinder[1].tag != "Goal"
													   && tileFinder[1].tag != "Weak") {
					tileName = tileFinder[1].name;
				} else {
					tileName = null;
				}
			}
		}
	}

	// POST-MOVE SEQUENCE ONESHOT
	// Handles the result of a successful landing

	void PostMove () {
		// Set new orientation to what was predicted earlier
		orientation = newOrientation;

		// Fix rotation to prevent odd-looking movements in the future
		newRotation = Rotation.eulerAngles;
		if (orientation == 0) {newRotation = new Vector3(0, 0, 0);}
		if (orientation == 1) {newRotation = new Vector3(0, 0, 90);}
		if (orientation == 2) {newRotation = new Vector3(90, 90, 90);}
		Rotation.eulerAngles = newRotation;

		// Handle special tile activation which had its name stored earlier
		if (tileName != null) {
			EventHandler.HandleEvent(tileName); // Send the tile name to the event handler
		}

		if (doFrag) { // Explode the player if they fell off the edge
			Frag();
		}

		// Store tiles which will fall away on the next movement
		if (!gameOver) {
			if (tileFinder[0].tag == "FallAway") {
				fallTile1 = tileFinder[0].gameObject;
			}
			if (orientation > 0) { // If laying down,
				if (tileFinder[1].tag == "FallAway") {
					fallTile2 = tileFinder[1].gameObject;
				}
			}
		}

		// Prepare for next movement
		moving = 0;
		step = 0;

		this.GetComponent<AudioSource>().Play();
	}
	
	// Update is called once per frame
	void Update () {

		// DETECT KEYPRESS

		if (moving == 0) {
			if (!gameOver) {
				if (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.UpArrow)) {
					moving = FixOrientation(1);
					PreMove();
				} else if (Input.GetKeyDown (KeyCode.D) || Input.GetKeyDown (KeyCode.RightArrow)) {
					moving = FixOrientation(2);
					PreMove();
				} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow)) {
					moving = FixOrientation(3);
					PreMove();
				} else if (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.LeftArrow)) {
					moving = FixOrientation(4);
					PreMove();
				}
			}

			// LOOP - PLAYER MOVEMENT

		} else if (moving != 5) {
			// Track how far the cube has moved into its animation
			step += Time.deltaTime;
			if (step > moveTime) {
				step = moveTime;
			}

			// Store old position into a variable which will be modified
			newPosition = oldPosition;

			// Change Y value based on transition from standing to laying and vice versa
			// Y value doesn't change for rolls
			if (orientation == 0) { // If standing
				newPosition.y = oldPosition.y + (-0.5f * step) / moveTime;
			} else if ((orientation == 1 && (moving == 2 || moving == 4)) ||
			           (orientation == 2 && (moving == 1 || moving == 3))) {
				newPosition.y = oldPosition.y + (0.5f * step) / moveTime;
			}

			// Move and rotate in the direction of movement
			if (moving == 1) { // Moving up
				Rotation.eulerAngles = oldRotation + new Vector3((90f * step) / moveTime, 0, 0);
				if (orientation == 1) { // Roll
					newPosition.z = oldPosition.z + (step / moveTime);
				} else { // Flip
					newPosition.z = oldPosition.z + (1.5f * step) / moveTime;
				}
			}
			if (moving == 2) { // Moving right
				if (orientation == 2) { // Roll
					Rotation.eulerAngles = oldRotation + new Vector3((90f * step) / moveTime, 90, 90);
					newPosition.x = oldPosition.x + (step / moveTime);
				} else { // Flip
					Rotation.eulerAngles = oldRotation + new Vector3(0, 0, (-90f * step) / moveTime);
					newPosition.x = oldPosition.x + (1.5f * step) / moveTime;
				}
			}
			if (moving == 3) { // Moving down
				Rotation.eulerAngles = oldRotation + new Vector3((-90f * step) / moveTime, 0, 0);
				if (orientation == 1) { // Roll
					newPosition.z = oldPosition.z - (step / moveTime);
				} else { // Flip
					newPosition.z = oldPosition.z - (1.5f * step) / moveTime;
				}
			}
			if (moving == 4) { // Moving left
				if (orientation == 2) { // Roll
					Rotation.eulerAngles = oldRotation + new Vector3((-90f * step) / moveTime, 90, 90);
					newPosition.x = oldPosition.x - (step / moveTime);
				} else { // Flip
					Rotation.eulerAngles = oldRotation + new Vector3(0, 0, (90f * step) / moveTime);
					newPosition.x = oldPosition.x - (1.5f * step) / moveTime;
				}
			}

			Position.position = newPosition;

			// Shrink tiles
			if (fallTile1 != null) {
				fallTile1.GetComponent<Transform>().localScale = new Vector3(
					1f - (step / moveTime), 0.2f, 1f - (step / moveTime));
			}
			if (fallTile2 != null) {
				fallTile2.GetComponent<Transform>().localScale = new Vector3(
					1f - (step / moveTime), 0.2f, 1f - (step / moveTime));
			}

			// -- ONESHOT AT THE END OF PLAYER MOVEMENT --

			if (step == moveTime) {
				Destroy(fallTile1);
				Destroy(fallTile2);
				fallTile1 = null;
				fallTile2 = null;
				PostMove();
			}
		}
	}
}
