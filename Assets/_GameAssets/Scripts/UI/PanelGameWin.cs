using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelGameWin : PanelBase
{
    private void OnEnable()
    {
        //SoundManager.I.PlaySFX(TypeSound.SFX_Win);
    }

    public void ButtonNextLevelOnClick()
    {

        TransitionGUI.I.ShowTransition(
() =>
{
    //NextLevelIndex();
    //EventManager.PublishLevelStateChangeEvent(LevelState.LevelInitialize);
    SoundManager.I.PlaySFX(TypeSound.SFX_Click);
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    DeActiveMe(null);
});
    }
}
