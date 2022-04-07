using UnityEngine;
using System.Collections;

public class LoadingStateCubeScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		renderer.enabled = SceneOverlordScript.showLoadingCubes;
	}
}
