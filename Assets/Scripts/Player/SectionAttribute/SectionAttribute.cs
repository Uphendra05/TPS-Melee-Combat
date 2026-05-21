using UnityEngine;

public class SectionAttribute : PropertyAttribute
{
    public string name;

    public SectionAttribute(string name)
    {
        this.name = name;
    }
}