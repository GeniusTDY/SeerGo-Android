Shader "Custom/MultiColorGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OriginalColor ("原始颜色", Color) = (0,1,0,1)
        _ColorTolerance ("颜色容差", Range(0,1)) = 0.3
        
 
        _GradientColor1 ("渐变颜色 1", Color) = (1,0,0,1)    // 红色
        _GradientColor2 ("渐变颜色 2", Color) = (0,1,0,1)    // 绿色
        _GradientColor3 ("渐变颜色 3", Color) = (0,0,1,1)    // 蓝色
        _UseThreeColors ("使用三色渐变", Float) = 1          // 开关：1=三色, 0=双色
        _GradientType ("渐变类型", Range(0,3)) = 0           // 0:无 1:水平 2:垂直 3:径向
        _GradientIntensity ("渐变强度", Range(0,1)) = 0.5
        _ColorBlend1 ("颜色1混合位置", Range(0,1)) = 0.3     // 第一个颜色过渡点
        _ColorBlend2 ("颜色2混合位置", Range(0,1)) = 0.7     // 第二个颜色过渡点
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GradientUVTransform;  // 图集UV→sprite局部UV的变换 (scaleX, scaleY, offsetX, offsetY)
            fixed4 _OriginalColor;
            float _ColorTolerance;
            
            // 多色渐变参数
            fixed4 _GradientColor1;
            fixed4 _GradientColor2;
            fixed4 _GradientColor3;
            float _UseThreeColors;
            float _GradientType;
            float _GradientIntensity;
            float _ColorBlend1;
            float _ColorBlend2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // 计算颜色相似度（针对您的绿色圆形）
            float ColorSimilarity(fixed3 color1, fixed3 color2)
            {
                float3 diff = color1 - color2;
                return 1.0 - length(diff);
            }

            // 三色渐变计算
            fixed4 CalculateMultiColorGradient(float2 uv)
            {
                if (_GradientIntensity <= 0.0 || _GradientType == 0) 
                    return fixed4(0,0,0,0);
                
                float gradientValue = 0.0;
                
                // 根据渐变类型计算渐变值
                if (_GradientType == 1) // 水平渐变
                {
                    gradientValue = uv.x;
                }
                else if (_GradientType == 2) // 垂直渐变
                {
                    gradientValue = uv.y;
                }
                else if (_GradientType == 3) // 径向渐变（适合圆形）
                {
                    float2 center = float2(0.5, 0.5);
                    gradientValue = distance(uv, center);
                    gradientValue = 1.0 - gradientValue; // 中心为1，边缘为0
                }
                
                fixed4 resultColor;
                
                if (_UseThreeColors > 0.5) // 三色渐变模式
                {
                    if (gradientValue <= _ColorBlend1)
                    {
                        // 颜色1到颜色2的渐变
                        float t = gradientValue / _ColorBlend1;
                        resultColor = lerp(_GradientColor1, _GradientColor2, t);
                    }
                    else if (gradientValue <= _ColorBlend2)
                    {
                        // 颜色2到颜色3的渐变
                        float t = (gradientValue - _ColorBlend1) / (_ColorBlend2 - _ColorBlend1);
                        resultColor = lerp(_GradientColor2, _GradientColor3, t);
                    }
                    else
                    {
                        // 颜色3
                        resultColor = _GradientColor3;
                    }
                }
                else // 双色渐变模式
                {
                    // 简单的双色渐变
                    resultColor = lerp(_GradientColor1, _GradientColor2, gradientValue);
                }
                
                return resultColor;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, i.uv);
                
                // 透明背景（棋盘格）直接返回，保持透明
                if (original.a < 0.1)
                    return original;

                // 计算与原始绿色圆形的相似度
                float similarity = ColorSimilarity(original.rgb, _OriginalColor.rgb);
                
                // 使用平滑过渡避免硬边界
                float replaceFactor = smoothstep(1.0 - _ColorTolerance, 1.0, similarity);
                
                if (replaceFactor > 0.0)
                {
                    // 从图集UV反算出sprite自身的局部UV(0~1)，保证渐变在不同帧间一致
                    float2 localUV = i.uv * _GradientUVTransform.xy + _GradientUVTransform.zw;
                    
                    // 计算多色渐变
                    fixed4 gradientColor = CalculateMultiColorGradient(localUV);
                    
                    // 根据渐变强度混合
                    fixed4 finalColor = lerp(original, gradientColor, _GradientIntensity * replaceFactor);
                    return finalColor;
                }
                
                // 非绿色区域保持原样
                return original;
            }
            ENDCG
        }
    }
}