using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    
    //////////////////////////////////////////////////////////////////////
    // types
    //////////////////////////////////////////////////////////////////////
    #region types
    
    public enum GameState_En
    {
        Nothing, Inited, Play
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    // fields
    //////////////////////////////////////////////////////////////////////
    #region fields

    //-------------------------------------------------- SerializeField
    [SerializeField]
    BgdManager bgdManager_Cp;

    [SerializeField]
    Player player_Cp;

    //-------------------------------------------------- public fields
    public GameState_En gameState;

    //-------------------------------------------------- private fields

    #endregion

    //////////////////////////////////////////////////////////////////////
    // properties
    //////////////////////////////////////////////////////////////////////
    #region properties

    //-------------------------------------------------- public properties

    //-------------------------------------------------- private properties

    #endregion

    //////////////////////////////////////////////////////////////////////
    // methods
    //////////////////////////////////////////////////////////////////////

    //-------------------------------------------------- Start is called before the first frame update
    void Start()
    {
        Init();
    }

    //-------------------------------------------------- Update is called once per frame
    void Update()
    {
        
    }
    
    //////////////////////////////////////////////////////////////////////
    // Init
    //////////////////////////////////////////////////////////////////////
    #region Init

    //-------------------------------------------------- Init
    public void Init()
    {
        bgdManager_Cp.Init();

        player_Cp.Init();

        gameState = GameState_En.Inited;

        Play();
    }

    #endregion

    //--------------------------------------------------
    public void Play()
    {
        bgdManager_Cp.Play();

        player_Cp.Play();

        gameState = GameState_En.Play;
    }

}
