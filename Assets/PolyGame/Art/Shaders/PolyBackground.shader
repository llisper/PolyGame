Shader "PolyGame/PolyBackground" 
{
    Properties 
    {
        _Alpha ("Alpha", Float) = 1.0
		[PerRendererData]
		_Color ("_Color", Color) = (1, 1, 1, 1)
		[PerRendererData]
		_Bounds ("_Bounds", Vector) = (0, 0, 0, 0)
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
			float3 _Color;
			float4 _Bounds;

            struct vdata 
            {
                float4 position : POSITION;
            };

            struct fdata 
            {
                float4 position : SV_POSITION;
				float2 worldPos : TEXCOORD0;
				float2 worldOrigin : TEXCOORD1;
            };

            float circleAlpha(fdata i)
            {
                float r = max(_Bounds.x, _Bounds.y);
                float2 v = i.worldPos - i.worldOrigin;
                return min(1.0, sqrt(dot(v, v)) / r) * _Alpha;
            } 

            float ellipseAlpha(fdata i)
            {
                float2 v = i.worldPos - i.worldOrigin;
                float k = v.y / v.x;
                float k2 = k * k;
                float a2 = _Bounds.x * _Bounds.x;
                float b2 = _Bounds.y * _Bounds.y;

				// find points cross with ellipse by vector v
				// v: y = kx
				// ellipse: x2 / a2 + y2 / b2 = 1 (a == extents.x, b == extents.y)
                float x = sqrt((a2 * b2) / (b2 + a2 * k2));
                float2 v1 = float2(x, k * x);
                float2 v2 = float2(-x, k * (-x));
                // if (dot(v, v1) >= 0) return v1 else return v2
                float2 vv = lerp(v2, v1, saturate(sign(dot(v, v1)) + 1.0));
                return min(1.0, sqrt(dot(v, v) / dot(vv, vv))) * _Alpha;
            }

            fdata vert (vdata v) 
            {
                fdata i;
                i.position = UnityObjectToClipPos(v.position);
				i.worldPos = mul(unity_ObjectToWorld, v.position).xy;
                i.worldOrigin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xy;
                return i;
            }

            float4 frag (fdata i) : SV_TARGET 
            {
                // float a = circleAlpha(i);
                float a = ellipseAlpha(i);
				return float4(_Color, a);
            }
            ENDCG
        }
    }
}
