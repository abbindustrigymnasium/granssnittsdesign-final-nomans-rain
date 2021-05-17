Shader "Custom/Sun Shader"
{
    Properties
    {
		_Spectrum ("Spectrum", 2D) = "white" {}
		_Emission("Emission", float) = 0
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.5

		sampler2D _Spectrum;
		float _Emission;
		float3 _EmissionColor;

        struct Input
        {
            float3 normal;
            float3 vertPos;
            float4 pos;
        };

        void vert (inout appdata_full v, out Input o)
        {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.normal = v.normal;
			o.vertPos = v.vertex;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			o.Emission = _EmissionColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
