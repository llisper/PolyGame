Shader "PolyGame/PolyBackground" 
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha ("Alpha", Float) = 1.0
		_Color ("_Color", Color) = (1, 1, 1, 1)
		_Bounds ("_Bounds", Vector) = (0, 0, 0, 0)
    }

    SubShader 
    { 
        Pass {
            Tags { "Queue" = "Background" }
            Blend Off
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag   
            #pragma multi_compile __ _USE_CIRCLE_ALPHA _TEXTURE_BG
			#pragma multi_compile __ _GREYSCALE

            #include "UnityCG.cginc"

            float _Alpha;
			float3 _Color;
			float4 _Bounds;
            sampler2D _MainTex;

            struct vdata 
            {
                float4 position : POSITION;
                #if defined (_TEXTURE_BG)
                float2 uv : TEXCOORD0;
				#endif
            };

            struct fdata 
            {
                float4 position : SV_POSITION;
                #if defined (_TEXTURE_BG)
                float2 uv : TEXCOORD0;
				#else
				float2 worldPos : TEXCOORD0;
				float2 worldOrigin : TEXCOORD1;
				#endif
            };

			#if ! defined (_TEXTURE_BG)
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
			#endif

            fdata vert (vdata v) 
            {
                fdata i;
                i.position = UnityObjectToClipPos(v.position);
                #if defined (_TEXTURE_BG)
					i.uv = v.uv;
				#else
					i.worldPos = mul(unity_ObjectToWorld, v.position).xy;
					i.worldOrigin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xy;
				#endif
                return i;
            }

            float4 frag (fdata i) : SV_TARGET 
            {
				float4 final;
                #if defined (_TEXTURE_BG)
					final = float4(tex2D(_MainTex, i.uv).rgb, 1);

				#else
					#if defined (_USE_CIRCLE_ALPHA)
					float a = circleAlpha(i);
					#else
					float a = ellipseAlpha(i);
					#endif
					final = float4(lerp(float3(1, 1, 1), _Color, a), 1);
				#endif

                #if defined (_GREYSCALE)
					final.rgb = dot(final.rgb, float3(0.3, 0.59, 0.11));
                #endif

				return final;
            }
            ENDCG
        }
    }
}
