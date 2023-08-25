Shader "SpaceSimulator/SpriteSystem20"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _FrameTex ("Texture", 2D) = "white" { }
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
                float2 colorUV : TEXCOORD0;
                float2 frameUV : TEXCOORD1;
                float frameZValue : TEXCOORD2;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 colorUV : TEXCOORD0;
                float2 frameUV : TEXCOORD1;
                float frameZValue : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _FrameTex;

            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.colorUV = IN.colorUV;
                OUT.frameUV = IN.frameUV;
                OUT.frameZValue = IN.frameZValue;
                
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float frame = tex2D(_FrameTex, IN.frameUV);
                frame = saturate((int)frame & (int)IN.frameZValue);
                frame = lerp(-1, 1, frame);
                clip(frame);

                return tex2D(_MainTex, IN.colorUV);
            }
            
            ENDCG
        }
    }
}
