using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Example/AnimationEvent")]
public class AnimationEventSO : ScriptableObject
{

    public AnimationClip clip;
    public float animationSpeed;
    public List<AnimationEventEntry> events = new ();
   


}

public enum AnimationEventType { Notify, NotifyState }


[System.Serializable]
public class AnimationEventEntry
{
    public string name = "New Event";
    public AnimationEventType eventType = AnimationEventType.Notify;

    // Notify — single point
    [SerializeReference] public BaseAnimationNotify notify;
    public float triggerTime = 0.1f;

    // NotifyState — duration
    [SerializeReference] public BaseAnimationNotifyState notifyState;
    public float startTime = 0.1f;
    public float endTime = 0.4f;

    // Shared
    public Color color = Color.yellow;

    // Helpers
    public float GetStart() => eventType == AnimationEventType.Notify ? triggerTime : startTime;
    public float GetEnd() => eventType == AnimationEventType.Notify ? triggerTime : endTime;
    public bool IsState() => eventType == AnimationEventType.NotifyState;


}
