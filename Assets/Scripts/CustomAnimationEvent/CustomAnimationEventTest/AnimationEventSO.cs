using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Example/AnimationEvent")]
public class AnimationEventSO : ScriptableObject
{

    public AnimationClip clip;
    public float animationSpeed;
    public List<AnimationEventEntry> events = new ();
   


}


