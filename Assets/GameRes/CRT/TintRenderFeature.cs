using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TintRenderFeature : ScriptableRendererFeature
{

    private TintPass _tintPass;

    //创建一个TintPass,之后放到addRenderPass中执行
    public override void Create()
    {
        _tintPass = new TintPass();
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_tintPass);
    }
    
    //-----------------------------------------------------------------------------------------------------------------
    
    class  TintPass : ScriptableRenderPass
    {
        private Material _material;

        private int tintId = Shader.PropertyToID("_Temp");
        private RenderTargetIdentifier src, tint;//渲染的源和修改后的结果
        
        public TintPass()
        {
            //从Shader创建材质并获取
            if (!_material)
            {
                _material = CoreUtils.CreateEngineMaterial("Unlit/CustomColorTint");
            }

            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            src = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(tintId,descriptor,FilterMode.Point);
            tint = new RenderTargetIdentifier(tintId);
        }


        //执行commanderbuffer
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get("TintRenderFeature");
            VolumeStack volumeStack = VolumeManager.instance.stack;
            PostScreenTint customSetting = volumeStack.GetComponent<PostScreenTint>();

            if (customSetting.IsActive())
            {
                _material.SetFloat("_VignetteWidth",(float)customSetting.vignetteWidth);
                _material.SetFloat("_Intensity",(float)customSetting.intensity);
                _material.SetFloat("_ScanlinePower",(float)customSetting.ScanlinePower);
                _material.SetFloat("_CycleIndex",(float)customSetting.CycleIndex);
                _material.SetFloat("_TransparencyFactor",(float)customSetting.TransparencyFactor);
                Blit(commandBuffer,src,tint,_material,0);
                Blit(commandBuffer,tint,src);
            }
            
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tintId);
        }
        
    }
}


