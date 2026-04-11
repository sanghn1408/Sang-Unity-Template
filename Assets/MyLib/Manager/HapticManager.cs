#if MOREMOUNTAINS_NICEVIBRATIONS
using MoreMountains.NiceVibrations;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticManager : Singleton<HapticManager>
{
#if MOREMOUNTAINS_NICEVIBRATIONS
    public void PlayHaptic(HapticTypes type)
    {
        if (DataManager.I.SaveData.IsHapic)
        {
            MMVibrationManager.Haptic(type);
            Debug.Log("rung rung");
        }
    }
#endif
}
