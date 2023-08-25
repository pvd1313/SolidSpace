Shader "Unlit/GridUnlit"
{
    Properties
    {
        [NoScaleOffset] _Albedo ("Albedo", 2D) = "white" { }
        _Size ("Size", float) = 1
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        LOD 100

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _Albedo;
            float _Size;

            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = mul(unity_ObjectToWorld, IN.vertex).xy / _Size;
                
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_Albedo, IN.uv);
                
                return col;
            }
            ENDCG
        }
    }
}
