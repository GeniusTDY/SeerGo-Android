Shader "Custom/DynamicRobotColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BodyColor ("Body Color", Color) = (1, 0.8, 0.2, 1)
        _EyeColor ("Eye Color", Color) = (1, 1, 0.3, 1)
        _BodyIntensity ("Body Intensity", Range(0, 2)) = 1.0
        _EyeIntensity ("Eye Intensity", Range(0, 3)) = 2.0
        _EyeThreshold ("Eye Threshold", Range(0, 1)) = 0.8
        _Saturation ("Saturation", Range(0, 2)) = 1.2
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BodyColor;
            float4 _EyeColor;
            float _BodyIntensity;
            float _EyeIntensity;
            float _EyeThreshold;
            float _Saturation;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            // RGB转HSV
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            // HSV转RGB
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, i.uv);
                
                if (original.a < 0.1)
                    return original;
                
                // 检测眼睛区域（基于亮度阈值）
                float luminance = dot(original.rgb, float3(0.299, 0.587, 0.114));
                bool isEye = luminance > _EyeThreshold;
                
                float3 finalColor;
                
                if (isEye)
                {
                    // 眼睛处理：强发光效果
                    float3 hsv = rgb2hsv(original.rgb);
                    hsv.y *= _Saturation; // 增强饱和度
                    float3 baseRGB = hsv2rgb(hsv);
                    
                    // 应用眼睛颜色并增强发光
                    finalColor = baseRGB * _EyeColor.rgb * _EyeIntensity;
                    // 添加发光溢出效果
                    finalColor += _EyeColor.rgb * (luminance - _EyeThreshold) * _EyeIntensity * 2.0;
                }
                else
                {
                    // 身体处理：保持纹理细节的基础上变色
                    float3 hsv = rgb2hsv(original.rgb);
                    hsv.y *= _Saturation;
                    float3 baseRGB = hsv2rgb(hsv);
                    
                    // 应用身体颜色
                    finalColor = lerp(baseRGB, baseRGB * _BodyColor.rgb, _BodyIntensity);
                }
                
                return fixed4(finalColor, original.a);
            }
            ENDCG
        }
    }
}