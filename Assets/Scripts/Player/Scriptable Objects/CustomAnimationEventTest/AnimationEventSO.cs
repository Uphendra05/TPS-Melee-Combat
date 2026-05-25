using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Example/AnimationEvent")]
public class AnimationEventSO : ScriptableObject
{

    public AnimationClip clip;
    public float aniamtionSpeed;
    public List<AnimationEventEntry> events = new ();
   


}

[System.Serializable]
public class AnimationEventEntry
{
    public string name;
    public float startTime;
    public float endTime;
    public Color color = Color.white;


}
