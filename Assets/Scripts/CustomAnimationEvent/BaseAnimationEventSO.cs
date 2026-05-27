using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAnimationEventSO : ScriptableObject
{
    public AnimationClip clip;
    public List<AnimationEventEntry> events = new();

}
