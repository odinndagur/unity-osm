using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class LinkedDynamicDataLoader : MonoBehaviour {
	
	enum LoadState {NotLoaded, WaitingForDependencies, InitialiseLoading, Loading, FinishLoading, Loaded};
	//public enum DataType {OpenStreetMap, Altitudes};
	
	Dictionary<string, StatedDataType> states = new Dictionary<string, StatedDataType>();
	Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();
	Dictionary<string, List<string>> reloadHooks = new Dictionary<string, List<string>>();
	HashSet<string> deleted = new HashSet<string>();
	
	class StatedDataType {
		LoadState state;
		public LoadState getState() { return state; } 
		public void setState(LoadState s) { state = s; } 
		
		public DynamicLoader data;
		
		public StatedDataType (DynamicLoader _data) {
			data = _data;
			state = LoadState.NotLoaded;
		}
	}
	
	HashSet<string> toDelete;
	// Use this for initialization
	void Start () {
		toDelete = new HashSet<string>();
	}
	
	public void addDynamicLoader(DynamicLoader loader, string name) {
		states[name] = new StatedDataType(loader);
	}
	
	public void addDependency(string name, string dep) {
		if (!dependencies.ContainsKey(name)) {
			dependencies[name] = new List<string>(1);
		}
		dependencies[name].Add(dep);
		
	}
	
	public void addRestartHook(string name, string hook) {
		if (!reloadHooks.ContainsKey(name)) {
			reloadHooks[name] = new List<string>(1);
		}
		reloadHooks[name].Add(hook);
		
	}
	
	public bool reload(string name) {
		if (states[name].getState() == LoadState.NotLoaded || states[name].getState() == LoadState.Loaded) {
			Debug.Log(name+" NotLoaded/Loaded -> WaitingForDependencies ");
			states[name].setState(LoadState.WaitingForDependencies);
			return true;
		}
		return false;
	}
	
	public bool tryLoad(string name) {
		if (states[name].getState() == LoadState.NotLoaded) {
			Debug.Log(name+" NotLoaded -> WaitingForDependencies ");
			states[name].setState(LoadState.WaitingForDependencies);
			return true;
		}
		return false;
	}
	
	public void tryLoadAll() {
		foreach (string name in states.Keys) {
			tryLoad(name);
		}
	}
	
	public bool isLoaded(string name) {
		if (deleted.Contains(name))
			return true;
		if (!states.ContainsKey(name))
			return false;
		return states[name].getState() == LoadState.Loaded;
	}
	
	public bool contains(string name) {
		return deleted.Contains(name) || states.ContainsKey(name);	
	}
	
	static bool skipLoadFunction = false;
	
	string currentName = "";
	// Update is called once per frame
	void Update () {
		
		toDelete.Clear();
		
		if (currentName.Equals("")) {
			float maxPriority = -1;
//			Debug.Log("# states: "+states.Keys.Count);
			foreach (string n in states.Keys) {
//				Debug.Log(n);
				if (states[n].data.isMarkedForDeletion())
					toDelete.Add(n);
				
				if (states[n].getState() == LoadState.WaitingForDependencies) {
//					Debug.Log("WaitingForDependencies "+n);
					if (!dependencies.ContainsKey(n) || dependencies[n].Count == 0) {
//						Debug.Log(n+" WaitingForDependencies -> InitialiseLoading (no dependencies)");
						states[n].setState(LoadState.InitialiseLoading);
					}
					else {
						bool allDependenciesLoaded = true;
						foreach (string child in dependencies[n]) {
							if (states[child].getState() == LoadState.NotLoaded) {
//								Debug.Log(child+" NotLoaded -> WaitingForDependencies ");
								states[child].setState(LoadState.WaitingForDependencies);
								allDependenciesLoaded = false;
							}
							else if (!isLoaded(child)) {
								allDependenciesLoaded = false;
							}
						}
						if (allDependenciesLoaded) {
//							Debug.Log(n+" WaitingForDependencies -> InitialiseLoading (all dependencies loaded)");
							states[n].setState(LoadState.InitialiseLoading);
						}
					}
						
				}
				
				if (states[n].getState() == LoadState.InitialiseLoading) {
					float priority = states[n].data.getPriority();
					if (maxPriority == -1 || priority > maxPriority) {
						maxPriority = priority;
						currentName = n;
					}
				}
				
			}
			
//			Debug.Log("state: "+currentName+" with priority "+maxPriority+" ("+states.Keys.Count+")");
		}
		
		foreach (string n in toDelete) {
			deleted.Add(n);
			states.Remove(n);
			dependencies.Remove(n);
			reloadHooks.Remove(n);
		}
		
		if (!currentName.Equals("")) {
			//Debug.Log("Check "+type+" = "+states[type].getState().ToString());
			if (states[currentName].getState() == LoadState.InitialiseLoading) {
				// init stuff
				Debug.Log(currentName+" InitialiseLoading -> Loading ");
				if (!skipLoadFunction) states[currentName].data.restartLoad();
				if (!skipLoadFunction) states[currentName].data.initLoad();
				states[currentName].setState(LoadState.Loading);
//				break; // to ensure that only one thing is done per frame
			}
			else if (states[currentName].getState() == LoadState.Loading) {
				//Debug.Log(name+" Loading ");
				if(skipLoadFunction || states[currentName].data.loadData()) {
					Debug.Log(currentName+" Loading -> FinishLoading ");
					states[currentName].setState(LoadState.FinishLoading);
				}
//				break; // to ensure that only one thing is done per frame
			}
			else if (states[currentName].getState() == LoadState.FinishLoading) {
				Debug.Log(currentName+" FinishLoading -> Loaded ");
				if (!skipLoadFunction) states[currentName].data.finialiseLoad();
				states[currentName].setState(LoadState.Loaded);
				
				
				// check for hooks and initialise
				if (reloadHooks.ContainsKey(currentName))
					foreach (string hook in reloadHooks[currentName])
						reload(hook);
				
				currentName = "";
//				break; // to ensure that only one thing is done per frame
			}
			
		}
		
		
	}
	
	
	
}

