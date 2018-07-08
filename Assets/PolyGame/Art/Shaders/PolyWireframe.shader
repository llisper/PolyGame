Shader "PolyGame/PolyWireframe" 
{
    Properties 
    {
		[PerRendererData]
        _Alpha ("Alpha", Float) = 0.0
    }

    SubShader 
    { 
        Pass {
            Tags { "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag   

            #include "UnityCG.cginc"

            float _Alpha;

            struct vdata 
            {
                float4 position : POSITION;
                float4 color : COLOR;
            };

            struct fdata 
            {
                float4 position : SV_POSITION;
                float3 color : COLOR;
            };

            fdata vert (vdata v) 
            {
                fdata i;
                i.position = UnityObjectToClipPos(v.position);
                i.color = v.color.rgb;
                return i;
            }

            float4 frag (fdata i) : SV_TARGET 
            {
				return float4(i.color, _Alpha);
            }
            ENDCG
        }
    }
}
