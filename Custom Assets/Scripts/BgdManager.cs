using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgdManager : MonoBehaviour
{
    
    /////////////////////////////////////////////////////////////
    // fields
    /////////////////////////////////////////////////////////////

    #region fields

    [SerializeField]
    Animator curtainAnim_Cp;

    [SerializeField]
    AudioSource bgdAudioS_Cp;

    #endregion

    /////////////////////////////////////////////////////////////
    // methods
    /////////////////////////////////////////////////////////////

    void Awake()
    {
        int screenHeight = Screen.height;
        Screen.SetResolution((int)((float)screenHeight * 1366f / 768f), screenHeight, true);
    }

    // Init
    public void Init()
    {
        if(!curtainAnim_Cp.gameObject.activeInHierarchy)
        {
            curtainAnim_Cp.gameObject.SetActive(true);
        }

        curtainAnim_Cp.transform.SetAsLastSibling();
    }

    // 
    public void Play()
    {
        CurtainUp();

        // PlayBgdAudio();
    }

    // Curtain up
    public void CurtainUp()
    {
        curtainAnim_Cp.SetTrigger("up");
    }

    // Curtain down
    public void CurtainDown()
    {
        curtainAnim_Cp.SetTrigger("down");
    }

    // 
    public void PlayBgdAudio()
    {
        if(bgdAudioS_Cp.isPlaying)
        {
            bgdAudioS_Cp.Stop();
        }

        bgdAudioS_Cp.Play();
    }
}
