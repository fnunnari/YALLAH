Shader "Custom/blushShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MainTexBlush("Albedo_Blush (RGB)", 2D) = "white" {}
		_MainTexWhite("Albedo_Pale (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_Blend("Blend Amount", Range(-1.0,1.0)) = 0.0
		_modeBlush("BlushMode", Range(0,1)) = 1
	}
		SubShader{
		  Tags { "RenderType" = "Opaque" }
		  CGPROGRAM
		  #pragma surface surf Lambert
		  struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		  };

		fixed4 _Color;
		  sampler2D _MainTex;
		  sampler2D _MainTexBlush;
		  sampler2D _MainTexWhite;
		  sampler2D _BumpMap;
		  float _Blend;
		  
		  void surf(Input IN, inout SurfaceOutput o) {
			  float blushing = 0.0f;
			  float whitening = 0.0f;
			  fixed4 main = tex2D(_MainTex, IN.uv_MainTex);
			  fixed4 blush = tex2D(_MainTexBlush, IN.uv_MainTex);
			  fixed4 white = tex2D(_MainTexWhite, IN.uv_MainTex);
			  fixed4 col = lerp(main, blush, _Blend);
			  if (_Blend < 0) {
				  col = lerp(main, white, _Blend*-1);
			  }
			  if (_Blend > 0) {
				  col = lerp(main, blush, _Blend);
			  }

			o.Albedo = col.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		  }
		  ENDCG
		}
			Fallback "Diffuse"
}
