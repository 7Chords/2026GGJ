using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[Serializable, VolumeComponentMenuForRenderPipeline("Color Tint",typeof(UniversalRenderPipeline))]
public class PostScreenTint : VolumeComponent,IPostProcessComponent
{
    public FloatParameter intensity=new FloatParameter(1);
    public FloatParameter vignetteWidth=new FloatParameter(0.1f);
    public FloatParameter ScanlinePower = new FloatParameter(1);
    public ClampedFloatParameter CycleIndex = new ClampedFloatParameter(0.2f,0,1f);
    public ClampedFloatParameter TransparencyFactor = new ClampedFloatParameter(0.2f, 0, 1f);
    
    
    
    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
}
