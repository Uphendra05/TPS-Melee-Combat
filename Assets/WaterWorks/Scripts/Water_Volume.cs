using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Water_Volume : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private readonly Material _material;
        private RTHandle _tempRT;

        public CustomRenderPass(Material mat)
        {
            _material = mat;
            // Needed if the shader reads _CameraDepthTexture (water depth fade, etc.)
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0; // color-only copy
            RenderingUtils.ReAllocateIfNeeded(ref _tempRT, desc, name: "_TemporaryColourTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_material == null) return;
            if (renderingData.cameraData.cameraType == CameraType.Reflection) return;

            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            CommandBuffer cmd = CommandBufferPool.Get("Water Volume");

            Blit(cmd, source, _tempRT, _material); // apply effect
            Blit(cmd, _tempRT, source);            // copy back

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }

        public void Dispose()
        {
            _tempRT?.Release();
        }
    }

    [System.Serializable]
    public class _Settings
    {
        public Material material = null;
        public RenderPassEvent renderPass = RenderPassEvent.AfterRenderingSkybox;
    }

    public _Settings settings = new _Settings();

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        if (settings.material == null)
            settings.material = Resources.Load<Material>("Water_Volume");

        m_ScriptablePass?.Dispose();
        m_ScriptablePass = new CustomRenderPass(settings.material)
        {
            renderPassEvent = settings.renderPass
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;
        renderer.EnqueuePass(m_ScriptablePass);
    }

    protected override void Dispose(bool disposing)
    {
        m_ScriptablePass?.Dispose();
    }
}