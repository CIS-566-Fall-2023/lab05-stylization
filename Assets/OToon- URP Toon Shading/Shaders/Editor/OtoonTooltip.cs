using System.Collections.Generic;

namespace OToon
{
    public static class OtoonToolTip
    {
        public static Dictionary<string, string> Tips = new Dictionary<string, string>
    {
     {"_ToonBlending","Blending between pure PBR( 0 ) and pure Toon (1)"},
     {"_BaseColor","Albedo color"},
     {"_SpecColor","Specular Color"},
     {"_SpecularSize","Control size of specular"},
     {"_SpecularFalloff","Control the smoothness of the edge of specular"},
     {"_SpecularClipMask","Pattern texture that mask out specular"},
     {"_SpecClipMaskScale","Pattern texture uv scale"},
     {"_SpecularClipStrength","Specular cutoff thredshold"},
     {"_StepViaRampTexture","Control stylize lighting through sampling ramp texture"},
     {"_NoiseScale","Scale of noise map"},
     {"_NoiseStrength","Noise Strength"},
     {"_DiffuseStep","Offseting the diffuse light direction influence"},
     {"_RimLightAlign","-1 = align as back light, 1 = align with light direction, 0 = align view edge"},
     {"_RimLightSmoothness","Control the smoothness of rim light"},
     {"_HalftoneNoiseClip","Mask thredshold of the noise map"},
     {"_BrushLowerCut","Cutout some of the light align part(use it to cutout parts that is too small)"},
     {"_BrushSize","Size of each halftone's pattern"},
     {"_HalftoneTilling","Scaling of halftone uv, increase the number to increase pattern density"},
     {"_ToneDiffuseStep","Offseting the diffuse light direction influence on overlay"},
     {"_SizeFalloff","Control how much the pattern size falloff with lights"},
     {"_HalfToneIncludeReceivedShadow","If the overlay calculation include shadowed area cast by others"},
     {"_SpecShadowStrength","Control how much specular can pass throgh shadowed area"},
     {"_HairSpecColor","Color of the anisotropic specular color"},
     {"_EnabledHairSpec","Enable the anisotropic specular"},
     {"_SpherizeNormalOrigin","The center of the sphere in obejct space(the desired value might vary on different mesh)"},
     {"_SpherizeNormalEnabled","Enable to treat the mesh normal as spehere, useful to flatten face mesh normal"},
     {"_HatchingSmoothness","Control hatching Edge Smoothness"},
     {"_HatchingDiffuseOffset","Control how much hatching affect by light direction"},
     {"_ShadowColor", "Custom shadow color, use alpha value to control the blending strength"}
    };
    }
}