using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
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
    Animator playerAnim_Cp;

    [SerializeField]
    float horizonCoef = 40f, verticalForwardCoef = 5f, verticalBackCoef = 1f;

    //-------------------------------------------------- public fields
    public GameState_En gameState;

    //-------------------------------------------------- private fields
    float horizonAxis, verticalAxis, shiftAxis;

    string jumpStateName = "Jump", slideStateName = "Slide", damageStateName = "Damage",
        getupStateName = "Getup";

    public bool damagedState;

    bool m_jumpState, m_slideState, m_damageState, m_getupState;

    #endregion

    //////////////////////////////////////////////////////////////////////
    // properties
    //////////////////////////////////////////////////////////////////////
    #region properties

    //-------------------------------------------------- public properties

    //-------------------------------------------------- private properties
    bool isSpecialActionState
    {
        get
        {
            bool result = false;

            if(jumpState || slideState || damageState || getupState)
            {
                result = true;
            }

            return result;
        }
    }

    bool jumpState
    {
        get
        {
            return playerAnim_Cp.GetBool(jumpStateName) ||
            playerAnim_Cp.GetCurrentAnimatorStateInfo(0).IsName(jumpStateName) ||
            m_jumpState;
        }
        set
        {
            m_jumpState = value;
            playerAnim_Cp.SetBool(jumpStateName, value);
        }
    }

    bool slideState
    {
        get
        {
            return playerAnim_Cp.GetBool(slideStateName) ||
            playerAnim_Cp.GetCurrentAnimatorStateInfo(0).IsName(slideStateName) ||
            m_slideState;
        }
        set
        {
            m_slideState = value;
            playerAnim_Cp.SetBool(slideStateName, value);
        }
    }

    bool damageState
    {
        get
        {
            return playerAnim_Cp.GetBool(damageStateName) &&
            playerAnim_Cp.GetCurrentAnimatorStateInfo(0).IsName(damageStateName) &&
            m_damageState;
        }
        set
        {
            m_damageState = value;
            playerAnim_Cp.SetBool(damageStateName, value);
        }
    }

    bool getupState
    {
        get
        {
            return playerAnim_Cp.GetBool(getupStateName) &&
            playerAnim_Cp.GetCurrentAnimatorStateInfo(0).IsName(getupStateName) &&
            m_getupState;
        }
        set
        {
            m_getupState = value;
            playerAnim_Cp.SetBool(getupStateName, value);
        }
    }

    bool isMoving
    {
        get
        {
            return !Mathf.Approximately(playerAnim_Cp.GetFloat("Speed"), 0f);
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    // methods
    //////////////////////////////////////////////////////////////////////

    //-------------------------------------------------- Start is called before the first frame update
    void Start()
    {
        
    }

    //-------------------------------------------------- Update is called once per frame
    void Update()
    {
        if(gameState == GameState_En.Play)
        {
            Move();
        }
    }
    
    //////////////////////////////////////////////////////////////////////
    // Init
    //////////////////////////////////////////////////////////////////////
    #region Init

    //-------------------------------------------------- Init
    public void Init()
    {
        gameState = GameState_En.Inited;
    }

    #endregion

    //--------------------------------------------------
    public void Play()
    {
        gameState = GameState_En.Play;
    }

    //--------------------------------------------------
    void Move()
    {
        // 
        verticalAxis = Input.GetAxis("Vertical");
        shiftAxis = Input.GetAxis("Shift");
        if(verticalAxis > 0.1f)
        {
            verticalAxis += shiftAxis;
        }
        
        playerAnim_Cp.SetFloat("Speed", verticalAxis);

        verticalAxis /= 2f;

        if(!damageState && !getupState && !damagedState)
        {
            if(verticalAxis > 0.05f)
            {
                transform.Translate(Vector3.forward * (verticalAxis - 0.05f) * Time.deltaTime
                    * verticalForwardCoef);
            }
            else if(verticalAxis < -0.05f)
            {
                transform.Translate(Vector3.forward * (verticalAxis + 0.05f) * Time.deltaTime
                    * verticalBackCoef);
            }
        }

        // 
        horizonAxis = Input.GetAxis("Horizontal");
        playerAnim_Cp.SetFloat("Direction", horizonAxis);

        if(!jumpState && !damageState && !getupState && !damagedState)
        {
            if(verticalAxis > 0.1f)
            {
                transform.Rotate(Vector3.up * horizonAxis * Time.deltaTime * horizonCoef);
            }
            else if(verticalAxis < -0.1f)
            {
                transform.Rotate(Vector3.up * horizonAxis * Time.deltaTime * horizonCoef);
            }
        }

        // 
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            Slide();
        }

        if(Input.GetKeyDown(KeyCode.J))
        {
            Damage();
        }

        if(Input.GetKeyDown(KeyCode.K))
        {
            Getup();
        }
    }

    //--------------------------------------------------
    void Jump()
    {
        if(isSpecialActionState)
        {
            return;
        }

        jumpState = true;

        StartCoroutine(Corou_Jump());
    }

    IEnumerator Corou_Jump()
    {
        yield return new WaitForSeconds(1f);

        jumpState = false;
    }

    //--------------------------------------------------
    void Slide()
    {
        if(isSpecialActionState)
        {
            return;
        }

        slideState = true;

        StartCoroutine(Corou_Slide());
    }

    IEnumerator Corou_Slide()
    {
        yield return new WaitForSeconds(1f);

        slideState = false;
    }

    //--------------------------------------------------
    void Damage()
    {
        if(isSpecialActionState || isMoving)
        {
            return;
        }

        damageState = true;

        damagedState = false;

        StartCoroutine(Corou_Damage());
    }

    IEnumerator Corou_Damage()
    {
        yield return new WaitUntil(() => playerAnim_Cp.GetCurrentAnimatorStateInfo(0)
            .IsName(damageStateName));

        damageState = false;

        yield return new WaitUntil(() => !damageState);

        damagedState = true;
    }

    //--------------------------------------------------
    void Getup()
    {
        if(!damagedState)
        {
            return;
        }

        getupState = true;

        StartCoroutine(Corou_Getup());
    }

    IEnumerator Corou_Getup()
    {
        yield return new WaitUntil(() => playerAnim_Cp.GetCurrentAnimatorStateInfo(0)
            .IsName(getupStateName));

        getupState = false;

        yield return new WaitUntil(() => !getupState);

        damagedState = false;
    }

}
