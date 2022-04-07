using UnityEngine;
using System.Collections;

public class TweenPosSize : MonoBehaviour {
	
	Vector3 targetPosition;
	Vector3 targetScale;
	
	// Use this for initialization
	void Start () {
		targetPosition = gameObject.transform.localPosition;
		targetScale = gameObject.transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.localPosition = gameObject.transform.localPosition + (targetPosition-gameObject.transform.localPosition)*0.3f;
		gameObject.transform.localScale = gameObject.transform.localScale + (targetScale-gameObject.transform.localScale)*0.2f;
	}
	
	
	public void setTargetPosition(Vector3 t) {
		targetPosition = t;
		
	}
	
	public void setTargetScale(Vector3 t) {
		targetScale = t;
		
	}
}
