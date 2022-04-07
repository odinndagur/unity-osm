using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class DynamicLoader {
	
	public abstract void restartLoad();//hack
	
	public abstract void initLoad();
	
	// returns true when loading is complete, false if loading is still to be done
	public abstract bool loadData();
	
	public abstract void finialiseLoad();
	
	bool markedForDelete = false;
	public void markForDeletion() {
		markedForDelete = true;
	}
	
	public bool isMarkedForDeletion() {
		return markedForDelete;	
	}
	
	public virtual float getPriority() {
		return 0f;	
	}
	
}

