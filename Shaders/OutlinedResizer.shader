
Shader "Custom/Test/OutlinedResizer"
{
	Properties{
		_Center ("Center", Vector) = (0,0,0)
		_Radius ("Radius", Float) = 0.5
		_Amount ("Extrusion Amount", Range(-0.1,0.1)) = 0

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
		float2 uv;
		float DN;
	};

	uniform float4 _FirstOutlineColor;
	uniform float _FirstOutlineWidth;

	uniform float4 _SecondOutlineColor;
	uniform float _SecondOutlineWidth;

	uniform float4 _Color;
	uniform float _Angle;

	float _Radius;
	float3 _Center;
	float _Amount;
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
				
				float3 wPos = mul(unity_ObjectToWorld, v.vertex);
				float d = distance( _Center, wPos);
				float dN = 1 - saturate(d / _Radius);
				dN = step(0.005, dN);
				v.vertex.xyz += v.normal * dN * _Amount;

				float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));
				
				if ( degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) {
					v.vertex.xyz += normalize(v.normal.xyz) * _FirstOutlineWidth;
				}else {
					v.vertex.xyz += scaleDir *_FirstOutlineWidth;
				}
				float3 modelPos = mul (float4(0,0,0,1), unity_ObjectToWorld).xyz;

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

			#pragma surface surf Lambert vertex:vert

			void vert (inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input,o);
				float3 wPos = mul(unity_ObjectToWorld, v.vertex);
				float d = distance( _Center, wPos);
				float dN = 1 - saturate(d / _Radius);
				dN = step(0.005, dN);

				v.vertex.xyz += v.normal * dN * _Amount;
				o.DN = dN;
			}

			void surf(Input IN, inout SurfaceOutput  o)
			{
				float dN = step(0.005, IN.DN);
				o.Albedo = float4(1, 0, 0, 1) * (1-dN) + float4(0,0,1,1) * dN;
			}

			ENDCG
	}
}
