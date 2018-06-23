Shader "PolyGame/PolyS" 
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
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
			#pragma shader_feature _USE_VERT_COLOR

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;

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
				float3 color : COLOR;
				#else
                float2 uv : TEXCOORD0;
				#endif
            };

            fdata vert (vdata v) 
            {
                fdata i;
                i.position = UnityObjectToClipPos(v.position);
				#if defined (_USE_VERT_COLOR)
				i.color = v.color.rgb;
				#else
                i.uv = v.uv;
				#endif
                return i;
            }

            float4 frag (fdata i) : SV_TARGET 
            {
				#if defined (_USE_VERT_COLOR)
                return float4(i.color * _Color.rgb, _Color.a);
				#else
                return float4(tex2D(_MainTex, i.uv).rgb * _Color.rgb, _Color.a);
				#endif
            }
            ENDCG
        }
    }
}
