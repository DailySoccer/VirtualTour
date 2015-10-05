using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class UMALODConversionEntry
{
	public string SourcePieceName;
	public string DestinationPieceName;
	public int LODLevel;
	public int groupInt;
	public ConversionGroup group
	{
		get { return (ConversionGroup)groupInt; }
		set { groupInt = (int)value; }
	}
	public enum ConversionGroup
	{
		SlotData,
		OverlayData,
		RaceData
	}
}
