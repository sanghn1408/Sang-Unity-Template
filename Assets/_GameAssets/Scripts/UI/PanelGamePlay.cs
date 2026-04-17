using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelGamePlay : PanelBase
{
    public void ButtonRestartOnClick()
    {
        //EventManager.PublishLevelStateChangeEvent(LevelState.LevelInitialize);
        SoundManager.I.PlaySFX(TypeSound.SFX_Click);
        TransitionGUI.I.ShowTransition(
() =>
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    // stepFinish → chạy sau khi fade-in xong
});

    }

    public void ButtonSettingsOnClick()
    {
        UIManager.I.Show<PanelSetting>();
        SoundManager.I.PlaySFX(TypeSound.SFX_Click);
    }

    public void ButtonNextLevelOnClick()
    {

        TransitionGUI.I.ShowTransition(
() =>
{
    //DataManagerTileMatch.NextLevelIndex();
    //EventManager.PublishLevelStateChangeEvent(LevelState.LevelInitialize);
    SoundManager.I.PlaySFX(TypeSound.SFX_Click);
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    DeActiveMe(null);
});
    }
}
