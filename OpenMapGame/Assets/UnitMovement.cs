using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour {
	
	int atNode = -1;
	int fromNode = -1;
	float distanceAlongEdge = 0.0f;
	
	float speed = 0.1f; // distance per frame
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.G)){
//			print ("Destinations:" + destinations.Count);
			moveUnitIfHaveLocation(speed);
		}
		if (Input.GetKeyDown(KeyCode.H)) {
			lookForPathTo(291434863);
		}
	}
	
	private void moveUnitIfHaveLocation(float speed) {
		if (destinations.Count > 0) {
//			print ("Destinations:" + destinations.Count);
			int nextloc = destinations.Peek();
			moveUnitToLocation(nextloc, speed);
			
			updateLocation ();
		}
	}
	
	public void moveUnitToLocation(int nodeId, float speed) {
		
//		print ("atNode:" + atNode);
		if (atNode != -1) { // we are at a node
			
			float distance = MapData.distanceBetweenNodes(atNode, nodeId);
			
			if (speed < distance) { // we will not reach target node in one step
				// go part of the way
				distanceAlongEdge += speed;
				fromNode = atNode;
				atNode = -1;
			}
			else { //(speed >= distance) // we will reach other node, with some distance still to travel
				// we have reached the destination node
				
				atNode = nodeId;
				fromNode = -1;
				distanceAlongEdge = 0.0f;
				destinations.Dequeue();
				
				float speedRemainig = speed-distance;
				
				// if unit has another target, move it there
				moveUnitIfHaveLocation(speedRemainig);
				
			}
		}
		else { // we are along an edge
			System.Diagnostics.Debug.Assert(fromNode != -1);
			System.Diagnostics.Debug.Assert(destinations.Count > 0);	
			float distance = MapData.distanceBetweenNodes(fromNode, nodeId);
			float remainingDistance = distance-distanceAlongEdge;
			if (speed < remainingDistance) { // we will not reach target node in one step
				distanceAlongEdge += speed;
			}
			else { //(speed >= remainingDistance) // we will reach other node, with some distance still to travel
				
				atNode = nodeId;
				fromNode = -1;
				distanceAlongEdge = 0.0f;
				destinations.Dequeue();
				
				float speedRemainig = speed-remainingDistance;
				
				// if unit has another target, move it there
				moveUnitIfHaveLocation(speedRemainig);
			}
		}
	}
	
	Queue<int> destinations = new Queue<int>();
	
	public void setUnitLocation(int nodeId) {
	
		atNode = nodeId;
		
		updateLocation();
		
	}
	
	private class IdAndDistTraveled {
	
		public IdAndDistTraveled (int _id, float dTrav, float distToGo) {
			id = _id;
			distTraveled = dTrav;
			distanceToGo = distToGo;
		}
		
		public int id = -1;
		public int parentId = -1;
		public float distTraveled = 0.0f;
		public float distanceToGo = 0.0f;
		
		public float getHeuristic() {
			return distTraveled+distanceToGo;	
		}
		
		public int CompareTo(IdAndDistTraveled other)
        {
			float diff = getHeuristic() - other.getHeuristic();
            return (int)Mathf.Sign(diff);
        }
	};
	
	int ByHeuristic(IdAndDistTraveled leftMost, IdAndDistTraveled rightMost)
	{
	    return (int)Mathf.Sign(leftMost.getHeuristic() - rightMost.getHeuristic()); // TODO this infinite loops!!! :(
	}
	
	public void lookForPathTo(int destPoint){
		// for now use nodeAt or node from as start point
		int startPoint = -1;
		if (atNode != -1) startPoint = atNode;
		else startPoint = destinations.Peek ();
		
		
		HashSet<int> visitedNodes = new HashSet<int>();
		Dictionary<int, int> nodeIdToParent = new Dictionary<int, int>();
		List<IdAndDistTraveled> priorityQ = new List<IdAndDistTraveled>();
		
		priorityQ.Add (new IdAndDistTraveled(startPoint, 0.0f, MapData.distanceBetweenNodes(startPoint, destPoint)));
		visitedNodes.Add(startPoint);
		nodeIdToParent.Add (startPoint, -1);
		
//		print ("adding start point to list: "+startPoint);
		
		int failedSourceId = -1;
		bool foundDest = false;
		//while(true) {
		for (int i = 0 ; i < 1000 ; i++) {
//			print ("priorityQ size: "+priorityQ.Count);	
			if (priorityQ.Count == 0) {
//				print ("no path found");
				break;
			}
			priorityQ.Sort(ByHeuristic);
			if(i == 1000-1) {
				print ("last loop");
				
			}
			
			IdAndDistTraveled idDist = priorityQ[0];
			priorityQ.RemoveAt(0);
//			print ("have: "+idDist.id +" (" +idDist.distTraveled+ ") ... "+destPoint);
			
			if (idDist.id == destPoint){
//				print ("found dest");
				foundDest = true;
				// do something
				break;
			}
			
			Node n = MapData.getNode(idDist.id);
			int numberOfChildren = 0;
			foreach (int connNodeId in n.ConnectedNodeIds) {
//				print ("child "+connNodeId +" : "+visitedNodes.Contains(connNodeId));
				if (visitedNodes.Contains(connNodeId)) {
					continue; // todo if distance traveled so far is better than previous one, rather use this path than current one
				}
				numberOfChildren ++;
				
//				print ("adding to list");
				float traveledSoFar = idDist.distTraveled + MapData.distanceBetweenNodes(idDist.id, connNodeId);
				float distToGo = MapData.distanceBetweenNodes(connNodeId,destPoint);
				
				priorityQ.Add(new IdAndDistTraveled(connNodeId, traveledSoFar, traveledSoFar+distToGo));
				visitedNodes.Add(connNodeId);
				nodeIdToParent.Add(connNodeId, idDist.id);
//				print ("dist to go: " + distToGo);
			}
			if (numberOfChildren == 0) { // means this path is coming to an end
				// make a linerender
				
				failedSourceId = idDist.id;
				
				
				
			}
		}
		
		int idToDraw = -1;
		if (foundDest) {
			idToDraw = destPoint;
			List<int> ids = new List<int>();
			int currId = destPoint;
			while(currId != -1){
				ids.Insert(0, currId);
				currId = nodeIdToParent[currId];
			}
			foreach (int nodeId in ids) {
				queueDestinationNode(nodeId);	
			}
		}
		else {
			if (failedSourceId != -1) {
				idToDraw = failedSourceId;
			}
		}
		
		if (idToDraw != -1) {
			LineRenderer path = Instantiate(lineRenderer) as LineRenderer;
			path.material = redMaterial;
			
			List<Vector3> pos = new List<Vector3>();
			
			int currentId = idToDraw;
			while(currentId != -1) {
				
				//print (MapData.normalizePosition(currentId));
				pos.Add(MapData.normalizePosition(currentId)+Vector3.up*0.1f);
				
				currentId = nodeIdToParent[currentId];
			}
			
			path.SetVertexCount(pos.Count);
			int i = 0;
			foreach(Vector3 v in pos) {
				path.SetPosition(i, v);
				i++;
			}
			path.SetWidth(0.05f,0.05f);	
				
		}
		
	}
	
	public LineRenderer lineRenderer;
	public Material redMaterial;
	
	public void updateLocation () {
	
		if (atNode != -1) {
			Node n = MapData.nodeDict[atNode] as Node;
			transform.position = MapData.normalizePosition(n);
			
//			print ("at node: "+atNode);
//			print ("connected nodes: ");
//			foreach (int nId in n.ConnectedNodeIds) {
//				print ("  "+nId);
////				foreach (int nId2 in MapData.getNode(nId).ConnectedNodeIds) {
////					print ("    "+nId2);
////				}
//			}
//			if (destinations.Count > 0)
//				print ("destination: " + destinations.Peek());
//			else
//				print ("destination: none");
		}
		else {
			System.Diagnostics.Debug.Assert(fromNode != -1);
			System.Diagnostics.Debug.Assert(destinations.Count > 0);
			
			Node n0 = MapData.nodeDict[fromNode] as Node;
			Node n1 = MapData.nodeDict[destinations.Peek()] as Node;
			
			float dist = MapData.distanceBetweenNodes(n0, n1);
			float ratioAlongEdge = distanceAlongEdge/dist;
			
			transform.position = (MapData.normalizePosition(n1)*ratioAlongEdge) + (MapData.normalizePosition(n0)*(1.0f-ratioAlongEdge));
			
//			print ("from node: "+fromNode);
//			print ("to node:   "+destinations.Peek());
//			print ("dist along:"+distanceAlongEdge+"/"+dist);
		}
	}
	
	
	public void queueDestinationNode(int nodeId) {
		destinations.Enqueue(nodeId);
	}
}
