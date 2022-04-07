using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.IO;



public class SceneOverlordScript : MonoBehaviour {
	
	public static bool showLoadingCubes = false; // this should be in some form of static  global options class
	
	
	public LinkedDynamicDataLoader mapLoader;
	public Terrain terrain;
	
	public GameObject refToRoad = null;
	public GameObject refToUnit = null;
	public GameObject refToRoadMarker = null;
	public GameObject refToLineRenderRoad = null;
	public GameObject refToLoadingStateCube = null;
	
	public GUIText guiTextLineRenderer = null;
	public GUIText guiTextBlockInfo = null;
	public GUIText guiTextTerrainLoadInfo = null;
	
	public Material refToRoadMaterial = null;
	public Material refToPathMaterial = null;
	
	public GameObject refToMouseCursorCube = null;
	
	
	LLBounds wtfBounds;
	
//	Dictionary<float, TweenPosSize []> granualityToCursor;
	Dictionary<float, MapData.RoadType> granualityToRoadType;
	float [] granualities;
	
	// Use this for initialization
	void Start () {
	
		granualities = new float [6];
		granualities [0] = 1.28f;
		granualities [1] = 0.64f;
		granualities [2] = 0.32f;
		granualities [3] = 0.16f;
		granualities [4] = 0.08f;
		granualities [5] = 0.04f;
		
		
//		granualities = new float [1];
//		granualities [0] = 1.28f;
		
//		granualityToCursor = new Dictionary<float, TweenPosSize[]>();
//		granualityToCursor[1.28f] = new TweenPosSize[4];
//		granualityToCursor[0.64f] = new TweenPosSize[4];
//		granualityToCursor[0.32f] = new TweenPosSize[4];
//		granualityToCursor[0.16f] = new TweenPosSize[4];
//		granualityToCursor[0.08f] = new TweenPosSize[4];
//		granualityToCursor[0.04f] = new TweenPosSize[4];
//		foreach (TweenPosSize [] tweens in granualityToCursor.Values) {
//			for (int i = 0 ; i < 4 ; i++) {
//				tweens[i] = (Instantiate(refToMouseCursorCube) as GameObject).GetComponent<TweenPosSize>();
//			}
//		}
		granualityToRoadType = new Dictionary<float, MapData.RoadType>();
		granualityToRoadType[1.28f] = MapData.RoadType.Trunk;
		granualityToRoadType[0.64f] = MapData.RoadType.Primary;
		granualityToRoadType[0.32f] = MapData.RoadType.Secondary;
		granualityToRoadType[0.16f] = MapData.RoadType.Tertiary;
		granualityToRoadType[0.08f] = MapData.RoadType.LivingStreet;
		granualityToRoadType[0.04f] = MapData.RoadType.FootPath;
		
		MapData.guiTextBlockInfo = guiTextBlockInfo;
		
		MapData.loadingCubeRef = refToLoadingStateCube;
		
		MapData.loadingCubeStore = new List<GameObject>(100);
		MapData.bufferLoadingCubes(100);
			
		
		MapData.nodeDict = new Hashtable();
		MapData.wayDict = new Hashtable();
		MapData.wayToProcessDict = new Hashtable();
		MapData.relationDict = new Hashtable();
		MapData.wayIdToLineRenderer = new Dictionary<int, LineRenderer>();
		
		MapData.lineRendererVertexCount = new Dictionary<LineRenderer, int>();
		
		MapData.terrainDetails = new TerrainDetails(new LLBounds(18.3,-34.1,18.7,-33.7), terrain);
		float [,] h = new float[513,513];
		MapData.terrainDetails.terrain.terrainData.SetHeights(0,0, h);
		
		
		//wtfBounds = new LLBounds(18.3634,-33.9632,18.4675,-33.8956);
//		MapData.bounds = new LLBounds(18.3634,-33.9632,18.5675,-33.7956);
		//MapData.bounds = new LLBounds(18.3634,-33.9632,18.4675,-33.9156);
		//wtfBounds      = new LLBounds(18.3634,-33.9632,18.4675,-33.9156);
		//MapData.bounds = new LLBounds(18.3634,-33.9632,18.4675,-33.9456);
		//MapData.bounds = new LLBounds(18.3634,-33.9132,18.4075,-33.8956);
		//MapData.bounds = new LLBounds(18.37335,-33.93554,18.39938,-33.91941);
		//MapData.bounds = new LLBounds(18.38,-33.93,18.42,-33.90);
//		wtfBounds =      new LLBounds(18.37,-33.94,18.39,-33.93);
//		MapData.bounds = wtfBounds;
		
//		mapLoader.addDynamicLoader(new OSMXAPIDynamicLoader(), "OSMXAPI");
//		mapLoader.addDynamicLoader(new OSMDynamicLoader(0.1f), "OSM");
//		
//		mapLoader.addDependency("OSM", "OSMXAPI");
		
//		mapLoader.addDynamicLoader(new OSMDynamicLoader("CustomAssets/OSMchunks/map18.36,-33.94,18.38,-33.92.osm", 0.1f), "OSM1");
//		mapLoader.addDynamicLoader(new OSMDynamicLoader("CustomAssets/OSMchunks/map18.38,-33.94,18.4,-33.92.osm", 0.1f), "OSM2");
//		mapLoader.addDynamicLoader(new OSMDynamicLoader("CustomAssets/OSMchunks/map18.4,-33.94,18.42,-33.92.osm", 0.1f), "OSM3");
//		mapLoader.addDynamicLoader(new OSMDynamicLoader("CustomAssets/OSMchunks/map18.42,-33.94,18.44,-33.92.osm", 0.1f), "OSM4");
//		mapLoader.addDynamicLoader(new OSMDynamicLoader("CustomAssets/OSMchunks/map18.44,-33.94,18.46,-33.92.osm", 0.1f), "OSM5");
//		mapLoader.addDynamicLoader(new OSMDynamicLoader("CustomAssets/OSMchunks/map18.46,-33.94,18.48,-33.92.osm", 0.1f), "OSM6");
		
		mapLoader.addDynamicLoader(new AltitudeDynamicLoader(MapData.terrainDetails, guiTextTerrainLoadInfo, 0.02f), "Altitude");
		
		mapLoader.addDynamicLoader(new LineRendererDynamicLoader(refToLineRenderRoad, refToPathMaterial, guiTextLineRenderer, 0.01f), "LineRender");
//		mapLoader.addDependency("LineRender", "OSM1");
//		mapLoader.addDependency("LineRender", "OSM2");
//		mapLoader.addDependency("LineRender", "OSM3");
//		mapLoader.addDependency("LineRender", "OSM4");
//		mapLoader.addDependency("LineRender", "OSM5");
//		mapLoader.addDependency("LineRender", "OSM6");
		mapLoader.addDependency("LineRender", "Altitude");
		
		//mapLoader.addDynamicLoader(new NodeUnitDynamicLoader(fractionOfFrameForLoading, refToUnit), "Units");
		//mapLoader.addDependency("Units", "LineRender");
		
		
		
		
		
		
	}
	
	public bool loadOsmRegion(OSMBounds bounds) {
		
		
//		Debug.Log(bounds.boundString());
		string fileloaderName = "OSMFileLoad"+bounds.boundString();
		string loaderName = "OSM"+bounds.boundString();
		if (!mapLoader.contains(loaderName)) {
			Progressable prog = MapData.getLoadingCube().GetComponent<Progressable>();
			
			OSMFileLoader osmFileLoader;
			if (OSMFileLoader.fileIsLocal(bounds)) {
				osmFileLoader = new OSMFileDynamicLoader(
						bounds,
						prog,
						0.01f);
			}
			else {
				osmFileLoader = new OSMXAPIDynamicLoader(
						bounds,
						prog,
						0.01f);
			}
			mapLoader.addDynamicLoader(osmFileLoader, fileloaderName);
			mapLoader.addDynamicLoader(
				new OSMDynamicLoader(
					osmFileLoader,
					prog,
					0.01f), 
				loaderName);
			mapLoader.addDependency(loaderName, fileloaderName);
			mapLoader.addDependency(loaderName, "Altitude");
			mapLoader.addRestartHook(loaderName, "LineRender");
			mapLoader.tryLoad(loaderName);
			return true;
		}
		else
			return false;
	
	}
	
	bool firstTime = true;
	// Update is called once per frame
	void Update () {
		
		if (firstTime) {
			firstTime = false;
			
			mapLoader.tryLoad("Altitude");
			
			Debug.Log("FirstTime");
		}
		
		if (Input.GetKeyDown(KeyCode.C)){
			showLoadingCubes = !showLoadingCubes;
			MapData.setBlockLoadText();
		}
		
		if(mapLoader.isLoaded("Altitude")) {
		
			//Debug.Log("DRAWLINE START");
			//Debug.Log(MapData.bounds.minLon+":"+MapData.bounds.minLat);
			MapData.normalizePosition(MapData.terrainDetails.bounds.minLon, MapData.terrainDetails.bounds.minLat);
			
			//Debug.Log("DA FUC");
			Debug.DrawLine (MapData.normalizePosition(MapData.terrainDetails.bounds.minLon, MapData.terrainDetails.bounds.minLat), MapData.normalizePosition(MapData.terrainDetails.bounds.maxLon, MapData.terrainDetails.bounds.minLat), Color.red);
			Debug.DrawLine (MapData.normalizePosition(MapData.terrainDetails.bounds.maxLon, MapData.terrainDetails.bounds.minLat), MapData.normalizePosition(MapData.terrainDetails.bounds.maxLon, MapData.terrainDetails.bounds.maxLat), Color.red);
			Debug.DrawLine (MapData.normalizePosition(MapData.terrainDetails.bounds.maxLon, MapData.terrainDetails.bounds.maxLat), MapData.normalizePosition(MapData.terrainDetails.bounds.minLon, MapData.terrainDetails.bounds.maxLat), Color.red);
			Debug.DrawLine (MapData.normalizePosition(MapData.terrainDetails.bounds.minLon, MapData.terrainDetails.bounds.maxLat), MapData.normalizePosition(MapData.terrainDetails.bounds.minLon, MapData.terrainDetails.bounds.minLat), Color.red);
			
		}
		
		Vector3 mouseWorldPosition;
		
		Vector3 mouseScreenPosition;
		Ray mouseRay;
		
		mouseScreenPosition = Input.mousePosition;
//		Ray oldMouseRay = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
		Ray oldMouseRay = Camera.mainCamera.ScreenPointToRay(new Vector3(Camera.mainCamera.pixelRect.width/2, Camera.mainCamera.pixelRect.height/2, 0f));
//		Debug.Log();
		mouseRay = new Ray(Camera.mainCamera.transform.position, Vector3.down+oldMouseRay.direction.normalized);
		Debug.DrawRay(Camera.mainCamera.transform.position, (Vector3.down+oldMouseRay.direction.normalized)*100);
		RaycastHit rayHit = new RaycastHit();
		bool hit = Physics.Raycast(mouseRay, out rayHit, 10000f, 1 << 9); // 9 is the ground layer
		mouseWorldPosition = rayHit.point;
		
		Debug.DrawLine(mouseWorldPosition-Vector3.left*10   , mouseWorldPosition+Vector3.left*10   , Color.red);
		Debug.DrawLine(mouseWorldPosition-Vector3.up*10     , mouseWorldPosition+Vector3.up*10     , Color.red);
		Debug.DrawLine(mouseWorldPosition-Vector3.forward*10, mouseWorldPosition+Vector3.forward*10, Color.red);
		Vector3 globalPosition = MapData.unitsToLl(mouseWorldPosition);
		
//		double granuality = 0.64;
		foreach (float granuality in granualities){
			for (int i = -1 ; i < 2 ; i+=2) {
				for (int j = -1 ; j < 2 ; j+=2) {
					float offsetX = granuality/2*i;
					float offsetY = granuality/2*j;
					float multiplier = granuality/0.01f;
					float origGlobX = (float)System.Math.Round((globalPosition.x+offsetX-(granuality/2))/multiplier, 2)*multiplier;
					float origGlobY = (float)System.Math.Round((globalPosition.z+offsetY-(granuality/2))/multiplier, 2)*multiplier;
					
					Debug.DrawRay(MapData.llToUnits(globalPosition.x+offsetX, globalPosition.z+offsetY), Vector3.up*100);
					
					float globX = origGlobX;
					float globY = origGlobY;
					OSMBounds selectedRegion = new OSMBounds( globX, globY, globX+granuality,globY+granuality, granualityToRoadType[granuality]);
					
	//				if (hit) {
	//					Vector3 minCorner = MapData.llToUnits(selectedRegion.minLon, selectedRegion.minLat);
	//					Vector3 maxCorner = MapData.llToUnits(selectedRegion.maxLon, selectedRegion.maxLat);
	//					granualityToCursor[granuality][i].setTargetScale(new Vector3((maxCorner.x - minCorner.x), 50, (maxCorner.z - minCorner.z)));
	//					granualityToCursor[granuality][i].setTargetPosition(new Vector3(minCorner.x+(maxCorner.x - minCorner.x)/2, 25, minCorner.z+(maxCorner.z - minCorner.z)/2));
	//				}
					
			//			Debug.Log (selectedRegion);
					
					Debug.DrawRay(MapData.llToUnits(selectedRegion.minLon, selectedRegion.minLat), Vector3.up*100, Color.red);
					Debug.DrawRay(MapData.llToUnits(selectedRegion.minLon, selectedRegion.maxLat), Vector3.up*100, Color.red);
					Debug.DrawRay(MapData.llToUnits(selectedRegion.maxLon, selectedRegion.maxLat), Vector3.up*100, Color.red);
					Debug.DrawRay(MapData.llToUnits(selectedRegion.maxLon, selectedRegion.minLat), Vector3.up*100, Color.red);
					if (hit/* && Input.GetMouseButton(0)*/) {
						if (MapData.insideTerrain(selectedRegion))
							loadOsmRegion(selectedRegion);
					}
				}
			}
		}
		
//			Debug.DrawLine (MapData.normalizePosition(MapData.heightBounds.minLon, MapData.heightBounds.minLat), MapData.normalizePosition(MapData.heightBounds.maxLon, MapData.heightBounds.minLat), Color.red);
//			Debug.DrawLine (MapData.normalizePosition(MapData.heightBounds.maxLon, MapData.heightBounds.minLat), MapData.normalizePosition(MapData.heightBounds.maxLon, MapData.heightBounds.maxLat), Color.red);
//			Debug.DrawLine (MapData.normalizePosition(MapData.heightBounds.maxLon, MapData.heightBounds.maxLat), MapData.normalizePosition(MapData.heightBounds.minLon, MapData.heightBounds.maxLat), Color.red);
//			Debug.DrawLine (MapData.normalizePosition(MapData.heightBounds.minLon, MapData.heightBounds.maxLat), MapData.normalizePosition(MapData.heightBounds.minLon, MapData.heightBounds.minLat), Color.red);
////			
//			Debug.DrawLine (MapData.normalizePosition(wtfBounds.minLon, wtfBounds.minLat), MapData.normalizePosition(wtfBounds.maxLon, wtfBounds.minLat), Color.red);
//			Debug.DrawLine (MapData.normalizePosition(wtfBounds.maxLon, wtfBounds.minLat), MapData.normalizePosition(wtfBounds.maxLon, wtfBounds.maxLat), Color.red);
//			Debug.DrawLine (MapData.normalizePosition(wtfBounds.maxLon, wtfBounds.maxLat), MapData.normalizePosition(wtfBounds.minLon, wtfBounds.maxLat), Color.red);
//			Debug.DrawLine (MapData.normalizePosition(wtfBounds.minLon, wtfBounds.maxLat), MapData.normalizePosition(wtfBounds.minLon, wtfBounds.minLat), Color.red);
		
		//Debug.Log("DRAWLINE END");
		
		
	}
	
	
}


