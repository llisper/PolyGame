﻿Shader "PolyGame/PolyS" 
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Highlight ("Highlight", Color) = (0, 0, 0, 0)
        _ZWrite ("ZWrite", Float) = 0.0
    }

    SubShader 
    { 
        Pass {
            Tags { "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag   
            #pragma multi_compile __ _USE_VERT_COLOR
            #pragma multi_compile __ _GREYSCALE

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float4 _Highlight;

            struct vdata 
            {
                float4 position : POSITION;
                #if defined (_USE_VERT_COLOR)
                float4 color : COLOR;
                #else
                float2 uv : TEXCOORD0;
                #endif
            };

            struct fdata 
            {
                float4 position : SV_POSITION;
                #if defined (_USE_VERT_COLOR)
                float4 color : COLOR;
                #else
                float2 uv : TEXCOORD0;
                #endif
            };

            fdata vert (vdata v) 
            {
                fdata i;
                i.position = UnityObjectToClipPos(v.position);
                #if defined (_USE_VERT_COLOR)
                i.color = v.color;
                #else
                i.uv = v.uv;
                #endif
                return i;
            }

            float4 frag (fdata i) : SV_TARGET 
            {
                #if defined (_USE_VERT_COLOR)
                float4 c = float4(i.color.rgb * _Color.rgb + _Highlight.rgb, i.color.a * _Color.a);
                #else
				float4 texcol = tex2D(_MainTex, i.uv);
                float4 c = float4(texcol.rgb * _Color.rgb + _Highlight.rgb, texcol.a * _Color.a);
                #endif
    
                #if defined (_GREYSCALE)
                c.rgb = dot(c.rgb, float3(0.3, 0.59, 0.11));
                #endif
                return c;
            }
            ENDCG
        }
    }
}
