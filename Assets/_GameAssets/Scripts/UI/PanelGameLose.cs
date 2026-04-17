using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelGameLose : PanelBase
{
    private void OnEnable()
    {
        //SoundManager.I.PlaySFX(TypeSound.SFX_Lose);
    }

    public void ButtonRetryOnClick()
    {
        SoundManager.I.PlaySFX(TypeSound.SFX_Click);
        TransitionGUI.I.ShowTransition(
() =>
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    // stepFinish → chạy sau khi fade-in xong
});
    }
}
