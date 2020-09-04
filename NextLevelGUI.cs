using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NextLevelGUI : MonoBehaviour {

	public PlayerScript PlayerScript;
	public GameScript EventHandler;
	//public GameObject Controller;

	public Animator GUIAnimator;
	public Text LevelText;
	private void InvokeFadeOut() {GUIAnimator.Play("FadeOut");}

	public Button HomeBtn;
	private bool goingHome;

	void Start () {
		// Connect home button
		HomeBtn.GetComponent<Button>().onClick.AddListener(delegate{
			if (!goingHome) {
				goingHome = true;
				StartCoroutine(NextScene(true));
			}
		});

		// Tell the player which level it is
		int level = SceneManager.GetActiveScene().buildIndex;
		if (level < 10) { // 1 digit levels
			LevelText.text = ("Level 0" + level);
		} else { // 2 digit levels
			LevelText.text = ("Level " + level);
		}

		if (!PlayerScript.Died) { // If the player DIDN'T just die, play the "Level xx" GUI.
			GUIAnimator.Play("NextLevelGUI");
		}
	}

	// Fade out at the end of a level
	private IEnumerator NextScene (bool home) {
		GUIAnimator.Play("FadeOut");
		yield return new WaitForSeconds(2f);

		// Reset camera angle for next level
		CameraScript.cameraAngle = 15f;

		if (home) {
			SceneManager.LoadScene(0);
		} else {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
	}
	public void FadeOut() {StartCoroutine(NextScene(false));} // Allows another script to call this coroutine
}
