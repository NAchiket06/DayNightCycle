using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteInEditMode]
public class DayNightCycler : MonoBehaviour
{

    [SerializeField] float CycleTime =1f;
    

    public PresetsHolder preset;

    [SerializeField, Range(0, 24)] float TimeOfDay;

    #region Enums
    public enum TimeEnum
    {
        Morning, Night, Dawn
    }
    public TimeEnum td;
    #endregion
    
    [Header("Volumes")]
    #region Volumes

    public Volume SkyandFogVolume;
    public Volume GlobalVolume;
    #endregion

    [Header("Lights")]
    #region Lights

    #region Directional Lights
    public Light Sun;
    public Light Moon;
    float DirectionalLightsIntensity;
    float prevIntensity;
    #endregion

    #region SpotLights
    [SerializeField] GameObject LightsParent;
    float LightIntensity,refLightTime=0.01f;
    float MaxLightIntensity = 30000,LightswitchTime = 0.25f;
    #endregion

    #endregion

    [Header("Fog")]
    #region Fog
    float CurrentFogIntensity;
    float FogDayIntensity = 150f, FogNightIntensity = 75f;
    float RefFogIntensity, FogChangeTime = 0.1f;
    #endregion
    //---------------------------------------------------------------------------------------------START------------------------------------------------------------------------------------------------//

    private void Start()
    {
        GetDirectionLight();
        CheckIfPresetExists();

        DirectionalLightsIntensity = 0;
        TimeOfDay = 0;
    }
    //---------------------------------------------------------------------------------------------START------------------------------------------------------------------------------------------------//


    private void CheckIfPresetExists()
    {
        if (preset == null)
        {
            Debug.LogError("No Preset on Day Night Cycle");
        }
    }

    private void GetDirectionLight()
    {
        //Sun = GameObject.FindWithTag("Directional Light").GetComponent<Light>();
    }

    //---------------------------------------------------------------------------------------------UPDATE------------------------------------------------------------------------------------------------//
    private void Update()
    {
        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime/ CycleTime;
            TimeOfDay %= 24;
        }
        SetTimeEnum();
        updateSettings(TimeOfDay / 24f);
        RenderSettings.fogColor = preset.FogGradient.Evaluate(TimeOfDay/24f);
        UpdateLights();

    }


    //---------------------------------------------------------------------------------------------UPDATE------------------------------------------------------------------------------------------------//

    private void SetTimeEnum()
    {
        if (TimeOfDay >= 0.5f && TimeOfDay <= 11)
        {
            td = TimeEnum.Morning;
        }
        else if ((TimeOfDay >= 0 && TimeOfDay < 0.5) || (TimeOfDay >= 11 && TimeOfDay <= 11.5))
        {
            td = TimeEnum.Dawn;
        }
        else
        {
            td = TimeEnum.Night;
        }
    }

    void updateSettings(float percent)
    {
        UpdateSkybox(percent);
        UpdateDirectionalLights(percent);
        UpdateShadows(percent);
        UpdateEnvironmentReflections(percent);
        UpdateFog(percent);

    }


    private void UpdateDirectionalLights(float percent)
    {

        Sun.transform.localRotation = Quaternion.Euler(percent * 360f + 180, 0, 0);
        Moon.transform.localRotation = Quaternion.Euler(percent * 360 , 0, 0);
        Sun.color = preset.DirectionLight.Evaluate(percent);
        RenderSettings.ambientSkyColor = preset.DirectionLight.Evaluate(percent);

        if(td == TimeEnum.Morning || td == TimeEnum.Dawn)
        {
            //convert time of 0.083 to 0.45 to range of 0-preset.MaxIntensity;

            DirectionalLightsIntensity = (float)((percent) / (0.45)) * preset.MaxDirectionalLightIntensity;
            DirectionalLightsIntensity *= 10f;
            DirectionalLightsIntensity = Mathf.Clamp(DirectionalLightsIntensity, 0, 15000f);
            Sun.intensity = DirectionalLightsIntensity;

        }
   
        if (td == TimeEnum.Night)
        {
            DirectionalLightsIntensity = preset.MaxDirectionalLightIntensity - (float)((percent - 0.54) / (1 - 0.54)) * preset.MaxDirectionalLightIntensity;
            DirectionalLightsIntensity *= 2f;
            DirectionalLightsIntensity = Mathf.Clamp(DirectionalLightsIntensity, 0, 15000f);
            Moon.intensity = DirectionalLightsIntensity;
        }
        
    }

    private void UpdateShadows(float percent)
    {
        
        if(td == TimeEnum.Night)
        {
            Sun.shadows = LightShadows.None;    
            Moon.shadows = LightShadows.Soft;
        }
        else
        {
            Sun.shadows = LightShadows.Soft;
            Moon.shadows = LightShadows.None;
        }
    }

    private void UpdateSkybox(float percent)
    {
        PhysicallyBasedSky sky;
        if (SkyandFogVolume.profile.TryGet<PhysicallyBasedSky>(out sky))
        {
            if (td == TimeEnum.Night)
            {
               sky.spaceEmissionTexture.value = preset.nightSky;
            }
            if (td == TimeEnum.Morning)
            { 
                sky.spaceEmissionTexture.value = preset.daySky;
            }
            if (td == TimeEnum.Dawn)
            {
                //RenderSettings.skybox = preset.SunsetSkyBox;
                sky.spaceEmissionTexture.value = preset.daySky;

            }
            sky.spaceRotation.value = new Vector3(0, percent * 180f, 0);
        }


    }   

    private void UpdateEnvironmentReflections(float percent)
    {
        if (td == TimeEnum.Morning)
        {
            RenderSettings.ambientSkyColor = preset.AmbientColor.Evaluate(percent);
        }
    }

    private void UpdateFog(float percent)
    {
        Fog fog;
        if (GlobalVolume.profile.TryGet<Fog>(out fog))
        {
            fog.albedo.value = preset.FogGradient.Evaluate(percent);
        }

        if(td == TimeEnum.Morning)
        {
            fog.meanFreePath.value = Mathf.SmoothDamp(fog.meanFreePath.value, FogDayIntensity,ref RefFogIntensity, FogChangeTime);
        }
        if(td == TimeEnum.Night)
        {
            fog.meanFreePath.value = Mathf.SmoothDamp(fog.meanFreePath.value, FogNightIntensity, ref RefFogIntensity, FogChangeTime);
        }

    }

    private void UpdateLights()
    {

        if(LightsParent == null)
        {
            Debug.LogError("No Lights Parent attached");
            return;
        }
        if(td == TimeEnum.Night)
        {
            LightIntensity = Mathf.SmoothDamp(LightIntensity, MaxLightIntensity, ref refLightTime, LightswitchTime);
        }
        else 
        {
            LightIntensity = Mathf.SmoothDamp(LightIntensity, 0f, ref refLightTime, LightswitchTime);
        }

        for(int i = 0; i < LightsParent.transform.childCount; i++)
        {
            //if (i > 6)
            //{
            //    LightIntensity = Mathf.Clamp(LightIntensity, 0, 2000f);
            //    LightsParent.transform.GetChild(i).GetComponent<Light>().intensity = LightIntensity;
            //    continue;   
            //}
            LightsParent.transform.GetChild(i).GetComponent<Light>().intensity = LightIntensity;
        }
    }
}
