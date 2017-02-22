Shader "Custom/Raymarch Example"
{
	SubShader
	{
		Pass
	{
		Blend SrcAlpha Zero

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

	// New Code
	float3 _CamPos;
	float3 _CamRight;
	float3 _CamUp;
	float3 _CamForward;

		struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	v2f vert(appdata_base v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord;
		return o;
	}

	float distFunc(float3 pos)
	{
		const float sphereRadius = 1;
		return length(pos) - sphereRadius;
	}

	fixed4 renderSurface(float3 pos)
	{
		const float2 eps = float2(0.0, 0.01);

		float ambientIntensity = 0.1;
		float3 lightDir = float3(0, -0.5, 0.5);

		float3 normal = normalize(float3(
			distFunc(pos + eps.yxx) - distFunc(pos - eps.yxx),
			distFunc(pos + eps.xyx) - distFunc(pos - eps.xyx),
			distFunc(pos + eps.xxy) - distFunc(pos - eps.xxy)));

		float diffuse = ambientIntensity + max(dot(-lightDir, normal), 0);

		return fixed4(diffuse, diffuse, diffuse, 1);
	}


		
	fixed4 frag(v2f i) : SV_Target
	{
		float2 uv = i.uv - 0.5;
		float3 pos = _CamPos;
		float3 ray = _CamUp * uv.y + _CamRight * uv.x + _CamForward;

		/*float2 uv = i.uv - 0.5;
		float3 camUp = float3(0, 1, 0);
		float3 camForward = float3(0, 0, 1);
		float3 camRight = float3(1, 0, 0);

		float3 pos = float3(0, 0, -5);
		float3 ray = camUp * uv.y + camRight * uv.x + camForward;*/

		fixed4 color = 0;

		for (int i = 0; i < 30; i++)
		{
			float d = distFunc(pos);

			if (d < 0.01)
			{
				color = renderSurface(pos);
				break;
			}

			pos += ray * d;

			if (d > 40)
			{
				break;
			}
		}

		return color;
	}
		ENDCG
	}
	}
}
