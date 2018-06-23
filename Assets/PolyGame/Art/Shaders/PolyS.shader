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

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;

            struct vdata 
            {
                float4 position : POSITION;
				float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct fdata 
            {
                float4 position : SV_POSITION;
				float3 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            fdata vert (vdata v) 
            {
                fdata i;
                i.position = UnityObjectToClipPos(v.position);
				i.color = v.color.rgb;
                i.uv = v.uv;
                return i;
            }

            float4 frag (fdata i) : SV_TARGET 
            {
				float3 tex = tex2D(_MainTex, i.uv).rgb;
				float3 v = i.color;
                return float4(tex * v * _Color.rgb, _Color.a);
            }
            ENDCG
        }
    }
}
