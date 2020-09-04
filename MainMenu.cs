using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public Button StartBtn;
	public Button BackBtn;
	public Text StartBtnText;
	public Animator MenuAnimator;
	public GameObject LevelBtns;

	public bool levelSelected = false; // Used to disable back button once game is starting

	public IEnumerator WaitAndStart(int level) {
		Debug.Log("Now waiting for level start " + level);
		yield return new WaitForSeconds(3);
		PlayerScript.Died = false; // Erase memory of the player having died
		Debug.Log("Load scene now.");
		SceneManager.LoadScene(level);
	}

	// Start the game
	void StartGame (int level) {
		MenuAnimator.Play("StartGame");
		StartCoroutine(WaitAndStart(level));
	}

	void BackToMenu() {
		if (!levelSelected) {
			MenuAnimator.Play("BackToMenu");
		}
	}

	// On main menu opening, load level progress.
	void Awake() {
		SaveLoad.Load();
	}

	// Set up the interface.
	void Start () {
		if (SaveLoad.levelNo == 1) { // Start from scratch
			StartBtnText.text = "Start";
			StartBtn.GetComponent<Button>().onClick.AddListener(delegate{StartGame(1);});

		} else { // Start button is replaced with level select button
			StartBtnText.text = "Level Select";
			StartBtn.GetComponent<Button>().onClick.AddListener(delegate{MenuAnimator.Play("LevelSelect");});
			// The back button allows to return FROM the level select screen
			BackBtn.GetComponent<Button>().onClick.AddListener(BackToMenu);
		}
	}
}