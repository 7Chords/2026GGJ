using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Color = System.Drawing.Color;

[Serializable, VolumeComponentMenuForRenderPipeline("CRTFilter",typeof(UniversalRenderPipeline))]
public class CRTVolume : VolumeComponent,IPostProcessComponent
{
    public FloatParameter intensity=new FloatParameter(1);
    public ColorParameter tintColor = new ColorParameter(UnityEngine.Color.white);
    
    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
}
