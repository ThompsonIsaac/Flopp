using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour {

	static private bool hasLoaded = false;

	// Keep playing the music regardless of the scene we are on.
	void Awake () {
		if (!hasLoaded) {
			DontDestroyOnLoad(transform.gameObject);
			hasLoaded = true;
		} else { // Prevent duplicate on return to main menu.
			Destroy(transform.gameObject);
		}
	}

}
