Shader "SolidSpace/Gizmos"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "IgnoreProjector"="True" }

        Pass
        {
            ZWrite off
            ZTest off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                fixed4 color : COLOR;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            
            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);;
                OUT.color = IN.color;
                
                return OUT;
            }
            
            fixed4 frag (v2f IN) : SV_Target
            {
                return IN.color;
            }
            
            ENDCG
        }
    }
}