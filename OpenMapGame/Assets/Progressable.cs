using UnityEngine;
using System.Collections;

public class Progressable : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	Color start;
	Color end;
	
	public void setColors(Color s, Color e) {
		start = s;
		end = e;
	}
	
	public void setProgress(float p) {
		renderer.material.color = new Color(start.r*(1f-p)+end.r*p,
											start.g*(1f-p)+end.g*p,
											start.b*(1f-p)+end.b*p,
											start.a*(1f-p)+end.a*p);
	}
}
