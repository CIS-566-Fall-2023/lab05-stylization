using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace OToon
{
    public class OutlineObjectFeature : ScriptableRendererFeature
    {
        DrawOutlineObjectPass outlineObjectPass;
        Material depthNormalsMaterial;
        public LayerMask mask;
        public RenderPassEvent Event;
        public RenderQueueRange Range;
        public override void Create()
        {
            Range = RenderQueueRange.opaque;
            outlineObjectPass = new DrawOutlineObjectPass(name, true, Event, Range, mask, StencilState.defaultValue, 0);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            outlineObjectPass.Setup(renderingData.cameraData.cameraTargetDescriptor);
            renderer.EnqueuePass(outlineObjectPass);
        }

        public class DrawOutlineObjectPass : ScriptableRenderPass
        {
            FilteringSettings m_FilteringSettings;
            RenderStateBlock m_RenderStateBlock;
            ShaderTagId m_ShaderTagId = new ShaderTagId("OutlineObject");
            internal RenderTextureDescriptor descriptor { get; private set; }
            string m_ProfilerTag;
            ProfilingSampler m_ProfilingSampler;
            bool m_IsOpaque;

            bool m_UseDepthPriming;

            static readonly int s_DrawOutlineObjectPassDataPropID = Shader.PropertyToID("_DrawOutlineObjectPassData");

            public void Setup(RenderTextureDescriptor baseDescriptor)
            {
                descriptor = baseDescriptor;
            }


            public DrawOutlineObjectPass(string profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
            {
                m_ProfilerTag = profilerTag;
                m_ProfilingSampler = new ProfilingSampler(profilerTag);
                renderPassEvent = evt;
                m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
                m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            }


            /// <inheritdoc/>
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                using (new ProfilingSample(cmd, m_ProfilerTag))
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    var sortFlags = SortingCriteria.CommonOpaque;
                    var sortingSettings = new SortingSettings(renderingData.cameraData.camera);
                    sortingSettings.criteria = sortFlags;
                    var drawSettings = new DrawingSettings(m_ShaderTagId, sortingSettings);
                    drawSettings.perObjectData = PerObjectData.None;

                    ref CameraData cameraData = ref renderingData.cameraData;
                    Camera camera = cameraData.camera;
                    context.DrawRenderers(renderingData.cullResults, ref drawSettings,
                        ref m_FilteringSettings, ref m_RenderStateBlock);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}