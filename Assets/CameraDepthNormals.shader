Shader "CustomCameraMode/CameraDepthNormals"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    CGINCLUDE
        
        #include "UnityCG.cginc"
        
        sampler2D _MainTex;
        float4 _MainTex_ST;
        
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
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }
        
    ENDCG
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {        
            Name "CameraDepthBlit"
            
            CGPROGRAM
            
                #pragma vertex vert
                #pragma fragment frag
    
                fixed4 frag (v2f i) : SV_Target
                {
                    float depth = tex2D(_MainTex, i.uv).r;
                    depth = Linear01Depth(depth);
                    return float4(depth, depth, depth, 1);
                }

            ENDCG
        }
        
        Pass
        {        
            Name "CameraNormalsBlit"
            
            CGPROGRAM
            
                #pragma vertex vert
                #pragma fragment frag
    
                fixed4 frag (v2f i) : SV_Target
                {
                    float depth;
                    float3 normal;
                    DecodeDepthNormal(tex2D(_MainTex, i.uv), depth, normal);
                    return float4(normal.r, normal.g, normal.b, 1);
                }

            ENDCG
        }
    }
}
