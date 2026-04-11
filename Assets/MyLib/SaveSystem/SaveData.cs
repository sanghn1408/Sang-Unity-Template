using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains all the variables that will be serialized and saved to a file.<br/>
/// Can be considered as a save file structure or format.
/// </summary>
[Serializable]
public class SaveData
{
	[Header("Main Data")]
	public int Level;
	public long Coin;
	public string NamePlayer;
	public bool IsHapic;
	public bool IsSound;
	public bool IsMusic;
	public bool IsMasterSound;
	public bool IsRemoveAds;


	
    public SaveData()
	{
		Level = 0;
		Coin = 0;
		NamePlayer = "";
		IsHapic = true;
		IsSound = true;
		IsMusic = true;
		IsMasterSound = true;
		IsRemoveAds = false;

		
    }
}



