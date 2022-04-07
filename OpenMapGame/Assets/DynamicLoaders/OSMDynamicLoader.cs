using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


public interface OSMFileStore {
	
	string getData();
	
	OSMBounds getBounds();
	
	bool succeeded();
	
};


public abstract class OSMFileLoader : IteratedDynamicLoader, OSMFileStore {
	protected OSMBounds bounds;
	public OSMFileLoader(OSMBounds b, float fraction) : base (fraction) {
		bounds = b;
	}
	
	public static bool fileIsLocal (OSMBounds b) {
		string filename = b.getFileName();
		return System.IO.File.Exists(filename);
	}
	
	public abstract string getData();
	
	public abstract OSMBounds getBounds();
	
	public abstract bool succeeded();
	
	
	public override float getPriority() {
		
		Vector3 minCorner = MapData.llToUnits(getBounds().minLon, getBounds().minLat);
		Vector3 maxCorner = MapData.llToUnits(getBounds().maxLon, getBounds().maxLat);
		return Vector3.Magnitude(Camera.mainCamera.transform.position - (minCorner+maxCorner)/2)*(maxCorner.x-maxCorner.x);
	}
	
}
	
	
public class OSMDynamicLoader : IteratedDynamicLoader {
	
	OSMFileLoader osmFile;
	
	Progressable prog = null;
	
	int nodeTotal;
	int nodeCount;
	
	public OSMDynamicLoader(OSMFileLoader osmf, Progressable p, float fraction) : base (fraction) {
		osmFile = osmf;
		
		LLBounds bounds = osmFile.getBounds();
		
		prog = p;
		
	}
	
	public override float getPriority() {
		
		Vector3 minCorner = MapData.llToUnits(osmFile.getBounds().minLon, osmFile.getBounds().minLat);
		Vector3 maxCorner = MapData.llToUnits(osmFile.getBounds().maxLon, osmFile.getBounds().maxLat);
		return Vector3.Magnitude(Camera.mainCamera.transform.position - (minCorner+maxCorner)/2)*(maxCorner.x-maxCorner.x);
	}
	
	XmlNode node;
	public override void initLoad() {
		
		
		
		Debug.Log("loading xml");
	
		
		XmlDocument doc = new XmlDocument();
		if (osmFile.succeeded()) {
			doc.LoadXml(osmFile.getData());
		}
		else {
			finishLoading();
			return;
		}
//		if (overrideFile.Equals(""))
//			doc.LoadXml(MapData.xapiData); // todo fix and make dependant on a xapi laoder of some form
//		else {
//			if (System.IO.File.Exists(overrideFile))
//				doc.Load(overrideFile);
//			else {
//				finishLoading();
//				return;
//			}
//			
//		}
		
		XmlNode mainNode = doc.ChildNodes.Item(1);
		
		Debug.Log(mainNode.Name+" ("+mainNode.Attributes.Count+", "+mainNode.ChildNodes.Count+")");
		
		nodeTotal = mainNode.ChildNodes.Count;
		nodeCount = 0;
		
		node = mainNode.FirstChild;
		
		prog.setColors(new Color(0f,1f,0f,0.1f), new Color(0f,1f,0f,0.0f));
		prog.setProgress(0f);
			
	}
	
	public override void loadIteration(){
		
		
		//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
		if (node.Name == "bounds") {
			
//			Debug.Log("bounds found ("+MapData.bounds+")");
//			Debug.Log(MapData.xapiData);
//			Debug.Log(node.Attributes.GetNamedItem("minlat").Value);
//			Debug.Log(node.Attributes.GetNamedItem("minlon").Value);
//			MapData.bounds.minLat = double.Parse(node.Attributes.GetNamedItem("minlat").Value);
//			MapData.bounds.minLon = double.Parse(node.Attributes.GetNamedItem("minlon").Value);
//			MapData.bounds.maxLat = double.Parse(node.Attributes.GetNamedItem("maxlat").Value);
//			MapData.bounds.maxLon = double.Parse(node.Attributes.GetNamedItem("maxlon").Value);
//			
//			Debug.Log("bounds found ("+MapData.bounds+")");	
			
		}
		//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
		else if (node.Name == "node") {
			int val = int.Parse(node.Attributes.GetNamedItem("id").Value);
			double lon = double.Parse(node.Attributes.GetNamedItem("lon").Value);
			double lat = double.Parse(node.Attributes.GetNamedItem("lat").Value);
			Node n = new Node(val, lon, lat);
			
			
			MapData.nodeDict[val] = n;
			
			if (MapData.firstNodeId == -1) {
				MapData.firstNodeId = val;
				//print ("firstNodeId: "+firstNodeId);
			}
			
			MapData.lastNodeId = val;
			//print ("lastNodeId: "+lastNodeId);
			
		}
		//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
		else if (node.Name == "way") {
			int val = int.Parse(node.Attributes.GetNamedItem("id").Value);
			Way way = new Way(val);
			
			foreach (XmlNode wayChild in node.ChildNodes) {
				if (wayChild.Name == "nd") {
					
					way.NodeID.Add(int.Parse(wayChild.Attributes.GetNamedItem("ref").Value));
				}
				else if (wayChild.Name == "tag") {
					string k = wayChild.Attributes.GetNamedItem("k").Value;
					string v = wayChild.Attributes.GetNamedItem("v").Value;
					way.addTag(k,v);
					if (k == "highway") {
						//print (k+":"+v);	
						
					}
				}
			}
			
			MapData.wayToProcessDict[val] = way;
		}
		//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
		else if (node.Name == "relation") {
			int val = int.Parse(node.Attributes.GetNamedItem("id").Value);
			Relation relation = new Relation(val);
			
			foreach (XmlNode relChild in node.ChildNodes) {
				
				if (relChild.Name == "member") {
					string type = relChild.Attributes.GetNamedItem("type").Value;
					int id_ref = int.Parse(relChild.Attributes.GetNamedItem("ref").Value);
					string role = relChild.Attributes.GetNamedItem("role").Value;
					relation.addMember(type,id_ref, role);
				}
				else if (relChild.Name == "tag") {
					string k = relChild.Attributes.GetNamedItem("k").Value;
					string v = relChild.Attributes.GetNamedItem("v").Value;
					relation.addTag(k,v);
				}
				else {
					//print(relChild.Name);
				}
			}
			
			MapData.relationDict[val] = relation;
		}
		//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
		else {
		
			Debug.Log(node.Name+":"+node.LocalName+" ("+node.Attributes.Count+", "+node.ChildNodes.Count+")");	
		}
		//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
		node = node.NextSibling;
		nodeCount++;
		prog.setProgress((float)nodeCount/nodeTotal);
		
		if (node == null) {
			finishLoading();
		}
	}
	
	public override void finialiseLoad(){
		
		
		MapData.storeLoadingCube(prog.gameObject);
		
		Debug.Log("node Count: "+MapData.nodeDict.Count);
		Debug.Log("way  Count: "+MapData.wayDict.Count);
		Debug.Log("rel  Count: "+MapData.relationDict.Count);
		
//		foreach (Relation r in MapData.relationDict.Values){
//			print("rel: "+r.name+" ("+r.NodeMembers.Count+","+r.WayMembers.Count+")");
//			foreach(Relation.Member m in r.NodeMembers) {
//				Node n = MapData.nodeDict[m.id_ref] as Node;
//				print("node: "+n.Latitude +":"+n.Longitude);
//			}
//			foreach(Relation.Member m in r.WayMembers) {
//				if (MapData.wayDict.Contains(m.id_ref)){
//					Way w = MapData.wayDict[m.id_ref] as Way;
//					print("name: "+w.name);
//				}
//			}
//			break;
//		}
		markForDeletion();
		osmFile.markForDeletion();
		
		Debug.Log("loaded");
		MapData.loadedBlockCallBack();
	}
	
	
	
	void printXmlNode(XmlNode node, int maxDepth){
		printXmlNode(node, maxDepth, 0);
	}
		
		
	void printXmlNode(XmlNode node, int maxDepth, int currentdepth){
		
		Debug.Log(node.Name+" ("+node.ChildNodes.Count+")");
		
		if (currentdepth != maxDepth && node.HasChildNodes) {
			foreach (XmlNode childNode in node) {
				printXmlNode(childNode, maxDepth, currentdepth+1);
			}
		}
	}
	
	
}
