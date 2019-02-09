
Shader "Custom/Outline/OutlineExtruded"
{
	Properties{
		_Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex("Texture", 2D) = "white" {}

		_FirstOutlineColor("Outline color", Color) = (0,0,0,1)
		_FirstOutlineWidth("Outlines width", Range(0.0, 2.0)) = 1.1

		_Angle("Switch shader on angle", Range(0.0, 180.0)) = 85
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata{
		float4 vertex : POSITION;
		float4 normal : NORMAL;
	};

	struct v2f{
		float4 pos : SV_POSITION;
		UNITY_FOG_COORDS(1)
	};

	struct Input 
	{
		float2 uv_MainTex;
	};

	uniform float4 _Color;

	uniform float4 _FirstOutlineColor;
	uniform float _FirstOutlineWidth;

	uniform float _Angle;
	ENDCG

	SubShader
	{
		Tags {"Queue" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }

		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back
			CGPROGRAM

			#pragma multi_compile_fog
			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v){
				appdata original = v;

				float3 modelPos = mul (float4(0,0,0,1), unity_ObjectToWorld).xyz;
				float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
				
				if ( degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) {
					v.vertex.xyz += normalize(v.normal.xyz) * _FirstOutlineWidth;
				}else {
					v.vertex.xyz += scaleDir *_FirstOutlineWidth;
				}

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,UnityObjectToClipPos(modelPos));

				return o;
			}

			half4 frag(v2f i) : COLOR{
				UNITY_APPLY_FOG(i.fogCoord, _FirstOutlineColor);
				return _FirstOutlineColor;
			}

			ENDCG
		}

			CGPROGRAM
			#pragma surface surf Lambert noforwardadd

			sampler2D _MainTex;

			void surf(Input IN, inout SurfaceOutput  o)
			{
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;
			}

			ENDCG
	}
}
