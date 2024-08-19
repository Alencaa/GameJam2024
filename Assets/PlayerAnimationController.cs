using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void Update()
    {
        //ResetingAnimator();
    }
    public void SwitchAnimation(string firstAnimation, string secondAnimation)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(parameter.name, false);
            }
        }
        animator.SetBool(firstAnimation, false);
        animator.SetBool(secondAnimation, true);
    }

    private void ResetingAnimator()
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            // Check if the parameter is a boolean
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                // Set the boolean parameter to false
                if (animator.GetCurrentAnimatorStateInfo(0).IsName(parameter.name) && !animator.IsInTransition(0))
                {
                    animator.SetBool(parameter.name, false);
                }
            }
        }
    }

    public void PlayerAnimationDirectly(string animation, bool condition)
    {
        animator.SetBool(animation, condition);
    }
}
