using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class CollectionIteratedDynamicLoader<T> : DynamicLoader {

	ICollection collection = null;
	int itemsProcessed = 0;
	bool loadingDone = false;
	
	float fractionOfFrameRate = 0.2f;
	
	public CollectionIteratedDynamicLoader (float fraction) {
		fractionOfFrameRate = fraction;
	}
	
	public void setCollection(ICollection i) {
		collection = i;
	}
	
	public void finishLoading() {
		loadingDone = true;
	}
	
	
	public override void restartLoad() {
		collection = null;
	    loadingDone = false;
		itemsProcessed = 0; // TODO Hack! make sure that, when reloading this initLoad is called and then calls its subclass
	}
	
//	public abstract void initLoad();
	
	// returns true when loading is complete, false if loading is still to be done
	public override bool loadData() {
		
		if (loadingDone)// in case loading is finished in the init stage
			return true;

		float startTime = Time.realtimeSinceStartup;
		int i = 0;
		foreach(T t in collection) { // did you setCollection ?
			
			//Debug.Log(wi +" < "+waysProcessed);
			if (i < itemsProcessed) {
				i++;
				continue;
			}
			
			loadIteration(t);
			if (loadingDone)
				return true;
			i++;
			//Debug.Log(wi +" > "+(waysProcessed+5));
			float timeElapsed = Time.realtimeSinceStartup - startTime;
//			if (timeElapsed > (Time.deltaTime*fractionOfFrameRate)) { // fraction of a frame
			if (timeElapsed > (fractionOfFrameRate)) { // fraction of a frame
//				Debug.Log("number processed "+(i-itemsProcessed));
				itemsProcessed = i;
				return false;
			}
			
		}
		return true;
			
	}
	
	
	public abstract void loadIteration (T t); // returns true if loading is complete, false if loading is still to be done
	
//	public abstract void finialiseLoad();
	
}