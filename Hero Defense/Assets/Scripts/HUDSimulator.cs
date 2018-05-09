using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDSimulator : MonoBehaviour {



	public bool isImgOn;
	public Image img;

	void Start () {

		img.enabled = true;
		isImgOn = true;
	}

	void Update () {

		if (Input.GetKeyDown ("i")) {

			if (isImgOn == true) {

				img.enabled = false;
				isImgOn = false;
			}

			else {

				img.enabled = true;
				isImgOn = true;
			}
		}
	}
}
