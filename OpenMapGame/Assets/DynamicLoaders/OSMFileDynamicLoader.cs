using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;


public class OSMFileDynamicLoader : OSMFileLoader {
	
	
	Progressable prog = null;
	
	StringBuilder text;
	bool success;
	System.IO.StreamReader reader;
	System.IO.FileStream fileStream;
	
	char [] buff = null;
	
	static int bytesToRead = 1024*32;
	
	public OSMFileDynamicLoader(OSMBounds b, Progressable p, float fraction) : base (b, fraction) {
		
		text = new StringBuilder();
		
		prog = p;
		
		Vector3 minCorner = MapData.llToUnits(bounds.minLon, bounds.minLat);
		Vector3 maxCorner = MapData.llToUnits(bounds.maxLon, bounds.maxLat);
		prog.gameObject.transform.localScale = new Vector3(maxCorner.x - minCorner.x, 50, maxCorner.z - minCorner.z);
		prog.gameObject.transform.position = new Vector3(minCorner.x+(maxCorner.x - minCorner.x)/2, 25, minCorner.z+(maxCorner.z - minCorner.z)/2);
		
		prog.setColors(new Color(1f,0f,0f,0.1f), new Color(0f,1f,0f,0.1f));
		prog.setProgress(0f);
		
		MapData.loadingBlockCallBack();
	}
	
	public override string getData() {
		return text.ToString();	
	}
	
	public override OSMBounds getBounds() {
		return bounds;
	}
	
	public override bool succeeded() {
		return success;
	}
	
	public override void initLoad() {
		reader = null;
		string filename = bounds.getFileName();
		success = false;
		if (!System.IO.File.Exists(filename)) {
			finishLoading();
			return;
		}
			
		fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
		
		reader = new System.IO.StreamReader(fileStream);
		
		buff = new char[bytesToRead];
	}
	
	public override void loadIteration(){
		int len = reader.ReadBlock(buff,0,bytesToRead);
//		Debug.Log(len);
		text.Append(buff,0, len);
		
		
		prog.setProgress((float)fileStream.Position/fileStream.Length);
		//Debug.Log(fileStream.Position+"/"+fileStream.Length);
		
		if (reader.EndOfStream) {
			success = true;
			finishLoading();
		}
	}
	
	public override void finialiseLoad(){
		prog.setProgress(1.0f);
		if (reader != null)
			reader.Close();
		buff = null;
	}
	
	
	
	
}

