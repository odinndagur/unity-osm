using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class IteratedDynamicLoader : DynamicLoader {

	int itemsProcessed = 0;
	bool loadingDone = false;
	
	float fractionOfFrameRate = 0.2f;
	
	public IteratedDynamicLoader (float fraction) {
		fractionOfFrameRate = fraction;
	}
	
	
	public void finishLoading() {
		loadingDone = true;
	}
	
	public override void restartLoad(){
		loadingDone = false;
		itemsProcessed = 0;
	}
	
//	public abstract void initLoad();
	
	// returns true when loading is complete, false if loading is still to be done
	public override bool loadData() {
		
		if (loadingDone) // in case loading is finished in the init stage
			return true;

		float startTime = Time.realtimeSinceStartup;
		int i = 0;
		while (true) { // did you setCollection ?
			
			//Debug.Log(wi +" < "+waysProcessed);
			if (i < itemsProcessed) {
				i++;
				continue;
			}
			
			loadIteration();
			if (loadingDone)
				return true;
			i++;
			//Debug.Log(wi +" > "+(waysProcessed+5));
			float timeElapsed = Time.realtimeSinceStartup - startTime;
//			if (timeElapsed > (Time.deltaTime*fractionOfFrameRate)) { // fraction of a frame
			if (timeElapsed > (fractionOfFrameRate)) { // fraction of a frame
				//Debug.Log("number processed "+(i-itemsProcessed));
				itemsProcessed = i;
				return false;
			}
			
		}
			
	}
	
	
	public abstract void loadIteration (); // returns true if loading is complete, false if loading is still to be done
	
//	public abstract void finialiseLoad();
	
}