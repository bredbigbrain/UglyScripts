// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Outline/PostEffect Outline"
{
	Properties
	{
		_MainTex("Main Texture",2D) = "white"{}
		_SceneTex("Main Texture",2D) = "white"{}
		_ObjectColor("Objects Color", Color) = (1,1,1,1)
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM

			sampler2D _MainTex;
			sampler2D _SceneTex;
			half4 _ObjectColor;
			half4 _OutlineColor;

			float2 _MainTex_TexelSize;

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uvs : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				//Also, we need to fix the UVs to match our screen space coordinates. There is a Unity define for this that should normally be used.
				o.uvs = o.pos.xy / 2 + 0.5;

				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 scene = tex2D(_SceneTex, float2(i.uvs.x, i.uvs.y));

				int NumberOfIterations = 9;

				//split texel size into smaller words
				float TX_x = _MainTex_TexelSize.x;
				float TX_y = _MainTex_TexelSize.y;

				float ColorIntensityInRadius = 0;
				
				if (tex2D(_MainTex,i.uvs.xy).r>0)
				{
					return scene * _ObjectColor;
				}
				else
				{
					for (int k = 0; k < NumberOfIterations; k += 1)
					{
						for (int j = 0; j < NumberOfIterations; j += 1)
						{
							ColorIntensityInRadius += tex2D(
								_MainTex,
								i.uvs.xy + float2(
								(k - NumberOfIterations / 2)*TX_x,
									(j - NumberOfIterations / 2)*TX_y)
							).r;
						}
					}

					scene = (1 - ColorIntensityInRadius) * scene;

					return ColorIntensityInRadius * _OutlineColor + scene;
				}
			}
			ENDCG
		}
	}
}