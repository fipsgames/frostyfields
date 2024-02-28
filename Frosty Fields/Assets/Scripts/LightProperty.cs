using UnityEngine;

[System.Serializable]
public class LightProperty
{

    public string name;

    public Color color;
    [Range(0f, 1f)]
    public float intensity;

    [Range(0f, 360f)]
    public float direction;
    [Range(0f, 90f)]
    public float angle;

    [HideInInspector]
    public Quaternion rotation;

    public LightProperty()
    {
        name = "tmp";
        color = new Color();
        intensity = 0;
        direction = 0;
        angle = 0;
        rotation = new Quaternion();
        ////Logger.Log("Light Instanced: " + name + " color: " + color.ToString() + " intensity: " + intensity + " direction: " + direction + " angle: " + angle + " rotation: " + rotation.ToString(), null, true);
    }
}