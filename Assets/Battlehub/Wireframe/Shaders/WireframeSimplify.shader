// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Battlehub/WireframeSimplify"
{
	Properties
	{
		_Color("Color", Color) = (0.2, 0.2, 0.2, 1)
		_Alpha("Alpha", Float) = 1.0
		_Thickness("Thickness", Float) = 1.0
		[HideInInspector]_ZTest("__zt", Float) = 0.0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Cull Back
			ZTest[_ZTest]
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 color : COLOR;
				float4 baryc : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;
			float _Alpha;
			float _Thickness;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.baryc = v.color;
				o.color = _Color;
				o.color.a = v.color.a;
				return o;
			}

			float edgeFactor(float3 barycentric) {
				float3 d = fwidth(barycentric);
				float3 a3 = smoothstep(float3(0, 0, 0), d * _Thickness, barycentric);
				return min(min(a3.x, a3.y), a3.z);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 color = i.color;
				color.a = lerp(1.0 - edgeFactor(i.baryc), 0, color.a) * _Alpha;
				return color;
			}
			ENDCG
		}
	}
	CustomEditor "BattlehubWireframeEditor"
}
