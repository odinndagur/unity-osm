using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class LLBounds {
	
	public LLBounds () {
	}
	// old: right,top,left,bottom

	// new: left,bottom,right,top
	public LLBounds (double _minLon, double _minLat, double _maxLon, double _maxLat) {
		minLat = _minLat;
		minLon = _minLon;
		maxLat = _maxLat;
		maxLon = _maxLon;
	}
	
	public double minLat = double.NaN;
	public double minLon = double.NaN;
	public double maxLat = double.NaN;
	public double maxLon = double.NaN;
	
	public override string ToString() {
		return ""+minLon+","+minLat+","+maxLon+","+maxLat;
	}
	
	public string boundString () {
		return minLon.ToString("0.##")+","+minLat.ToString("0.##")+","+maxLon.ToString("0.##")+","+maxLat.ToString("0.##");
	}
	
	
}

public class OSMBounds : LLBounds {
	
	MapData.RoadType roadType;
	public OSMBounds (double _minLon, double _minLat, double _maxLon, double _maxLat, MapData.RoadType rt) : base(_minLon, _minLat, _maxLon, _maxLat) {
		roadType = rt;
	}
	
	public string getFileName(){
		return "CustomAssets/OSMchunks/"+roadType.ToString()+"_"+(maxLon-minLon).ToString("0.##")+"_"+(maxLat-minLat).ToString("0.##")+"_map_"+boundString()+".osm";	
	}
	public string getAPICall(){
		string optionstring = "";
		
		switch(roadType) {
		case MapData.RoadType.Motorway:
		case MapData.RoadType.Trunk:
			optionstring = "[highway=trunk|trunk_link|motorway|motorway_link]";
			break;
		case MapData.RoadType.Primary:
			optionstring = "[highway=primary|primary_link]";
			break;
		case MapData.RoadType.Secondary:
			optionstring = "[highway=secondary|secondary_link]";
			break;
		case MapData.RoadType.Tertiary:
			optionstring = "[highway=tertiary|tertiary_link]";
			break;
		case MapData.RoadType.LivingStreet:
			optionstring = "[highway=living_street|residential]";
			break;
		case MapData.RoadType.FootPath:
		case MapData.RoadType.Unclassified:
			optionstring = "[highway=path|footway|cycleway|bridleway|steps|pedestrian|unclassified|road]";
			break;
		}
//		switch(roadType) {
//		case MapData.RoadType.Motorway:
//			optionstring = "[highway=moterway|moterway_link]";
//			break;
//		case MapData.RoadType.Trunk:
//			optionstring = "[highway=trunk|trunk_link]";
//			break;
//		case MapData.RoadType.Primary:
//			optionstring = "[highway=primary|primary_link]";
//			break;
//		case MapData.RoadType.Secondary:
//			optionstring = "[highway=secondary|secondary_link]";
//			break;
//		case MapData.RoadType.Tertiary:
//			optionstring = "[highway=tertiary|tertiary_link]";
//			break;
//		case MapData.RoadType.LivingStreet:
//			optionstring = "[highway=living_street|residential]";
//			break;
//		case MapData.RoadType.FootPath:
//			optionstring = "[highway=path|footway|cycleway|bridleway|steps|pedestrian]";
//			break;
//		case MapData.RoadType.Unclassified:
//			optionstring = "[highway=unclassified|road]";
//			break;
//		
//		}
		return "http://www.overpass-api.de/api/xapi?way"+optionstring+"[bbox="+boundString()+"]";
//		return "CustomAssets/OSMchunks/"+roadType.ToString()+"_"+(maxLon-minLon).ToString("0.##")+"_"+(maxLat-minLat).ToString("0.##")+"_map_"+boundString()+".osm";	
	}
	
}

public class Node {
	
	public Node(int _id, double _lon, double _lat) {
		id = _id;	
		lon = _lon;
		lat = _lat;
	}
	
	int id = -1;
	double lon = -1;
	public double Longitude { get { return lon; } }
	double lat = -1;
	public double Latitude { get { return lat; } }
	
	List<int> connectedNodes = new List<int>();
	public List<int> ConnectedNodeIds { get { return connectedNodes; } }
	
	public void clearNodes() {
		connectedNodes.Clear();	
	}
	
	public void addNode (int node_id) {
		connectedNodes.Add(node_id);
	}
	
}


public class Way {
	
	
	public Way(int _id) {
		id = _id;
	}
	
	public int id = -1;
	
	public string name = "";

	
	List<int> nodeIDs = new List<int>();
	
	Hashtable tags = new Hashtable();
	public Hashtable Tags { get { return tags; } }
	
	public void addTag (string k, string v) {
		if (k == "name")
			name = v;
		else
			tags.Add(k,v);
	}
	
	public string getTag(string k) {
		return tags[k] as string;	
	}
	
	public void addNodeID (int nodeID) {
		nodeIDs.Add(nodeID);
	}
	
	public List<int> NodeID { get { return nodeIDs; } }
	
//	public List<int> getNodeID () {
//		return nodeIDs;
//	}
	
}

public class Relation : Way {
	
	public Relation(int _id) : base (_id){
	}
	
	public class Member {
		public Member (int id_r, string _role) {
			id_ref = id_r;
			role = _role;
		}
		public int id_ref = -1;
		public string role = "";
	}
	
	public void addMember (string type, int id, string role) {
		if (type == "node") {
			nodeMembers.Add(new Member(id, role));
		} 
		else if (type == "way") {
			wayMembers.Add(new Member(id, role));
		}
	}
	
	List<Member> nodeMembers = new List<Member>();
	public List<Member> NodeMembers { get { return nodeMembers; } }
	
	List<Member> wayMembers = new List<Member>();
	public List<Member> WayMembers { get { return wayMembers; } }
}

public class TerrainDetails {

	public Terrain terrain;
	public LLBounds targetBounds;
	public LLBounds bounds;
	
	public TerrainDetails(LLBounds bounds, Terrain t) {
		targetBounds = bounds;
		terrain = t;
	}
	
}

public static class MapData {
	
	public class OsmRegion {
		
		public LLBounds region;
		
		OsmRegion() {
		
		}
	}
	
	public enum RoadType {Motorway, Trunk, Primary, Secondary, Tertiary, LivingStreet, FootPath, Unclassified};
	
	public static double LLtoUnitsRatio = 0.000833333333;
	
	public static TerrainDetails terrainDetails;
	public static string xapiData;
	
//	public static Terrain terrain;
	//public static float [,] heightData;
//	public static LLBounds heightBounds;
	
	public static GUIText guiTextBlockInfo = null;
	static int blocksLoaded = 0;
	static int blocksLoading = 0;
	static int blocksLoadingFromInternet = 0;
	public static void loadingBlockCallBack(){
		blocksLoading++;
		setBlockLoadText();
	}
	public static void loadingBlockFromInternetCallBack(){
		blocksLoadingFromInternet++;
		setBlockLoadText();
	}
	public static void loadedBlockCallBack(){
		blocksLoaded++;
		setBlockLoadText();
	}
	public static void setBlockLoadText() {
		guiTextBlockInfo.text = "Blocks "+blocksLoaded+"/"+(blocksLoading+blocksLoadingFromInternet)+" ("+blocksLoadingFromInternet+" from internet)"+
			(SceneOverlordScript.showLoadingCubes?" - displayed":" - hidden");
		
	}
	public static List<OsmRegion> osmRegions;
//	public static LLBounds bounds;
	public static Hashtable nodeDict;
	public static Hashtable wayDict;
	public static Hashtable wayToProcessDict;
	public static Hashtable relationDict;
	
	public static Dictionary<int, LineRenderer> wayIdToLineRenderer;
	
	public static Dictionary<LineRenderer, int> lineRendererVertexCount;
	
	public static bool insideTerrain(LLBounds b) {
//		Debug.Log("----");
//		Debug.Log(terrainDetails.bounds.minLat +" < "+ b.minLat+":"+b.maxLat+" < "+ terrainDetails.bounds.maxLat+" && "+ terrainDetails.bounds.minLon +" < "+ b.minLon+":"+b.maxLon+" < "+ terrainDetails.bounds.maxLon);
//		Debug.Log(terrainDetails.bounds.minLat +" < "+ b.minLat+":"+b.maxLat+" < "+ terrainDetails.bounds.maxLat+" && "+ terrainDetails.bounds.minLon +" < "+ b.minLon+":"+b.maxLon+" < "+ terrainDetails.bounds.maxLon);
//		Debug.Log(terrainDetails.bounds.maxLat +" < "+ b.minLat+":"+b.maxLat+" < "+ terrainDetails.bounds.maxLat+" && "+ terrainDetails.bounds.minLon +" < "+ b.minLon+":"+b.maxLon+" < "+ terrainDetails.bounds.maxLon);
//		Debug.Log(terrainDetails.bounds.minLat +" < "+ b.minLat+":"+b.maxLat+" < "+ terrainDetails.bounds.maxLat+" && "+ terrainDetails.bounds.minLon +" < "+ b.minLon+":"+b.maxLon+" < "+ terrainDetails.bounds.maxLon);
//		Debug.Log(b.minLat +" < "+ terrainDetails.bounds.minLat+" && "+terrainDetails.bounds.maxLat+" < "+ b.maxLat);
//		Debug.Log(b.minLon +" < "+ terrainDetails.bounds.minLon+" && "+terrainDetails.bounds.maxLon+" < "+ b.maxLon);
		if (
			((terrainDetails.bounds.minLat < b.minLat && b.minLat < terrainDetails.bounds.maxLat) && (terrainDetails.bounds.minLon < b.minLon && b.minLon < terrainDetails.bounds.maxLon)) ||
			((terrainDetails.bounds.minLat < b.minLat && b.minLat < terrainDetails.bounds.maxLat) && (terrainDetails.bounds.minLon < b.maxLon && b.maxLon < terrainDetails.bounds.maxLon)) ||
			((terrainDetails.bounds.minLat < b.maxLat && b.maxLat < terrainDetails.bounds.maxLat) && (terrainDetails.bounds.minLon < b.minLon && b.minLon < terrainDetails.bounds.maxLon)) ||
			((terrainDetails.bounds.minLat < b.maxLat && b.maxLat < terrainDetails.bounds.maxLat) && (terrainDetails.bounds.minLon < b.maxLon && b.maxLon < terrainDetails.bounds.maxLon)) ||
			(b.minLat < terrainDetails.bounds.minLat && terrainDetails.bounds.maxLat < b.maxLat) ||
			(b.minLon < terrainDetails.bounds.minLon && terrainDetails.bounds.maxLon < b.maxLon)
			) {
			return true;	
		}
		else
			return false;
	}
	
	//-----------------------------------------------------------
	// TODO put in own file
	public static GameObject loadingCubeRef = null;
	
	public static List<GameObject> loadingCubeStore;
	
	public static GameObject getLoadingCube() {
		if (loadingCubeStore.Count == 0) {
			return MonoBehaviour.Instantiate(loadingCubeRef) as GameObject;
		}
		else {
			GameObject ret = loadingCubeStore[0];
			loadingCubeStore.RemoveAt(0);
			return ret;
		}
	}
	public static void storeLoadingCube(GameObject lc) {
		loadingCubeStore.Add(lc);
	}
	public static void bufferLoadingCubes(int num) {
		while (loadingCubeStore.Count <= num) {
			loadingCubeStore.Add(MonoBehaviour.Instantiate(loadingCubeRef) as GameObject);
		}
	}
	//-----------------------------------------------------------
	
	// these are temporary for creating 'randomly placed' nodes
	public static int firstNodeId = -1;
	public static int lastNodeId = -1; 
	public static int someNodeId = -1;
	public static int noNodeId = -1;
	
	
	public static Vector3 normalizePosition(int nodeId) {
		return normalizePosition(getNode (nodeId));
	}
	public static Vector3 normalizePosition(Node n) {
		return normalizePosition(n.Longitude, n.Latitude);
	}
	
	
	public static Vector3 llToUnits(double lon, double lat) {
		return normalizePosition(lon,lat);
	}
	public static Vector3 normalizePosition(double lon, double lat) {
		//Debug.Log("ll:"+lat +"-"+ heightBounds.minLat+":"+lon +"-"+ heightBounds.minLon);
		
		float gameToMapRatio = 2000f/512f;
		//TODO later when I have multiple terrains I'll have to find which one this location is above, for now I only have one
		float gameX = (float)((lon - terrainDetails.bounds.minLon)/LLtoUnitsRatio);
		float gameY = (float)((lat - terrainDetails.bounds.minLat)/LLtoUnitsRatio);
		
		//Debug.Log("gm "+gameX*gameToMapRatio+":"+gameX*gameToMapRatio);
		return new Vector3(gameX*gameToMapRatio, terrainDetails.terrain.terrainData.GetInterpolatedHeight(gameX/512f, gameY/512f)+0.2f, gameY*gameToMapRatio);
	}
	
	public static Vector3 unitsToLl(Vector3 units) {
		//Debug.Log("ll:"+lat +"-"+ heightBounds.minLat+":"+lon +"-"+ heightBounds.minLon);
		
		float gameToMapRatio = 2000f/512f;
		
		double worldx = units.x/gameToMapRatio*LLtoUnitsRatio;
		double worldz = units.z/gameToMapRatio*LLtoUnitsRatio;
		
		return new Vector3((float)(worldx+terrainDetails.bounds.minLon),units.y*GeoTiffReader.heightRatio/1024f,(float)(worldz+terrainDetails.bounds.minLat));
	}
	
	
	public static float distanceBetweenNodes(Node fromNode, Node toNode) {
		return Vector3.Distance(normalizePosition(fromNode), normalizePosition(toNode));
	}
	public static float distanceBetweenNodes(int fromNodeId, int toNodeId) {
		return Vector3.Distance(normalizePosition(getNode (fromNodeId)), normalizePosition(getNode (toNodeId)));
	}
	
	public static Node getNode(int nodeId) {
		return nodeDict[nodeId] as Node;
	}
}

