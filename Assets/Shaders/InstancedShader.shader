Shader "NBody/InstancedShader" 
{
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_EmissionMap("Emission Map", 2D) = "black" {}
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
	}

	SubShader{
	Tags{ "RenderType" = "Opaque" }
	LOD 600

	CGPROGRAM
	#pragma surface surf Standard addshadow
	#pragma multi_compile_instancing
	#pragma instancing_options procedural:setup

	sampler2D _MainTex;
	sampler2D _EmissionMap;

	struct Input {
		float2 uv_MainTex;
	};	

	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<float4> positionBuffer;
		StructuredBuffer<float4> colorBuffer;
	#endif

	void setup()
	{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		float4 position = positionBuffer[unity_InstanceID];
		float scale = position.w;

		float4 color = colorBuffer[unity_InstanceID];
		if (color.x == 0 && color.y == 0 && color.z == 0) scale = 0.01f;

		if (scale == 0) position.xyz = 1000;

		unity_ObjectToWorld._11_21_31_41 = float4(scale, 0, 0, 0);
		unity_ObjectToWorld._12_22_32_42 = float4(0, scale, 0, 0);
		unity_ObjectToWorld._13_23_33_43 = float4(0, 0, scale, 0);
		unity_ObjectToWorld._14_24_34_44 = float4(position.xyz, 1);
		unity_WorldToObject = unity_ObjectToWorld;
		unity_WorldToObject._14_24_34 *= -1;
		unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
#endif
	}

	half _Glossiness;
	half _Metallic;
	float4 _EmissionColor;

	void surf(Input IN, inout SurfaceOutputStandard o) 
	{
		float4 col = 1;

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		col = colorBuffer[unity_InstanceID];
#else
		col = float4(0, 0, 1, 1);
#endif

		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * col;
		o.Albedo = c.rgb;
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = c.a;
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * col.rgb;
#else
		o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * _EmissionColor.rgb;
#endif
	}
	ENDCG
	}
	FallBack "Diffuse"
}
