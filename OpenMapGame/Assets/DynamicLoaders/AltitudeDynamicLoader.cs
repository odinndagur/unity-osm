using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AltitudeDynamicLoader : IteratedDynamicLoader {
	
	TerrainDetails terrainDetails;
	GUIText guiTextTerrainLoadInfo = null;
	public AltitudeDynamicLoader (TerrainDetails t, GUIText guiText, float fraction) : base (fraction) {
		terrainDetails = t;
		guiTextTerrainLoadInfo = guiText;
	}
	
	GeoTiffReader tiffRead;
	int index;
	
	public override void initLoad() {
		
		
		tiffRead = new GeoTiffReader("CustomAssets/srtm_40_19.tif", "CustomAssets/srtm_40_19.hdr");
		Debug.Log("AltitudeDynamicLoader bounds: "+terrainDetails.bounds);
		
		tiffRead.initHeightDataExtract(terrainDetails.targetBounds, out terrainDetails.bounds);
		Debug.Log("hb:"+terrainDetails.bounds);
		index = 0;
		
	}
	
	public override void loadIteration(){
		
		
		if (tiffRead.getStrip(index))
			index++;
		else
			finishLoading();
		
		if ((index)%(GeoTiffReader.intermediateStrips) == 0)
			terrainDetails.terrain.terrainData.SetHeights(0, index-GeoTiffReader.intermediateStrips, tiffRead.getIntermediateHeightDataStrip());
		
		
		guiTextTerrainLoadInfo.text = "Terrain Loaded: "+index+"/"+tiffRead.getNumberOfStrips()+" ("+((float)index/tiffRead.getNumberOfStrips()*100).ToString("0.00")+"%)";
//		
//		// load in height data
//		
//		
//		Debug.Log("loading heightmaps");
//		
//		GeoTiffReader tiffRead = new GeoTiffReader("Assets/srtm_40_19.tif", "Assets/srtm_40_19.hdr");
//		
//		Debug.Log("heightmaps loaded");
//		
//		float[,] heightData = tiffRead.getHeightData(MapData.bounds, out MapData.heightBounds);
//		
//		MapData.terrain.terrainData.SetHeights(0,0, heightData);
//		//terrain.transform.position+=new Vector3((float)(MapData.bounds.minLon-outBounds.minLon), 0f, (float)(MapData.bounds.minLat-outBounds.minLat));
//		
//		Debug.Log("loaded heightmaps");
////		
//		return true;
	}
	
	public override void finialiseLoad(){
		//terrainDetails.terrain.terrainData.SetHeights(0,0, tiffRead.getIntermediateHeightData());
		tiffRead.finishHeightDataExtract();
	}
	
	
}
