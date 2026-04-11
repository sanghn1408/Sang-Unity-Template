using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCheat : MonoBehaviour
{
    private void OnCheatWin()
    {
        GamePlayManager.I.GameWin();
    }

    private void OnCheatLose()
    {
        GamePlayManager.I.GameOver();
    }
}
