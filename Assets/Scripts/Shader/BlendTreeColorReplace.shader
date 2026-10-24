Shader "Custom/BlendTreeColorReplace"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NewColor ("New Color", Color) = (1,0,0,1)
        _OriginalColor ("Original Color", Color) = (0,1,0,1)
        _ColorTolerance ("Color Tolerance", Range(0,1)) = 0.3
        _Saturation ("Saturation", Range(0,2)) = 1.0
        _Brightness ("Brightness", Range(0,2)) = 1.0
        
        [Header(Gradient Settings)]
        _GradientColor1 ("Gradient Color 1", Color) = (1,1,1,1)
        _GradientColor2 ("Gradient Color 2", Color) = (0,0,0,1)
        _GradientType ("Gradient Type", Range(0,3)) = 0 // 0:None, 1:Horizontal, 2:Vertical, 3:Radial
        _GradientIntensity ("Gradient Intensity", Range(0,1)) = 0.5
        _GradientSpeed ("Gradient Speed", Range(0,2)) = 0.0
    }
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
                float2 worldPos : TEXCOORD1; // 用于径向渐变
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _NewColor;
            fixed4 _OriginalColor;
            float _ColorTolerance;
            float _Saturation;
            float _Brightness;
            
            // 渐变参数
            fixed4 _GradientColor1;
            fixed4 _GradientColor2;
            float _GradientType;
            float _GradientIntensity;
            float _GradientSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy; // 世界坐标用于渐变
                return o;
            }

            // 计算亮度
            float GetLuminance(fixed3 color)
            {
                return 0.299 * color.r + 0.587 * color.g + 0.114 * color.b;
            }

            // 调整饱和度
            fixed3 ApplySaturation(fixed3 color, float saturation)
            {
                float luminance = GetLuminance(color);
                return lerp(fixed3(luminance, luminance, luminance), color, saturation);
            }

            // 应用渐变效果
            fixed4 ApplyGradient(fixed4 baseColor, float2 uv, float2 worldPos)
            {
                if (_GradientIntensity <= 0.0 || _GradientType == 0) 
                    return baseColor;
                
                float gradientValue = 0.0;
                float timeFactor = _Time.y * _GradientSpeed;
                
                // 根据渐变类型计算渐变值
                if (_GradientType == 1) // 水平渐变
                {
                    gradientValue = uv.x + timeFactor;
                }
                else if (_GradientType == 2) // 垂直渐变
                {
                    gradientValue = uv.y + timeFactor;
                }
                else if (_GradientType == 3) // 径向渐变
                {
                    float2 center = float2(0.5, 0.5);
                    gradientValue = distance(uv, center) + timeFactor;
                }
                
                gradientValue = frac(gradientValue); // 确保在0-1范围内
                fixed4 gradientColor = lerp(_GradientColor1, _GradientColor2, gradientValue);
                
                // 混合原始颜色和渐变颜色
                return lerp(baseColor, gradientColor, _GradientIntensity);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, i.uv);
                
                if (original.a > 0.1) // 非透明像素
                {
                    // 计算颜色相似度
                    float colorDistance = distance(original.rgb, _OriginalColor.rgb);
                    
                    // 如果颜色接近目标颜色，进行替换
                    if (colorDistance < _ColorTolerance)
                    {
                        fixed3 newColor = _NewColor.rgb;
                        
                        // 应用亮度调整
                        newColor *= _Brightness;
                        
                        // 应用饱和度调整
                        newColor = ApplySaturation(newColor, _Saturation);
                        
                        fixed4 result = fixed4(newColor, original.a);
                        
                        // 应用渐变效果
                        result = ApplyGradient(result, i.uv, i.worldPos);
                        
                        return result;
                    }
                    else
                    {
                        // 对未替换的颜色也应用渐变效果（可选）
                        return ApplyGradient(original, i.uv, i.worldPos);
                    }
                }
                
                // 透明背景保持原样
                return original;
            }
            ENDCG
        }
    }
}
