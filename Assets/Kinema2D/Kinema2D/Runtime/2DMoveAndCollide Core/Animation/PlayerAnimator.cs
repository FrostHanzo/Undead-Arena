using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator anim;
    public PlayerController player;
    public MoveAndCollide2D movement;
    public GameObject visuals;

    public void Update()
    {
        anim.SetFloat("VelocityX", Mathf.Abs(movement.velocity.x));
        //anim.SetFloat("VelocityY", movement.velocity.y);

        anim.SetBool("Grounded", movement.grounded);
        //anim.SetBool("OnWall", player.wallSliding);
        //anim.SetBool("Stunned", player.stunned);


        if (player.flipped){
            visuals.transform.localScale = (new Vector2 (-1, 1));
        } else visuals.transform.localScale = new Vector2(1, 1);
    }
}


