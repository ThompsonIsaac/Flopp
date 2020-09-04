using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClass : MonoBehaviour {

	private int myLevel;
	private GameObject Menu; // References MainMenu object

	private void Start () { // Determine this button's level number and get in position
		Menu = GameObject.Find("Menu"); // Find menu object
		myLevel = int.Parse(this.name.Substring(8, 2));
		//this.transform.position = new Vector3(
		//		((myLevel - 1) % 7 - 3) * 12, -1 * ((myLevel - 1) / 7 - 1.5f) * 10, 0);
		this.transform.GetChild(0).gameObject.GetComponent<Text>().text = (myLevel)+"";

		if (SaveLoad.levelNo >= myLevel) {
			this.GetComponent<Button>().onClick.AddListener(ContinueGame);
		} else {
			this.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);
		}
	}

	private void ContinueGame () { // After one of the level select buttons has been pressed,
		Menu.GetComponent<MainMenu>().levelSelected = true;
		Menu.GetComponent<Animator>().Play("ContGame"); // Play the menu's animation
		StartCoroutine(Menu.GetComponent<MainMenu>().WaitAndStart(
			this.GetComponent<ButtonClass>().myLevel)); // Start the coroutine countdown to start
	}
}
