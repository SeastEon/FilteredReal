Shader "Custom/Terrain"
{
    Properties {
        textTexture("Texture", 2D) = "white"{}
        testScale("Scale", Float) = 1
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int MaxLayerCount = 8;
        const static float epsilon = 1E-4;
        
        int layersCount;
        float3 BaseColors[MaxLayerCount];
        float BaseStartHeights[MaxLayerCount];
        float BaseBlends[MaxLayerCount];
        float BaseColorStrength[MaxLayerCount];
        float BaseTextureScales[MaxLayerCount];

        float minHeight;
        float maxHeight;

        sampler2D textTexture;
        float testScale;

        UNITY_DECLARE_TEX2DARRAY(BaseTextures);

        struct Input { 
            float3 worldPos; 
            float3 worldNormal;
        };

        float inverseLerp(float a, float b, float value){
             return saturate((value - a) / (b - a));
        }

        float3 triplannar(float3 worldPos, float scale, float3 blendAxes, int textureIndex){
            float3 ScaledWorldPos = worldPos/ scale;
            
            float3 xprojection = UNITY_SAMPLE_TEX2DARRAY(BaseTextures, float3 (ScaledWorldPos.y , ScaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yprojection = UNITY_SAMPLE_TEX2DARRAY(BaseTextures, float3 (ScaledWorldPos.x , ScaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zprojection = UNITY_SAMPLE_TEX2DARRAY(BaseTextures, float3 (ScaledWorldPos.x , ScaledWorldPos.y, textureIndex)) * blendAxes.z;
            return xprojection + yprojection + zprojection;
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

            for(int i = 0; i < layersCount; i ++){
                float drawStrength = inverseLerp(-BaseBlends[i]/2 - epsilon, BaseBlends[i]/2, heightPercent - BaseStartHeights[i]);
                
                float3 baseColor = BaseColors[i] * BaseColorStrength[i];
                float3 textureColor = triplannar(IN.worldPos, BaseTextureScales[i], blendAxes, i) * (i - BaseColorStrength[i]);
                
                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
            }

        }
        ENDCG
    }
    FallBack "Diffuse"
}
