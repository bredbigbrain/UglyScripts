Shader "Custom/DrawSimple"
{
	SubShader
	{
		ZWrite Off
		ZTest Always
		Lighting Off
		Pass
		{
			CGPROGRAM
			#pragma vertex VShader
			#pragma fragment FShader

			struct VertexToFragment
			{
				float4 pos:POSITION;
			};

			VertexToFragment VShader(VertexToFragment i)
			{
				VertexToFragment o;
				o.pos = UnityObjectToClipPos(i.pos);
				return o;
			}

			half4 FShader() :COLOR
			{
				return half4(1,0,0,1);
			}

			ENDCG
		}
	}
}