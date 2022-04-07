using UnityEngine;
using System.Collections;
using System.Collections.Generic;


class LineRendererDynamicLoader : CollectionIteratedDynamicLoader<Way> {
	
	
	GameObject refToLineRenderRoad = null;
	Material refToPathMaterial = null;
	
	GUIText guiTextLineRenderer;
	
	int nodeCountDown;
	
	
	public LineRendererDynamicLoader(GameObject _refToLineRenderRoad, Material _refToPathMaterial, GUIText guiTextLR, float fraction): base (fraction){
		refToLineRenderRoad = _refToLineRenderRoad;
		refToPathMaterial = _refToPathMaterial;
		guiTextLineRenderer = guiTextLR;
	}
	
	public override float getPriority() {
		return float.PositiveInfinity;	
	}
	
	public override void initLoad() {
//		nodeCountDown = 100;
		setCollection(MapData.wayToProcessDict.Values);
		wayCounter = 0;
		wayHighwayCounter = 0;
		Debug.Log("ways: "+MapData.wayDict.Count);
		Debug.Log("todo: "+MapData.wayToProcessDict.Count);
	}
	
	int wayCounter;
	int wayHighwayCounter;
	
	public override void loadIteration (Way w)
	{

		MapData.wayDict[w.id] = w;
		bool isHighway = false;
		string highway = w.getTag("highway");
		if (highway != null && !highway.Equals("")) {
			isHighway = true;
		}
		//if (firstTime)
		//	print ("highway - |"+highway+"|"+wayColor);
		
		wayCounter++;
		if (isHighway) {
			wayHighwayCounter++;
//			if(nodeCountDown == 0) {
//				MapData.someNodeId = w.NodeID[0];
//				//print ("someNodeId: "+someNodeId);
//				nodeCountDown = -1;
//			}
//			else {
//				nodeCountDown--;
//			}
			GameObject lineRenderRoad;
			LineRenderer lr;
			if (MapData.wayIdToLineRenderer.ContainsKey(w.id)) {
				lr = MapData.wayIdToLineRenderer[w.id];
				lineRenderRoad = lr.gameObject;
				//return;
			}
			else {
				lineRenderRoad = MonoBehaviour.Instantiate(refToLineRenderRoad) as GameObject;
				lr = lineRenderRoad.GetComponent<LineRenderer>();
				MapData.wayIdToLineRenderer[w.id] = lr;
			}
			
			if (!MapData.lineRendererVertexCount.ContainsKey(lr) || MapData.lineRendererVertexCount[lr] != w.NodeID.Count){
				lr.SetVertexCount(w.NodeID.Count);
				MapData.lineRendererVertexCount[lr] = w.NodeID.Count; // HACKKKKKK
			}
			else {
				return;	
			}
			lr.transform.position = new Vector3(0f,0f,0f);
			LineRenderRoadScript lrrScript= lineRenderRoad.GetComponent<LineRenderRoadScript>();
			for (int i = 0 ; i < w.NodeID.Count ; i++) {
				
				
				Node n0 = MapData.nodeDict[w.NodeID[i]] as Node;
				//Debug.Log(n0.Latitude+":"+n0.Longitude);
				Vector3 normPos = MapData.normalizePosition(n0.Longitude, n0.Latitude);
				lr.SetPosition(i, normPos);
				lrrScript.setPosition(normPos);
				
				
				if (highway.Equals("motorway") || highway.Equals("motorway_link")) {
//						lr.SetWidth(0.3f,0.3f);
					lr.SetWidth(0.6f,0.6f);
					lrrScript.setType(MapData.RoadType.Motorway);
				}
				else if (highway.Equals("trunk") || highway.Equals("trunk_link")) {
//						lr.SetWidth(0.25f,0.25f);
					lr.SetWidth(0.5f,0.5f);
					lrrScript.setType(MapData.RoadType.Trunk);
				}
				else if (highway.Equals("primary") || highway.Equals("primary_link")) {
//						lr.SetWidth(0.2f,0.2f);
					lr.SetWidth(0.4f,0.4f);
					lrrScript.setType(MapData.RoadType.Primary);
				}
				else if (highway.Equals("secondary") || highway.Equals("secondary_link")) {
//						lr.SetWidth(0.16f,0.16f);
					lr.SetWidth(0.32f,0.32f);
				}
				else if (highway.Equals("tertiary") || highway.Equals("tertiary_link")) {
//						lr.SetWidth(0.12f,0.12f);
					lr.SetWidth(0.24f,0.24f);
					lrrScript.setType(MapData.RoadType.Tertiary);
				}
				else if (highway.Equals("living_street") || highway.Equals("residential")) {
//						lr.SetWidth(0.1f,0.1f);
					lr.SetWidth(0.2f,0.2f);
					lrrScript.setType(MapData.RoadType.LivingStreet);
				}
				else if (highway.Equals("unclassified") || highway.Equals("road")) {
//						lr.SetWidth(0.1f,0.1f);
					lr.SetWidth(0.2f,0.2f);
					lrrScript.setType(MapData.RoadType.Unclassified);
				}
				else {
//						lr.SetWidth(.06f,.06f);
					lr.SetWidth(0.12f,0.12f);
					lrrScript.setType(MapData.RoadType.Unclassified);
				}
				
				if (highway.Equals("path") || 
					highway.Equals("footway") || 
					highway.Equals("cycleway") || 
					highway.Equals("bridleway") || 
					highway.Equals("steps") ||
					highway.Equals("pedestrian")) {
					
					lr.material = refToPathMaterial;
//						lr.SetWidth(.05f,.05f);
					lr.SetWidth(.1f,.1f);
					lrrScript.setType(MapData.RoadType.FootPath);
				}
				//lr.SetWidth(.02f,.02f);
				
			}
//				
			for (int i = 0 ; i < w.NodeID.Count-1 ; i++) {
				Node prevNode = MapData.nodeDict[w.NodeID[i]] as Node;
				Node nextNode = MapData.nodeDict[w.NodeID[i+1]] as Node;
				prevNode.clearNodes();
				prevNode.addNode(w.NodeID[i+1]);
				nextNode.addNode(w.NodeID[i]);
			}
		}
		guiTextLineRenderer.text = "LineRenderers: "+MapData.wayIdToLineRenderer.Count;
	}
	
	public override void finialiseLoad() {
		
		MapData.wayToProcessDict.Clear();
		
		// connect all the nodes
//		foreach (Way w in MapData.wayDict.Values) {
//		for (int i = 0 ; i < w.NodeID.Count-1 ; i++) {
//				Node prevNode = MapData.nodeDict[w.NodeID[i]] as Node;
//				Node nextNode = MapData.nodeDict[w.NodeID[i+1]] as Node;
//				prevNode.clearNodes();
//				prevNode.addNode(w.NodeID[i+1]);
//				nextNode.addNode(w.NodeID[i]);
//			}
//		}
//			
//		}
		Debug.Log ("Counted ways: "+wayCounter);
		Debug.Log ("Counted high(ways): "+wayHighwayCounter);
		Debug.Log ("Rendererd ways: "+MapData.wayIdToLineRenderer.Count);
		
		
	
	}
	
}

