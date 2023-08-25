Shader "SpaceSimulator/ParticleSystem20"
{
    Properties
    {
        
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {

                return fixed4 (1, 1, 1, 1);
            }
            
            ENDCG
        }
    }
}
