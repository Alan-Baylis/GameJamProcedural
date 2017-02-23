Shader "Custom/TriplanarTerrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Top Albedo (RGB)", 2D) = "white" {}
		_Tex2 ("Sides Albedo (RGB)", 2D) = "white" {}
		_Tex3 ("Bottom Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Top Normal", 2D) = "bump" {}
		_BumpMap2 ("Sides Normal", 2D) = "bump" {}
		_BumpMap3 ("Bottom Normal", 2D) = "bump" {}
		_BumpScale ("Bump intensity", Range(0,2)) = 1.0
		_NoiseMap ("Noise tex 3D", 3D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_TransitionIntensity ("Transition sharpness", Range(1,100)) = 1
		_NoiseIntensity ("Noise sharpness", Range(0,2)) = .2
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows

		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _Tex2;
		float4 _Tex2_ST;
		sampler2D _Tex3;
		float4 _Tex3_ST;
		sampler3D _NoiseMap;
		float4 _NoiseMap_ST;
		sampler2D _BumpMap;
		sampler2D _BumpMap2;
		sampler2D _BumpMap3;

		half _Glossiness;
		half _Metallic;
		half _TransitionIntensity;
		half _NoiseIntensity;
		fixed4 _Color;
		half _BumpScale;
        
        float sharpTransition(float value, float intensity)
        {
            if (value < .5)
                return pow(value*2, intensity)*.5;
            return 1-pow((1-value)*2, intensity)*.5;
        }

		struct Input {
            float3 worldPos;
			float3 viewDir;
            float3 worldNormal;
            INTERNAL_DATA
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 cTop = tex2D (_MainTex, IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zw);
			fixed4 cFront = tex2D (_Tex2, IN.worldPos.xy * _Tex2_ST.xy + _Tex2_ST.zw);
			fixed4 cSide = tex2D (_Tex2, IN.worldPos.zy * _Tex2_ST.xy + _Tex2_ST.zw);
			fixed4 cBottom = tex2D (_Tex3, IN.worldPos.xz * _Tex3_ST.xy + _Tex3_ST.zw);
            fixed3 noiseCoords = IN.worldPos*_NoiseMap_ST.x + _Time.x * _NoiseMap_ST.z;
            fixed noise = tex3D(_NoiseMap, noiseCoords).r;
            fixed noiseAdd = (noise*2)*_NoiseIntensity;
            
            float3 worldInterpolatedNormalVector = WorldNormalVector ( IN, float3( 0, 0, 1 ) );
			fixed3 nTop = UnpackNormal(tex2D (_BumpMap, IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zw));
			fixed3 nFront = UnpackNormal(tex2D (_BumpMap2, IN.worldPos.xy * _Tex2_ST.xy + _Tex2_ST.zw));
			fixed3 nSide = UnpackNormal(tex2D (_BumpMap2, IN.worldPos.zy * _Tex2_ST.xy + _Tex2_ST.zw));
			fixed3 nBottom = UnpackNormal(tex2D (_BumpMap3, IN.worldPos.xz * _Tex3_ST.xy + _Tex3_ST.zw));
            
            float dotToUp = dot(worldInterpolatedNormalVector, fixed3(0,1,0));
            float dotToRight = dot(worldInterpolatedNormalVector, fixed3(1,0,0));
            float topFactor = sharpTransition(clamp(1-dotToUp+noiseAdd, 0, 1), _TransitionIntensity);
            float bottomFactor = sharpTransition(clamp(-dotToUp+noiseAdd, 0, 1), _TransitionIntensity);
            float sidesFactor = sharpTransition(clamp(abs(dotToRight)+noiseAdd, 0, 1), _TransitionIntensity);
            
            fixed4 c = lerp(cFront, cSide, sidesFactor);
            c = lerp(cTop, c, topFactor);
            c = lerp(c, cBottom, bottomFactor);
            
            fixed3 n = lerp(nFront, nSide, sidesFactor);
            n = lerp(nTop, n, topFactor);
            n = lerp(n, nBottom, bottomFactor);
			
            o.Albedo = c.rgb * _Color.rgb;
            o.Normal = normalize(lerp(fixed3(0,0,1), n, _BumpScale));
			
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a * _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
