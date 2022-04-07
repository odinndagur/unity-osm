using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


public class OSMXAPIDynamicLoader : OSMFileLoader  {
	
	string text;
	bool success;
	Progressable prog = null;
	
	public OSMXAPIDynamicLoader(OSMBounds b, Progressable p, float frac) : this(b,p) {	}
	
	public OSMXAPIDynamicLoader(OSMBounds b, Progressable p) : base (b, 0f) {
		prog = p;
		
		Vector3 minCorner = MapData.llToUnits(bounds.minLon, bounds.minLat);
		Vector3 maxCorner = MapData.llToUnits(bounds.maxLon, bounds.maxLat);
		prog.gameObject.transform.localScale = new Vector3(maxCorner.x - minCorner.x, 50, maxCorner.z - minCorner.z);
		prog.gameObject.transform.position = new Vector3(minCorner.x+(maxCorner.x - minCorner.x)/2, 25, minCorner.z+(maxCorner.z - minCorner.z)/2);
		// TODO this is done twice
		prog.setColors(new Color(0f,0f,1f,0.1f), new Color(0f,1f,0f,0.1f));
		prog.setProgress(0f);
		
		
		MapData.loadingBlockFromInternetCallBack();
	}
	
	
	
	public override OSMBounds getBounds() {
		return bounds;
	}
	
	public override string getData() {
		return text;	
	}
	
	public override bool succeeded() {
		return www.isDone;
	}
	
	WWW www = null;
	
	public override void initLoad() {
		
		Debug.Log("Attempting XAPI file download");
		
		//bounds found (-33.9632:-33.8956,18.3634:18.4675)
		//http://www.overpass-api.de/api/xapi?*[bbox=7.1,51.2,7.2,51.3][highway=*]
		//http://www.overpass-api.de/api/xapi?*[bbox=18.3634,-33.9632,18.4675,-33.8956]
		//http://www.overpass-api.de/api/xapi?*[bbox=18.3634,-33.9632,18.4675,-33.8956][highway=*]
		//http://www.overpass-api.de/api/xapi?*[bbox=18.3634,-33.9632,18.4675,-33.8956]
		
//		string url = "http://www.overpass-api.de/api/xapi?*[bbox="+MapData.bounds+"][highway=*]";
//		string url = "http://www.overpass-api.de/api/xapi?*[bbox="+MapData.bounds+"]";
		
//		string url = "http://www.overpass-api.de/api/xapi?way[highway=*][bbox="+bounds+"]";
		string url = bounds.getAPICall();
		
		Debug.Log(url);
		www = new WWW(url);
		

		
	}
	//small example http://www.overpass-api.de/api/xapi?*[bbox=18.37,-33.94:18.39,-33.93][highway=*]
	
	
	public override void loadIteration(){
		
		prog.setProgress(0.5f);
//		Debug.Log("progress: "+www.progress +" from "+www.url);
		if (www.isDone) {
			text = www.text;
			finishLoading();
		}
	}
	
	public override void finialiseLoad(){
		string filename = bounds.getFileName();
		
		if (!System.IO.File.Exists(filename)) {
			System.IO.StreamWriter sw = new System.IO.StreamWriter(new System.IO.FileStream(filename,System.IO.FileMode.Create));
			sw.Write(text);
			sw.Close();
		}
		prog.setProgress(1.0f);
		
	}
	
	
	
	
}
