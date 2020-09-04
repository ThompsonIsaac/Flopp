using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScript : MonoBehaviour {

	// Handles GUI transitions between scenes
	public NextLevelGUI NextLevelGUI;

	// Allows phantom button to trigger phantom
	public PhantomScript PhantomScript;
	private bool doPhantom = false;

	public PlayerScript PlayerScript;

	// Player is moved down on win, and camera is moved to player
	public Transform PlayerTransform;
	public Camera Camera;
	private float cameraWinStep = 0;
	private Vector3 cameraOldPosition;

	// Potential toggleable variables used with switches on a level by level basis.
	// ENSURE these all reset to false at the beginning of each level.
	private bool toggle1 = false;
	private bool toggle2 = false;
	private bool toggle3 = false;
	private bool toggle4 = false;
	private bool toggle5 = false;

	// For changing tiles once toggled
	public Material FinishLine;

	// Store values for when a tile is falling to the floor from the air
	private GameObject FallingTile;
	public float fallDropTime = 2f;
	private float fallStep = 0f;
	private bool falling = false;
	private bool hasWon = false;

	// Store values for raising tiles out of view
	private List<GameObject> RaisingTiles = new List<GameObject>();
	private float raiseStep = 0f;

	// Store values for lowering tiles to below the view
	private List<GameObject> LoweringTiles = new List<GameObject>();
	public float tileLowerTime = 0.5f;
	private float lowerStep = 0f;

	// Store the current build index
	private int b;

	// Scene initialization
	void Start () {
		b = SceneManager.GetActiveScene().buildIndex; // Used for the level no. GUI and button detection
		cameraOldPosition = Camera.transform.position; // Used for moving the camera if the player wins
	}

	// Update is called once per frame
	void Update () {

		// Moving the camera to the player upon win
		// Disabled due to transition to orthographic camera.
		/* if (hasWon) {
			cameraWinStep += Time.deltaTime;
			// Have the camera slow down as it approaches the player
			float moveFactor = Mathf.Pow(0.3f, cameraWinStep);
			Camera.transform.position = new Vector3((cameraOldPosition.x * moveFactor) + 
				(PlayerTransform.position.x * (1 - moveFactor)), cameraOldPosition.y,
				(cameraOldPosition.z * moveFactor) + 
				((PlayerTransform.position.z - 5) * (1 - moveFactor)));
		} */

		// Handle player falling with a tile
		if (falling) {
			fallStep += Time.deltaTime;
			// Control acceleration of the player down
			Vector3 translateValue;
			if (hasWon) { // Winning is a slow descent
				translateValue = Vector3.down * Mathf.Pow((fallStep / 4f), 1.3f);
			} else { // Falling through weak floor is a faster descent
				translateValue = Vector3.down * Mathf.Pow((fallStep / 1.5f), 1.5f);
			}
			PlayerTransform.Translate(translateValue);
			FallingTile.transform.Translate(translateValue);
			if (fallStep > fallDropTime) {
				falling = false;
			}
		}

		// Handle lowering tiles (from the air)
		if (LoweringTiles.Count > 0) {
			lowerStep += Time.deltaTime;
			// Run once per frame for each tile being lowered
			for(int i = 0; i < LoweringTiles.Count; i++) {
				Vector3 currentPos = LoweringTiles[i].transform.position;

				if (lowerStep > tileLowerTime) { // If tile has reached destination,
					LoweringTiles[i].transform.position = new Vector3(currentPos.x,
						-0.1f, currentPos.z);

				} else { // Continue to move tile down
					LoweringTiles[i].transform.position = new Vector3(currentPos.x,
						9.9f - ((lowerStep / tileLowerTime) * 10f), currentPos.z);
				}
			}

			// Break the loop.
			if (lowerStep > tileLowerTime) {
				LoweringTiles.Clear();
				lowerStep = 0f;
			}
		}

		// Handle raising tiles (to the air)
		if (RaisingTiles.Count > 0) {
			raiseStep += Time.deltaTime;
			// Run once per frame for each tile being raised
			for(int i = 0; i < RaisingTiles.Count; i++) {
				Vector3 currentPos = RaisingTiles[i].transform.position;

				if (raiseStep > tileLowerTime) { // If tiles have reached destination,
					RaisingTiles[i].transform.position = new Vector3(currentPos.x,
						9.9f, currentPos.z);

				} else { // Continue to move tiles up
					RaisingTiles[i].transform.position = new Vector3(currentPos.x,
						((raiseStep / tileLowerTime) * 10f) - 0.1f, currentPos.z);
				}
			}

			// Break the loop.
			if (raiseStep > tileLowerTime) {
				RaisingTiles.Clear();
				raiseStep = 0f;
			}
		}
	}

	public void HandleEvent (string tileName) {

		// These events are applicable to all levels.
		if (tileName == "Goal" || tileName.Substring(0, 4) == "Weak") {
			FallingTile = GameObject.Find(tileName);
			falling = true;
			if (tileName == "Goal") {
				if (doPhantom) {
					PhantomScript.Stop();
				}
				int nextLevel = SceneManager.GetActiveScene().buildIndex + 1;
				if (SaveLoad.levelNo < nextLevel) { // If we have progressed past our save file,
					SaveLoad.levelNo = nextLevel;
					if (SaveLoad.levelNo == 26) {
						SaveLoad.levelNo = 25; // At end of game, leave save file on last level
					}
				}
				SaveLoad.Save();
				hasWon = true;
				NextLevelGUI.FadeOut();
				Debug.Log("Win sound");
			}

		// These events are level-specific.
		
		} else if (b == 3) {
			if (tileName == "LoPressure") {
				if (!toggle1) {LoweringTiles.Add(GameObject.Find("ToggleTile")); toggle1 = true;}
			}
		} else if (b == 4) {
			if (tileName == "HiPressure") {
				if (!toggle1) {LoweringTiles.Add(GameObject.Find("ToggleTile")); toggle1 = true;}
			}
		} else if (b == 7) { // Complex level in which buttons must be hit in a certain order.
			bool resetTiles = false;

			if (tileName == "LoPressure (2)") {
				if (!toggle1) {
					toggle1 = true;
					LoweringTiles.Add(GameObject.Find("ToggleTile (1)"));
				} else {
					resetTiles = true;
				}
			} else if (tileName == "LoPressure (4)") {
				if (toggle1 && !toggle2 && !toggle3 && !toggle4) {
					toggle2 = true;
					LoweringTiles.Add(GameObject.Find("ToggleTile (2)"));
				} else {
					resetTiles = true;
				}
			} else if (tileName == "LoPressure (3)") {
				if (toggle1 && toggle2 && !toggle3 && !toggle4) {
					toggle3 = true;
					LoweringTiles.Add(GameObject.Find("ToggleTile (3)"));
				} else {
					resetTiles = true;
				}
			} else if (tileName == "LoPressure (1)") {
				if (toggle1 && toggle2 && toggle3 && !toggle4) {
					toggle4 = true;
					LoweringTiles.Add(GameObject.Find("ToggleTile (4)"));
				} else {
					resetTiles = true;
				}
			}

			if (resetTiles) {
				if (toggle1) {RaisingTiles.Add(GameObject.Find("ToggleTile (1)")); toggle1 = false;}
				if (toggle2) {RaisingTiles.Add(GameObject.Find("ToggleTile (2)")); toggle2 = false;}
				if (toggle3) {RaisingTiles.Add(GameObject.Find("ToggleTile (3)")); toggle3 = false;}
				if (toggle4) {RaisingTiles.Add(GameObject.Find("ToggleTile (4)")); toggle4 = false;}
			}

		} else if (b == 9) {
			if (tileName == "HiPressure (1)") {
				if (!toggle1) {LoweringTiles.Add(GameObject.Find("ToggleTile")); toggle1 = true;}
			} else if (tileName == "LoPressure (1)") {
				if (toggle1) {RaisingTiles.Add(GameObject.Find("ToggleTile")); toggle1 = false;}
			}
		} else if (b == 10) {
			if (tileName == "LoPressure (1)") {
				if (!toggle1) {
					RaisingTiles.Add(GameObject.Find("ToggleTile (3)"));
					LoweringTiles.Add(GameObject.Find("ToggleTile (2)"));
					toggle1 = true;
				}
			} else if (tileName == "HiPressure (1)") {
				if (!toggle2) {
					RaisingTiles.Add(GameObject.Find("ToggleTile (4)"));
					RaisingTiles.Add(GameObject.Find("ToggleTile (5)"));
					toggle2 = true;
				}
			} else if (tileName == "HiPressure (2)") {
				if (!toggle3) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (1)"));
					toggle3 = true;
				}
			}
		} else if (b == 12) {
			if (tileName == "HiPressure (1)") {
				if (!toggle1) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (1)"));
					toggle1 = true;
				}
			} else if (tileName == "HiPressure (2)") {
				if (!toggle2) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (2)"));
					toggle2 = true;
				}
			}
		} else if (b == 16) { // First phantom level
			if (tileName == "PhantomTile") {
				if (!toggle1) {
					PhantomScript.Spawn();
					doPhantom = true;
					toggle1 = true;
				}
			}
		} else if (b == 17) {
			if (tileName == "HiPressure (1)") {
				if (!toggle1) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (1)"));
					toggle1 = true;
				}
			}
		} else if (b == 18) {
			if (tileName == "LoPressure (1)") {
				if (!toggle1) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (1)"));
					toggle1 = true;
				}
			}
		} else if (b == 19 || b == 22) {
			if (tileName == "HiPressure (1)") {
				if (!toggle1) {
					toggle1 = true;
					GameObject.Find(tileName).GetComponent<MeshRenderer>().material = FinishLine;
				}
			} else if (tileName == "HiPressure (2)") {
				if (!toggle2) {
					toggle2 = true;
					GameObject.Find(tileName).GetComponent<MeshRenderer>().material = FinishLine;
				}
			} else if (tileName == "HiPressure (3)") {
				if (!toggle3) {
					toggle3 = true;
					GameObject.Find(tileName).GetComponent<MeshRenderer>().material = FinishLine;
				}
			} else if (tileName == "HiPressure (4)") {
				if (!toggle4) {
					toggle4 = true;
					GameObject.Find(tileName).GetComponent<MeshRenderer>().material = FinishLine;
				}
			}
			if (toggle1 && toggle2 && toggle3 && toggle4 && !toggle5) {
				toggle5 = true;
				LoweringTiles.Add(GameObject.Find("ToggleTile"));
			}
		} else if (b == 20) {
			if (tileName == "PhantomTile") {
				if (!toggle1) {
					PhantomScript.Spawn();
					doPhantom = true;
					toggle1 = true;
				}
			} else if (tileName == "HiPressure (1)") {
				if (!toggle2) {
					LoweringTiles.Add(GameObject.Find("ToggleTile"));
					toggle2 = true;
				}
			}
		} else if (b == 21) {
			if (tileName == "HiPressure (1)") {
				if (!toggle1) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (1)"));
					toggle1 = true;
				}
			} else if (tileName == "HiPressure (2)") {
				if (!toggle2) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (2)"));
					toggle2 = true;
				}
			}
		} else if (b == 23) {
			if (tileName.Substring(0, 6) == "LoPres") {
				PlayerScript.KillPlayer();
			}
		} else if (b == 24) {
			if (tileName == "LoPressure (1)") {
				if (!toggle1) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (1)"));
					toggle1 = true;
				}
			} else if (tileName == "HiPressure (1)") {
				if (!toggle2) {
					LoweringTiles.Add(GameObject.Find("ToggleTile (2)"));
					toggle2 = true;
				}
			}
		} else if (b == 25) {
			if (tileName == "PhantomTile") {
				if (!toggle3) {
					PhantomScript.Spawn();
					doPhantom = true;
					toggle3 = true;
				}
			} else if (tileName == "HiPressure (1)") {
				if (!toggle1) {
					toggle1 = true;
					GameObject.Find(tileName).GetComponent<MeshRenderer>().material = FinishLine;
				}
			} else if (tileName == "HiPressure (2)") {
				if (!toggle2) {
					toggle2 = true;
					GameObject.Find(tileName).GetComponent<MeshRenderer>().material = FinishLine;
				}
			}
			if (toggle1 && toggle2 && !toggle4) {
				toggle4 = true;
				LoweringTiles.Add(GameObject.Find("ToggleTile"));
			}
		}
	}
}
