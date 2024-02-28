using UnityEngine;
using util;

public class LightScript : MonoBehaviour {


    public float speed;
    public LightProperty[] lightProperties;
    private int numLightProperties; 
    private Light myLight;

    private float floatIndex;
    private int upperPropIndex;
    private int lowerPropIndex;
    private float goalInterpolation;
    private LightProperty upperLightProperty;
    private LightProperty lowerLightProperty;
    private LightProperty lastLightProperty;
    private LightProperty nextLightProperty;
    private LightProperty currentLightProperty;
    private float fraction;

    private void Awake()
    {
        fraction = -1.0f;
        myLight = GetComponent<Light>();
        numLightProperties = lightProperties.Length;
        foreach (LightProperty l in lightProperties)
        {
            l.rotation = Quaternion.Euler(l.angle, -l.direction, 0.0f);
        }
        currentLightProperty = new LightProperty();
        lastLightProperty = new LightProperty();
        nextLightProperty = new LightProperty();
        ApplyProperty(lightProperties[0]);
    }

    public void ApplyProperty(LightProperty lightProperty)
    {
        myLight.color = lightProperty.color;
        myLight.intensity = lightProperty.intensity;
        myLight.gameObject.transform.rotation = lightProperty.rotation;
    }

    public void SetGoalProgress(float goalProgess)
    {
        floatIndex = goalProgess * (numLightProperties -1);
        upperPropIndex = System.Math.Max((int)System.Math.Ceiling(floatIndex), 1);
        lowerPropIndex = upperPropIndex - 1;
        goalInterpolation = floatIndex - lowerPropIndex;
        upperLightProperty = lightProperties[upperPropIndex];
        lowerLightProperty = lightProperties[lowerPropIndex];
        ////Logger.Log("LightScript goalProgress: " + goalProgess + " interpolationProp: " + (goalProgess * numLightProperties) + " upperProp: " + upperPropIndex + " lowerProp: " + lowerPropIndex + " goalInterpolation: " + goalInterpolation, this, true);
        lastLightProperty = currentLightProperty;
        nextLightProperty = InterpolateProperty(nextLightProperty, lowerLightProperty, upperLightProperty, goalInterpolation);
        fraction = 0.0f;
    }

    public LightProperty InterpolateProperty(LightProperty resultLightProp, LightProperty lowerLightProp, LightProperty higherLightProp, float interpolationFraction)
    {
        resultLightProp.color = higherLightProp.color * interpolationFraction + lowerLightProp.color * (1 - interpolationFraction);
        resultLightProp.intensity = higherLightProp.intensity * interpolationFraction + lowerLightProp.intensity * (1 - interpolationFraction);
        resultLightProp.rotation = Quaternion.Slerp(lowerLightProp.rotation, higherLightProp.rotation, interpolationFraction);
        ////Logger.Log("LightScript Interpolating fraction: " + interpolationFraction + " color: "  + resultLightProp.color + " intensity: " + resultLightProp.intensity + " rotation: " + resultLightProp.rotation, this, true);
        return resultLightProp;
    }

    // Update is called once per frame
    void Update () {
        if (fraction >= 0.0f && fraction <= 1.0f)
        {
            fraction += speed * Time.deltaTime;
            ////Logger.Log("PostProcessingPalette fraction: " + fraction + " smooth: " + Util.SmoothFraction(fraction), this, true);
            ApplyProperty(InterpolateProperty(currentLightProperty, lastLightProperty, nextLightProperty, Util.SmoothFraction(fraction)));
            ////Logger.Log("PostProcessingPalette moved to " + gameObject.transform.position, this, true);
        }
    }

    
}
