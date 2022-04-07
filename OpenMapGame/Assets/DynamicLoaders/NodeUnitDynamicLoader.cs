using UnityEngine;
using System.Collections;

public class NodeUnitDynamicLoader : CollectionIteratedDynamicLoader<Way> {
	
	
	public GameObject refToUnit = null;
	
	int countDown = 1000;
	
	public NodeUnitDynamicLoader(float fraction, GameObject _refToUnit) : base (fraction) {
		refToUnit = _refToUnit;
	}
	
	public override void initLoad() {
		countDown = 1000;
		setCollection(MapData.wayDict.Values);
	}
	
	public override void loadIteration(Way w) {
		
		string hw = w.getTag("highway");
		if(hw != null && !hw.Equals("")){
			GameObject un = MonoBehaviour.Instantiate(refToUnit) as GameObject;
			
			UnitMovement unMov = un.GetComponent<UnitMovement>();
			unMov.setUnitLocation(w.NodeID[0]);
			
			countDown--;
			if (countDown == 0) finishLoading();
		}
//		GameObject unit = MonoBehaviour.Instantiate(refToUnit) as GameObject;
//		
//		UnitMovement unitMov = unit.GetComponent<UnitMovement>();
//		
//		Debug.Log (MapData.firstNodeId+":"+MapData.someNodeId+":"+MapData.lastNodeId+":"+MapData.noNodeId);
//		unitMov.setUnitLocation(MapData.someNodeId);

//		unitMov.setUnitLocation(18659198);
//
//		unitMov.queueDestinationNode(291434910);
//		unitMov.queueDestinationNode(291434909);
//		unitMov.queueDestinationNode(25499429);
//		unitMov.queueDestinationNode(291434907);
//		unitMov.queueDestinationNode(25499427);
//		unitMov.queueDestinationNode(291434902);
//		unitMov.queueDestinationNode(25499430);
//		unitMov.queueDestinationNode(25499428);
//		unitMov.queueDestinationNode(25499431);
//		unitMov.queueDestinationNode(291434901);
//		unitMov.queueDestinationNode(25499433);
//		unitMov.queueDestinationNode(764719368);
//		unitMov.queueDestinationNode(25499438);
//  		unitMov.queueDestinationNode(25499439);
//		unitMov.queueDestinationNode(291434867);
//		unitMov.queueDestinationNode(291434863);
//		unitMov.queueDestinationNode(291434860);
		
	}
	
	public override void finialiseLoad() {
		
	}
	

}
