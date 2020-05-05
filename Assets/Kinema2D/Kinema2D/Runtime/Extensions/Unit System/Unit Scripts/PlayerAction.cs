using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent (typeof(PlayerController), typeof(Animator))]
public class PlayerAction : MonoBehaviour
{

    public PlayerController pc;
    public ActionState currentActionState;
    public Animator anim;
    public PlayerInput input;
    public PlayerControls controls;
    public bool Attack;

    public UnityEvent OnSwordSwing;

    public void Start()
    {
        controls = pc.controls;
        controls.Player.WestButton.Enable();
    }

    public void Awake()
    {
        pc = GetComponent<PlayerController>();
    }

    public void SetState(ActionState nextState)
    {
        if (currentActionState != null)
        {
            currentActionState.OnStateExit();
        }

        currentActionState = nextState;

        if (currentActionState != null)
        {
            currentActionState.OnStateEnter();
        }
    }

    public void FreePlayer()
    {
        pc.SetMoveState(pc.freeMoveState);
    }

    public virtual void InputCheck()
    {

      if (controls.Player.WestButton.triggered)
        {
            anim.SetTrigger("Punch");

            Debug.Log("PUNCH");

            //SFX_II.instance.Play("SwordSwing");
            OnSwordSwing.Invoke();
            pc.SetMoveState(pc.attackState);

            Invoke("FreePlayer", 0.3f);
        }


        if (controls.Player.EastButton.triggered)
        {
            anim.SetTrigger("Kick");

            Debug.Log("KICK");

            //SFX_II.instance.Play("SwordSwing");
            OnSwordSwing.Invoke();
            pc.SetMoveState(pc.attackState);

            Invoke("FreePlayer", 0.3f);
        }

        if (controls.Player.NorthButton.triggered)
        {

            anim.SetTrigger("UpperCut");

            Debug.Log("UPPERCUT");

            OnSwordSwing.Invoke();
            pc.SetMoveState(pc.attackState);

            Invoke("FreePlayer", 0.3f);

        }

        //if (controls.Player.EastButton.triggered)
        //{

        //    anim.SetTrigger("JumpKick");

        //    Debug.Log("ATTACK");

        //    OnSwordSwing.Invoke();
        //    pc.SetMoveState(pc.attackState);

        //    Invoke("FreePlayer", 0.3f);

        //}
    }

    // Update is called once per frame
    void Update()
    {
        InputCheck();
    }
}

public class ActionState : State
{

}
