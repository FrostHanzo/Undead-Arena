using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerController : UnitMovement
{
    //Beginning MovementController
    
        [SerializeField]
    public PlayerControls controls;

    [Header("Attributes")]

    public PlayerMovementStats playerMoveStats;

    public float curMoveSpeed;

    public bool WallSlides = true;

    // return values from moveStats;
    public override float MoveSpeed() {return playerMoveStats.baseMoveSpeed;}
        public virtual float JumpForce() { return playerMoveStats.jumpForce; }
          public virtual float WallJumpRecoverySpeed() { return playerMoveStats.wallJumpRecoverySpeed; }
            public virtual float WallSlideSpeed() { return playerMoveStats.maxWallSlideSpeed;}
                public virtual float MaxFallSpeed() { return playerMoveStats.maxFallSpeed;}

    
    [Space(10)]

    [Header ("Player State Management")]
    public FreeMoveState freeMoveState;
    public AttackState attackState;

    [Header("Collision State")] // move functionality into MoveAndCollide2D?
    public bool onWall;
    public bool wallCollision;
    public bool wallSliding;
    public bool wallSlideLeft;
    public bool wallSlideRight;

    public UnityEvent OnJumpEvent;

    private void HandleAttack(InputAction.CallbackContext context){
        // Attack Functionality
        Debug.Log("Attack");
    }

    public void AttackFinish(){
        //SetMoveState(freeMoveState); // neesd to be cached
    }

    public virtual void HandleJump(InputAction.CallbackContext context){
        if (movement.grounded)
            GroundedJump();
        else if (wallSliding)
        {
            SetMoveState(new WallJumpState(this, GetWallSlideDirection(), WallJumpRecoverySpeed()));
        }

        Debug.Log("Old Handle Jump");

    }

    public virtual void Initialize()
    {
        InitializeMoveStates();
        this.unitStats = playerMoveStats;
        
        
    }

    public void Awake(){

        controls = new PlayerControls();
        
         var northButton = controls.Player.NorthButton;
            //northButton.performed += HandleAttack;
            northButton.Enable();

        var eastButton = controls.Player.EastButton;
            eastButton.Enable();

        var westButton = controls.Player.WestButton;
            westButton.Enable();

        var jump = controls.Player.SouthButton;
            jump.performed += HandleJump;
            jump.Enable();

        var horizontal = controls.Player.Horizontal;
        
            horizontal.Enable();

        Initialize();
    }
    

    public virtual void InitializeMoveStates(){
        freeMoveState = new FreeMoveState(this);
        currentMovementState = freeMoveState;
        attackState = new AttackState(this);

        this.defaultState = freeMoveState;
    }
    

    public void IAttacked(Vector2 kb, int damage, float power){
        //SetMoveState(new KnockbackState(this, kb, power));
    }

    public virtual void OnLand(){ // invoke this through MoveAndCollideEvents
        //anim.SetTrigger("Land");
        //AttackFinish();
        Debug.Log("Landed");
    }
    
    public virtual void AbilityInputCheck(){

    }

    public void GroundedJump(){
        movement.velocity.y = JumpForce();
        OnJump();
        //anim.SetTrigger("Jump");
        OnJumpEvent.Invoke();
    }

    public virtual void AirJump()
    {
        movement.velocity.y = JumpForce();
        OnJump();
        
        //anim.SetTrigger("AirJump");
        OnJumpEvent.Invoke();
    }

    public override UnitMoveState GetNextState()
    {
        return freeMoveState;
    }

    // Internal Update
    public override void Update(){

        // Set Input
        Vector2 pInput = controls.Player.Horizontal.ReadValue<Vector2>();
        input.x = pInput.x;
        input.y = pInput.y;

        //Debug.Log(xInput);

        if (currentMovementState != null){
            currentMovementState.Tick();
        }

        if (currentMovementState.finished)
        {
            SetMoveState(GetNextState()); // you can use this with a neutral state, AKA Free Move State, and then this coupled move state can branch to new ones.
        }


        ComputeVelocity();


        // if our current state allows overriding

        if (!currentMovementState.overridesStateChanges)
        {

        }

        AbilityInputCheck();

        if (movement.velocity.y < -this.MaxFallSpeed())
        {
            movement.velocity.y = -this.MaxFallSpeed();
        }

        
    }

    public override void FixedUpdate()
    {

        base.FixedUpdate();

        curMoveSpeed = MoveSpeed();

        if (movement.justLanded)
        {
            this.OnLand();
        }
    }

    // Called in Update
    public override void ComputeVelocity(){ // called every UPDATE frame by the parent class. Sets PhysicsObject.targetvelocity = move;

        wallSliding = false;
        wallSlideLeft = false;
        wallSlideRight = false;

        move = Vector2.zero;

        // Move.x = currentMovementState.GetXVelocity();
        move.x = currentMovementState.GetXVelocity();
        // Move.y = currentMovementState.GetYVelocity();
        move.y = currentMovementState.GetYVelocity();

        if (move.x > 0){
            SetFlipped(false);
            /* spriteRendy.flipX = false; */
        } else if (move.x < 0){
            SetFlipped(true);
            /* spriteRendy.flipX = true; */
        }

        if (movement.onWall && move.x > 0){
            SetFlipped(true);
            wallSlideLeft = true;
            /* spriteRendy.flipX = true; */
        } else if (movement.onWall && move.x < 0){
            SetFlipped(false);
            wallSlideRight = true;
            /* spriteRendy.flipX = false; */
        }

        this.wallCollision = movement.wallCollision;
        
        wallSliding = (wallSlideLeft || wallSlideRight);

        if (WallSlides && wallSliding && movement.velocity.y <= -WallSlideSpeed()){
            movement.velocity.y = -WallSlideSpeed();
        }

        // In this implementation, target velocity is specifically for the X;
        movement.SetYOverride(currentMovementState.overridesYVelocity);
        movement.SetVelocity(move);

        // we can find a way to make simple for functions indicating, IsOnWall() or IsOnWallToRight();
    }

    public float GetWallSlideDirection(){

        float direction = 0f;

        if (!wallSliding){
            direction = 0f;
        } else if (wallSlideLeft){
            direction = -1f;
        } else if (wallSlideRight) direction = 1f;

        Debug.Log("Direction");
        return direction;
    }

    public int GetDirection()
    {
        if (this.flipped)
        {
            return -1;
        }
        else return 1;
    }

    public void SetFlipped(bool flip){

        if (currentMovementState.overridesFlipDirection)
        {
            return;
        }

        if (flip != this.flipped){
            OnFlipped();
            // currentMovementState.onFlipped?!?
        }

        flipped = flip;

        
    }

    public void OnFlipped(){
        if (movement.grounded){
            
        }
    }

    public virtual void OnJump(){
        
    }
}

public class WallJumpState : PlayerMovementState // Does not need to be a player Movement State, wall Jump Direction can be Genericized
{
    float wallJumpRecoverySpeed = 1f;
    float lerpPercentage;
    float initialJumpVelocity;
    float stateVelocity = 0f;

    public WallJumpState(PlayerController player, float wallJumpDireciton) : base(player)
    {

        lerpPercentage = 0;

        initialJumpVelocity = player.MoveSpeed() * wallJumpDireciton;

        // Jump the player
        
    }

    public override void OnStateEnter()
    {
        //SFX_II.instance.Play("Jump");

        player.movement.velocity.y = player.JumpForce();

        overridesFlipDirection = true;
    }

    public WallJumpState(PlayerController player, float wallJumpDireciton, float recoverySpeed) : base(player)
    {
        lerpPercentage = 0;

        initialJumpVelocity = player.MoveSpeed() * wallJumpDireciton;

        this.wallJumpRecoverySpeed = recoverySpeed;
    }


    public override void Tick()
    {

        stateVelocity = Mathf.Lerp(initialJumpVelocity, (player.input.x * player.MoveSpeed()), lerpPercentage);

        lerpPercentage += Time.deltaTime * wallJumpRecoverySpeed;

        if (lerpPercentage >= 0.5f)
        {
            overridesFlipDirection = false; // return flip control back to momentum
        }

        if (lerpPercentage >= 1.0f || player.movement.grounded)
        {
            player.SetMoveState(player.freeMoveState);
        }
    }

    public override float GetXVelocity()
    {
        return stateVelocity;
    }
}

[System.Serializable]
public abstract class PlayerMovementState : UnitMoveState {

    
    public PlayerController player;

    public PlayerMovementState(PlayerController player) : base (player){
        this.player = player;
        this.stateName = this.GetType().ToString();
    }

    public override float GetXVelocity() {return player.move.x;}
    public override float GetYVelocity() {return player.move.y;}
}

public class FreeMoveState : PlayerMovementState{

    public FreeMoveState (PlayerController player) : base (player){}

    public override float GetXVelocity(){
        return player.input.x * player.MoveSpeed();
    }

}

public class AttackState : PlayerMovementState {
     public AttackState(PlayerController player) : base (player){
        this.overridesFlipDirection = true;
     }

     public override float GetXVelocity(){

         if (player.movement.grounded){
             return 0f;
         } else return player.input.x * player.MoveSpeed();
         
     }
}
