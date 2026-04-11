using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Randomizes an integer parameter at transition to the Entry node of a state machine. This parameter can be used for choosing a random transition out from the Entry node. 
//
// How to setup a random transition from the Entry node of a sub state machine: 
// 
//  1) Attach SMB Random Int to Your Sub State Machine 
// 
//     - copy this SMBRandomInt.cs to your Assets folder 
//     - click your sub state machine in Animator, select "Add Behavour" in object inspector and choose SMB Random Int
//  
//  2) Configure SMB Random Int in Inspector
//   
//     - Enter new name for the random parameter if you are not happy with RandomInt
//     - Add the chosen parameter to your Animator's list of parameters as Integer type
//     - Configure MaxValue = [number of outgoing transitions - 1]
// 
//  3) Setup outgoing transitions
//   
//     - default transition always runs if any other doesn't -- let's have it run if our random value is 0
//     - for the first additional outgoing transition set condition [random parameter name] Equals 1
//     - for the second additional outgoing transition set condition [random parameter name] Equals 2
//     - ... etc etc ... 


public class SBMRandomInt : StateMachineBehaviour
{
    [Tooltip("The parameter name that will be set to random integer value. You must add this into your animator's parameter list as an Integer.")]
    public string parameterName = "RandomInt";

    [Tooltip("Minimum generated random value.")]
    public int minValue = 0;

    [Tooltip("Maximum generated random value.")]
    public int maxValue = 1;

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        int value = Random.Range(minValue,maxValue+1);
        animator.SetInteger(parameterName, value);
    }

}