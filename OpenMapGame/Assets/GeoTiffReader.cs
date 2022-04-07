using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO;

public class GeoTiffReader {
	
	public static float heightRatio = 2048f*8f; 
	
	public static int intermediateStrips = 5;
	
	string fileName;
	
	struct Header {
	
		public char readOrder0;
		public char readOrder1;
		public ushort fileTypeId;
		public uint ifdLocation;
		
		public Header(byte [] headerInfo) {
			
			readOrder0 = System.BitConverter.ToChar (headerInfo, 0);
			readOrder1 = System.BitConverter.ToChar (headerInfo, 1);
			fileTypeId = System.BitConverter.ToUInt16(headerInfo, 2);
			ifdLocation = System.BitConverter.ToUInt32(headerInfo, 4);
			
		}
		
		public override string ToString() {
			return ""+readOrder0+readOrder1+", type "+fileTypeId+", loc "+ifdLocation;
		}
		
	};
	Header header;
	
	
	public enum DataType
	{
		None        = 0,
		Byte        = 1,
		ASCII       = 2,
		Short       = 3,
		Long        = 4,
		Rational    = 5, // 2 longs: num/den
		SignedByte  = 6,
		Undefined   = 7,
		SignedShort = 8,
		SignedLong  = 9,
		SignedRational = 10, // 2 signed longs: num/den
		Float       = 11,
		Double      = 12
	};
	
	public enum IdType
	{
		ImageWidth          = 256,
		ImageHeight         = 257,
		BitsPerSample       = 258,
		Compression         = 259,
		PhotometricInterpretation = 262,
		StripOffsets        = 273, 
		SamplesPerPixel     = 277,
		RowsPerStrip        = 278,
		StripByteCounts     = 279,
		PlanarConfiguration = 284,
		SampleFormat        = 339
	};
	
	struct IfdInfo {
		
		public IdType id;
		public DataType type;
		public uint count;
		public uint locOrVal;
		public bool val;
		
		
		public void readFromBuffer(byte [] headerInfo, int index) {
			
			
			id =       (IdType)System.BitConverter.ToUInt16(headerInfo, index+0);
			type =     (DataType)System.BitConverter.ToUInt16(headerInfo, index+2);
			count =    System.BitConverter.ToUInt32(headerInfo, index+4);
			locOrVal = System.BitConverter.ToUInt32(headerInfo, index+8);
			
			uint bytesPerType = 0;
			if (type == DataType.Byte || 
				type == DataType.ASCII || 
				type == DataType.Undefined || 
				type == DataType.SignedByte ) {
				bytesPerType = 1;
			}
			else if (	type == DataType.Short || 
						type == DataType.SignedShort) {
				bytesPerType = 2;
			}
			else if (	type == DataType.Long ||
						type == DataType.SignedLong ||
						type == DataType.Float) {
				bytesPerType = 4;
			}
			else if (	type == DataType.Rational ||
						type == DataType.SignedRational ||
						type == DataType.Double) {
				bytesPerType = 8;
			}
			
			uint spaceNeeded = count*bytesPerType;
			val = (spaceNeeded <= 4); // if it required less than 4 bytes, then the value is stored, otherwise the location is stored
		}
		
		public override string ToString() {
			return ""+id+"("+(int)id+"), "+type+" ("+(int)type+"), x"+count+(val?"="+locOrVal:"@"+locOrVal);
		}
		
	};
	
	IfdInfo [] ifdArray; 
	
	Dictionary<IdType, IfdInfo> idToInfo;
	
	long [] stripOffsets;
	
	LLBounds bounds;
	
	public GeoTiffReader (string _fileName, string headerFileName) {
		
		fileName = _fileName;
		
		BinaryReader reader = new BinaryReader(File.OpenRead(fileName));
		
		byte [] headerInfo = reader.ReadBytes(8);
		
		header = new Header(headerInfo);
		
		MonoBehaviour.print (header);
		
		reader.BaseStream.Seek(header.ifdLocation, SeekOrigin.Begin);
		
		int numberOfIfds = reader.ReadInt16();
		
		MonoBehaviour.print (numberOfIfds);
		
		byte [] ifdByteArray = reader.ReadBytes(12*numberOfIfds);
		
		ifdArray = new IfdInfo[numberOfIfds];
		idToInfo = new Dictionary<IdType, IfdInfo>();
		
		for (int i = 0 ; i < numberOfIfds ; i++) {
			ifdArray[i].readFromBuffer(ifdByteArray, i*12);
			idToInfo[ifdArray[i].id] = ifdArray[i];
			MonoBehaviour.print(ifdArray[i]);
			
		}
		
		reader.BaseStream.Seek(idToInfo[IdType.StripOffsets].locOrVal, SeekOrigin.Begin);
		
		byte [] stripOffsetBytes = reader.ReadBytes((int)idToInfo[IdType.StripOffsets].count*4); // 4 for long size
		
		stripOffsets = new long[idToInfo[IdType.StripOffsets].count];
		
		for (int i = 0 ; i < idToInfo[IdType.StripOffsets].count ; i++) {
			stripOffsets[idToInfo[IdType.StripOffsets].count-i-1] = System.BitConverter.ToUInt32(stripOffsetBytes, i*4);
		}
		
		reader.Close();
		
		StreamReader headerReader = new StreamReader(File.OpenRead(headerFileName));
		
		string geoInfo = headerReader.ReadToEnd();
		
		double top = 0, bottom = 0, left = 0, right = 0;
		
		foreach(string s in geoInfo.Split(("\n").ToCharArray())) {
			//MonoBehaviour.print (s);
			if (s.StartsWith("Upper Left")) {
				Debug.Log(s);
				string [] ssplit = s.Split("(,)".ToCharArray());
				left = double.Parse(ssplit[1]);
				top  = double.Parse(ssplit[2]);
			}
			if (s.StartsWith("Lower Right")) {
				Debug.Log(s);
				string [] ssplit = s.Split("(,)".ToCharArray());
				right  = double.Parse(ssplit[1]);
				bottom = double.Parse(ssplit[2]);
			}
		}
		bounds = new LLBounds(left, bottom, right, top);
		
		MonoBehaviour.print (top +":"+bottom);
		MonoBehaviour.print (left+":"+right);
		
	}
	
	
	float [,] retData;
	float [,] retStrip;
	BinaryReader reader;
	
	int percentageAlongMinLatIndex;
	int percentageAlongMaxLatIndex;
	int percentageAlongMinLonIndex;
	int percentageAlongMaxLonIndex;
	
	public void initHeightDataExtract (LLBounds section, out LLBounds sectionOut) {
		
		
		Debug.Log(bounds.ToString());
		Debug.Log(section.ToString());
		
		double percentageLat = (section.maxLat-section.minLat)/(bounds.maxLat-bounds.minLat);
		double percentageLon = (section.maxLon-section.minLon)/(bounds.maxLon-bounds.minLon);
		
		//MonoBehaviour.print (percentageLat+":"+percentageLon);
		
		uint imageWidth  = idToInfo[IdType.ImageWidth].locOrVal;
		uint imageHeight = idToInfo[IdType.ImageHeight].locOrVal;
		//MonoBehaviour.print ((bounds.maxLat-bounds.minLat)/((double)imageWidth));
		
		double percentageAlongMinLat = (section.minLat-bounds.minLat)/(bounds.maxLat-bounds.minLat);
		double percentageAlongMinLon = (section.minLon-bounds.minLon)/(bounds.maxLon-bounds.minLon);
		double percentageAlongMaxLat = (section.maxLat-bounds.minLat)/(bounds.maxLat-bounds.minLat);
		double percentageAlongMaxLon = (section.maxLon-bounds.minLon)/(bounds.maxLon-bounds.minLon);
		
		percentageAlongMinLatIndex = (int)(percentageAlongMinLat*imageHeight);
		percentageAlongMinLonIndex = (int)(percentageAlongMinLon*imageWidth);
		percentageAlongMaxLatIndex = (int)(percentageAlongMaxLat*imageHeight)+1;
		percentageAlongMaxLonIndex = (int)(percentageAlongMaxLon*imageWidth)+1;
		
		//MonoBehaviour.print (percentageAlongMinLat*imageHeight+":"+percentageAlongMaxLat*imageHeight);
		
		
		Debug.Log ("%Lat: "+percentageAlongMinLat+":"+percentageAlongMaxLat);
		Debug.Log ("%Lon: "+percentageAlongMinLon+":"+percentageAlongMaxLon);
		Debug.Log ("%indexLat: "+percentageAlongMinLatIndex+":"+percentageAlongMaxLatIndex);
		Debug.Log ("%indexLon: "+percentageAlongMinLonIndex+":"+percentageAlongMaxLonIndex);
		
		sectionOut = new LLBounds(	(double)percentageAlongMinLonIndex/imageWidth  *(bounds.maxLon-bounds.minLon)+bounds.minLon,
									(double)percentageAlongMinLatIndex/imageHeight *(bounds.maxLat-bounds.minLat)+bounds.minLat,
									(double)percentageAlongMaxLonIndex/imageWidth  *(bounds.maxLon-bounds.minLon)+bounds.minLon,
									(double)percentageAlongMaxLatIndex/imageHeight *(bounds.maxLat-bounds.minLat)+bounds.minLat
									);
		
		retData = new float[(percentageAlongMaxLatIndex-percentageAlongMinLatIndex), (percentageAlongMaxLonIndex-percentageAlongMinLonIndex)];
		retStrip = new float[intermediateStrips,(percentageAlongMaxLonIndex-percentageAlongMinLonIndex)];
		
		Debug.Log (sectionOut.ToString());
		
		Debug.Log("retData size: "+retData.GetLength(0)+"x"+retData.GetLength(1));
		
		
		// initialise the reader
		reader = new BinaryReader(File.OpenRead(fileName));
	}
	
	float min = 0;
	float max = 10000;
	
	public bool getStrip(int i) {
		if (i < percentageAlongMaxLatIndex - percentageAlongMinLatIndex) {
		
			//MonoBehaviour.print (stripOffsets);
//			MonoBehaviour.print (i+"/"+(percentageAlongMaxLatIndex-percentageAlongMinLatIndex)+":"+(percentageAlongMinLatIndex+i)+":"+stripOffsets[percentageAlongMinLatIndex+i]+"+"+percentageAlongMinLonIndex+"*"+2);
			reader.BaseStream.Seek(stripOffsets[percentageAlongMinLatIndex+i]+(percentageAlongMinLonIndex*2), SeekOrigin.Begin);
			
			byte [] strip = reader.ReadBytes((percentageAlongMaxLonIndex - percentageAlongMinLonIndex)*2);
			for (int l = 0 ; l < (percentageAlongMaxLonIndex - percentageAlongMinLonIndex) ; l++) {
				retData[i,l] = System.BitConverter.ToInt16(strip, l*2)/(heightRatio);
				retStrip[(i%intermediateStrips),l] = retData[i,l];
				if (retData[i,l] < min) {
					min = retData[i,l];
					Debug.Log("min "+min);
				}
				if (retData[i,l] > max) {
					max = retData[i,l];
					Debug.Log("max "+max);
				}
			}
			
			return true; // valid strip
		}
		else 
			return false; // invalid strip
	}
	
	public int getNumberOfStrips() {
		return percentageAlongMaxLatIndex - percentageAlongMinLatIndex;
	}
	
	public void finishHeightDataExtract () {
		// close the reader
		reader.Close();
	}
	
	public float [,] getIntermediateHeightData() {
		return retData;
	}
	public float [,] getIntermediateHeightDataStrip() {
		return retStrip;
	}
		
		
	public float [,] getHeightData(LLBounds section, out LLBounds sectionOut) {
	
		initHeightDataExtract(section, out sectionOut);
		
		int i = 0;
		while (getStrip(i)) {
			i++;
		}
//		for (int i = 0 ; i < (percentageAlongMaxLatIndex - percentageAlongMinLatIndex) ; i++) {
//			getStrip(i);
//		}
		
//		MonoBehaviour.print("mm:"+min+":"+max);
		MonoBehaviour.print("index:"+(percentageAlongMaxLatIndex-percentageAlongMinLatIndex)+":"+(percentageAlongMaxLonIndex-percentageAlongMinLonIndex));
		
		return retData;
		
	}
	
	

}










