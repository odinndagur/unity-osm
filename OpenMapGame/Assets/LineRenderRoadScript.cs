using UnityEngine;
using System.Collections;

public class LineRenderRoadScript : MonoBehaviour {
	
	public LineRenderer thisLineRenderer = null;
	
	MapData.RoadType roadType;
	float drawDist = 1000;
	
	Vector3 lrPosition;
	
	float timeToSwitchVisible = 0f;
	float startTime = 0;
	// Use this for initialization
	void Start () {
		timeToSwitchVisible = Random.value*2;
		startTime = Time.realtimeSinceStartup;
		renderer.enabled = false;
	}
	
	public void setPosition(Vector3 pos) {
		lrPosition = pos;
	}
	public void setType(MapData.RoadType r) {
		roadType = r;
		
		switch(roadType) {
		case MapData.RoadType.Motorway:
		case MapData.RoadType.Trunk:
			drawDist = 2000;
			break;
		case MapData.RoadType.Primary:
			drawDist = 1200;
			break;
		case MapData.RoadType.Secondary:
			drawDist = 7500;
			break;
		case MapData.RoadType.Tertiary:
			drawDist = 500;
			break;
		case MapData.RoadType.LivingStreet:
		case MapData.RoadType.FootPath:
			drawDist = 300;
			break;
		case MapData.RoadType.Unclassified:
			drawDist = 300;
			break;
		
		}
		drawDist += Random.value*20;
	}
	
	// Update is called once per frame
	void Update () {
	
		if(timeToSwitchVisible != -1 && Time.realtimeSinceStartup > startTime+timeToSwitchVisible) {
			renderer.enabled = !renderer.enabled;
			timeToSwitchVisible = -1;
		}
		Vector3 cameraPos = Camera.mainCamera.transform.localPosition;// cameraPos.y = 0;
		Vector3 lrPos = lrPosition;// lrPos.y = 0;
		float magSq = Vector3.SqrMagnitude(cameraPos - lrPos);
		//Debug.Log(mag);
//		renderer.enabled = mag < drawDist;
		if (magSq < drawDist*drawDist) { // is in range
			if (!renderer.enabled && timeToSwitchVisible == -1) { // if it is hiding and it isn't queued to live/hide
				timeToSwitchVisible = Random.value*1f;
				startTime = Time.realtimeSinceStartup;
			}
		}
		else { // is out of range
			if (renderer.enabled && timeToSwitchVisible == -1) { // if it is invibible and it isn't queued to live/hide
				timeToSwitchVisible = Random.value*1f;
				startTime = Time.realtimeSinceStartup;
			}
		}

	}
}
