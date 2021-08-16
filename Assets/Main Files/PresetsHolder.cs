using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "DayNight Preset", menuName = "Scriptable Objects/SkyPreset")]
public class PresetsHolder : ScriptableObject
{
    //public Material daySkyBox;
    //public Material SunsetSkyBox;
    //public Material NightSkybox;
    public Cubemap nightSky;
    public Cubemap daySky;
    public Cubemap DawnSky;


    public Gradient FogGradient;
    public Gradient DirectionLight;
    public Gradient AmbientColor;


    public float MaxDirectionalLightIntensity=1;

}


/*
 * 
 * 6 - 6 day sky
 * 6 - 9 dawn
 * 9 - 4 night
 * 4 - 6 dawn
 */
