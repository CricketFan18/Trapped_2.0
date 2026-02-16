Shader "Custom/XORReveal"
{
    Properties
    {
        _MainTex ("Key Texture", 2D) = "white" {}
        _EncryptedTex ("Encrypted Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RevealSharpness ("Reveal Sharpness", Range(0.01, 0.5)) = 0.05
    }

    // ?????????????????????????????????????????????????????????????????
    //  URP SubShader
    // ?????????????????????????????????????????????????????????????????
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100" 
            "RenderPipeline"="UniversalPipeline" 
        }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_EncryptedTex);
            SAMPLER(sampler_EncryptedTex);
            
            float4 _MainTex_ST;
            float4 _EncryptedTex_ST;
            half4 _Color;
            float _RevealSharpness;

            float4 _UVLightPosition;
            float4 _UVLightDirection;
            float _UVLightAngle;
            float _UVLightEnabled;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Sample both textures using the same UVs
                half4 keyCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 encCol = SAMPLE_TEXTURE2D(_EncryptedTex, sampler_EncryptedTex, i.uv);
                
                // XOR decode: for binary B/W textures abs(a - b) = XOR
                half3 decoded = abs(encCol.rgb - keyCol.rgb);

                // UV light cone masking
                float3 lightToPixel = normalize(i.worldPos - _UVLightPosition.xyz);
                float dotProd = dot(lightToPixel, _UVLightDirection.xyz);
                float attenuation = smoothstep(_UVLightAngle - _RevealSharpness, _UVLightAngle + _RevealSharpness, dotProd);
                float dist = distance(i.worldPos, _UVLightPosition.xyz);
                float distFactor = saturate(1.0 - dist / 10.0);
                float lightResult = saturate(attenuation * distFactor * _UVLightEnabled);

                half4 result;
                result.rgb = decoded * _Color.rgb;
                result.a = lightResult;
                
                return result;
            }
            ENDHLSL
        }
    }

    // ?????????????????????????????????????????????????????????????????
    //  Built-in RP SubShader (fallback)
    // ?????????????????????????????????????????????????????????????????
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

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
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _EncryptedTex;
            float4 _EncryptedTex_ST;
            fixed4 _Color;
            float _RevealSharpness;

            float4 _UVLightPosition;
            float4 _UVLightDirection;
            float _UVLightAngle;
            float _UVLightEnabled;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample both textures using the same UVs
                fixed4 keyCol = tex2D(_MainTex, i.uv);
                fixed4 encCol = tex2D(_EncryptedTex, i.uv);
                
                // XOR decode: for binary B/W textures abs(a - b) = XOR
                // encrypted = secret XOR key
                // decoded   = encrypted XOR key = secret
                fixed3 decoded = abs(encCol.rgb - keyCol.rgb);

                // UV light cone masking
                float3 lightToPixel = normalize(i.worldPos.xyz - _UVLightPosition.xyz);
                float dotProd = dot(lightToPixel, _UVLightDirection.xyz);
                float attenuation = smoothstep(_UVLightAngle - _RevealSharpness, _UVLightAngle + _RevealSharpness, dotProd);
                float dist = distance(i.worldPos.xyz, _UVLightPosition.xyz);
                float distFactor = saturate(1.0 - dist / 10.0);
                float lightResult = saturate(attenuation * distFactor * _UVLightEnabled);

                fixed4 result;
                result.rgb = decoded * _Color.rgb;
                result.a = lightResult;
                
                return result;
            }
            ENDCG
        }
    }
}
