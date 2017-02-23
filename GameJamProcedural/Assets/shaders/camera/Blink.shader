Shader "Hidden/Blink" {
	Properties {
		_MainTex ("Base", 2D) = "" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	
	float4 _MainTex_TexelSize;
	half4 _MainTex_ST;
	half _Intensity;
	half _Desaturation;
		
	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		
		return o;
	} 
	
	half4 fragDs(v2f i) : SV_Target 
	{
		half4 c = tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy + _MainTex_TexelSize.xy * 0.5, _MainTex_ST));
		c += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy - _MainTex_TexelSize.xy * 0.5, _MainTex_ST));
		c += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy + _MainTex_TexelSize.xy * float2(0.5,-0.5), _MainTex_ST));
		c += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy - _MainTex_TexelSize.xy * float2(0.5,-0.5), _MainTex_ST));
		return c/4.0;
	}

	half4 frag(v2f i) : SV_Target 
	{
		half2 coords = i.uv;
		half2 uv = i.uv;
		
		coords = (coords - 0.5) * 2.0;		
		half coordDot = dot (coords,coords);
		
		half2 uvG = uv - _MainTex_TexelSize.xy * _Intensity * coords * coordDot;
		half4 color = tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(uv, _MainTex_ST));
        half desat = (color.r + color.g + color.b) / 3;
		#if SHADER_API_D3D9
			// Work around Cg's code generation bug for D3D9 pixel shaders :(
			color = color * 0.0001 + tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(uvG, _MainTex_ST));
		#else
			color = tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(uvG, _MainTex_ST));
		#endif
        color = lerp(color, desat, _Desaturation * _Intensity/100 * (1-coordDot));
		
		return color;
	}
    
	ENDCG 
	
Subshader {

 // 0: box downsample
 Pass {
	  ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      
      #pragma vertex vert
      #pragma fragment fragDs
      ENDCG
  }
// 1: distortion
Pass {
	  ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
}

Fallback off
	
} // shader
