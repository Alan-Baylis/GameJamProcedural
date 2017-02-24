Shader "Hidden/rainEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			//Global variable 
			float _TimeOfDay;
			sampler2D _NoiseOffsets;
			float3 _CamPos;
			float3 _CamRight;
			float3 _CamUp;
			float3 _CamForward;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
            
			float2 drop(inout float2 p)
			{
				float2 mv = _TimeOfDay * float2(0.5, -1.0) * 0.15;

				float drh = 0.0;
				float hl = 0.0;

				float4 rnd = float4(0.1, 0.2, 0.3, 0.4);
				for (int i = 0; i < 10; i++)
				{
					rnd = frac(sin(rnd * 2.184972) * 190723.58961);
					float fd = frac(_TimeOfDay * 0.2 + rnd.w);
					fd = exp(fd * -4.0);
					float r = 0.020 * (rnd.w * 1.5 + 1.0);
					float sz = 0.35;


					float2 q = (frac((p - mv) * sz + rnd.xy) - 0.5) / sz;
					mv *= 1.06;
                    q.y *= -1;

					float l = length(q + pow(abs(dot(q, float2(1.0, 0.4))), 0.7) * (fd * 0.2 + 0.1));
					if (l < r)
					{
						float h = sqrt(r * r - l * l);
                        float hfd = h*fd;
                        drh = hfd; // ou max(drh, hfd);
					}
					hl += exp(length(q - float2(-0.02, 0.01)) * -30.0) * 0.4 * fd;
				}
				p += drh * 5.0;
				return float2(drh, hl);
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 pluie = drop(i.uv);
				fixed4 col = tex2D(_MainTex, i.uv);
				col += pluie.x;

				// Twelve layers of rain sheets...
				float2 q = i.uv;
				float dis = 1.;
				for (int i = 0; i < 5; i++)
				{
					float f =  pow(dis, .45) + .25;

					float2 st = f * 
						(q * float2(1.5, .05 
								//+ (1-_CamForward.z)*0.02 
								+ (abs(_CamForward.y))*0.02
							)
							+ float2(q.y*_CamForward.z*0.3,0) 
							+ float2(0, _TimeOfDay*0.2) 
							+ float2(0,  (1 - _CamForward.z)*0.02)
							);
					
					f = (tex2Dbias(_NoiseOffsets, float4(st.x*.5, st.y*.5, 0, 0)).x)*2.0;
					f = clamp(pow(abs(f)*.5, 29.0) * 140, 0.00, q.y*.4 + .05);
					half4 bri = half4(.25, .25, .25 , 1);
					col += bri*f;
					dis += 3.5;
				}


				return col;
			}
			ENDCG
		}
	}
}
