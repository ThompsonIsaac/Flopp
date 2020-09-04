using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloppRipple : MonoBehaviour {

	public Transform Flopp;
	private float step = 0; // What color we are currently on
	private Color newColor;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		step += Time.deltaTime * 8f;
		if (step > 60) {
			step -= 60;
		}
		
		foreach (Transform child in Flopp) {
			float myStep = step - (child.position.x / 10f) + (child.position.y / 10f);
			if (myStep < 0) {
				myStep += 60;
			} else if (myStep >= 60) {
				myStep -= 60;
			}

			if (myStep >= 0 && myStep < 10) {
				newColor = new Color (1f, myStep / 10f, 0f, 1f);
			} else if (myStep >= 10 && myStep < 20) {
				newColor = new Color (1f - ((myStep - 10f) / 10f), 1f, 0f, 1f);
			} else if (myStep >= 20 && myStep < 30) {
				newColor = new Color (0f, 1f, (myStep - 20) / 10f, 1f);
			} else if (myStep >= 30 && myStep < 40) {
				newColor = new Color (0f, 1f - ((myStep - 30f) / 10f), 1f, 1f);
			} else if (myStep >= 40 && myStep < 50) {
				newColor = new Color ((myStep - 40f) / 10f, 0f, 1f, 1f);
			} else if (myStep >= 50 && myStep < 60) {
				newColor = new Color (1f, 0f, 1f - ((myStep - 50f) / 10f), 1f);
			}

			child.gameObject.GetComponent<Renderer>().material.color = newColor;
		}
	}
}