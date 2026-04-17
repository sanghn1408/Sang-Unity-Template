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
        UITransitionScreen.I.ShowTransition(
() =>
{
    GamePlayManager.I.GoToGamePlay();

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
    GamePlayController.I.NextLevel();
    //EventManager.PublishLevelStateChangeEvent(LevelState.LevelInitialize);
    SoundManager.I.PlaySFX(TypeSound.SFX_Click);
    GamePlayManager.I.GoToGamePlay();
    DeActiveMe(null);
});
    }
}
