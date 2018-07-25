Shader "PolyGame/SnapshotImage" 
{
    Properties 
    {
        [HideInInspector]
        _MainTex ("Texture", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
    }

    SubShader 
    { 
        Pass {
            Tags { "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            // ZWrite On 
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag   

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Mask;

            struct vdata 
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct fdata 
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fdata vert (vdata v) 
            {
                fdata i;
                i.position = UnityObjectToClipPos(v.position);
                i.uv = v.uv;
                return i;
            }

            float4 frag (fdata i) : SV_TARGET 
            {
                float3 final = tex2D(_MainTex, i.uv).rgb;
                float alpha = tex2D(_Mask, i.uv).a;
				return float4(final, alpha);
            }
            ENDCG
        }
    }
}
