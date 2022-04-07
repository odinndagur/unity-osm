using UnityEngine;
using System.Collections;

public class HelpTextScript : MonoBehaviour {
	
	bool helpDisplayed = false;
	public GUIText parent = null;
	
	static string hideText = "Press F1 for controls";
	static string showText = 
		"Left-Click\t\t\t\t\t\t\tDrag\n" +
		"Right Click\t\t\t\t\t\t\tRotate\n" +
		"'c'\t\t\t\t\t\t\t\t\t\tShow OSM loading areas (red = from HDD, blue =  from internet)\n" +
		"WASD\t\t\t\t\t\t\t\tMove\n" +
		"MouseWheel/Shift/Space\tChange Altitude";
	
	// Use this for initialization
	void Start () {
		parent.text = hideText;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.F1)) {
			helpDisplayed = !helpDisplayed;
			
			parent.text = helpDisplayed?showText:hideText;
		}
	}
}
