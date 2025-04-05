Shader "Sprites/HighlightSprite_Replacement"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
        _InsideColorOverlay("Inside Color Overlay", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA


            #include "UnityCG.cginc"

            #ifdef UNITY_INSTANCING_ENABLED

                UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
                    // SpriteRenderer.Color while Non-Batched/Instanced.
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
                    // this could be smaller but that's how bit each entry is regardless of type
                    UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

                #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
                #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

            #endif // instancing

            CBUFFER_START(UnityPerDrawSprite)
            #ifndef UNITY_INSTANCING_ENABLED
                fixed4 _RendererColor;
                fixed2 _Flip;
            #endif
                float _EnableExternalAlpha;
            CBUFFER_END

            // Material Color.
            fixed4 _Color;
            fixed4 _OutlineColor;
            fixed4 _InsideColorOverlay;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
            {
                return float4(pos.xy * flip, pos.z, 1.0);
            }

            v2f SpriteVert(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            sampler2D _AlphaTex;

            fixed4 SampleSpriteTexture (float2 uv)
            {
                fixed4 color = tex2D (_MainTex, uv);

            #if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D (_AlphaTex, uv);
                color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
            #endif

                return color;
            }

            fixed4 SpriteFrag(v2f IN) : SV_Target
            {
                // one pixel outer sharp line
                fixed4 sprite = SampleSpriteTexture(IN.texcoord) * IN.color;
                sprite.rgb *= sprite.a;

                fixed leftPixel = SampleSpriteTexture(IN.texcoord + float2(-_MainTex_TexelSize.x, 0)).a;
				fixed upPixel = SampleSpriteTexture(IN.texcoord + float2(0, _MainTex_TexelSize.y)).a;
				fixed rightPixel = SampleSpriteTexture(IN.texcoord + float2(_MainTex_TexelSize.x, 0)).a;
				fixed bottomPixel = SampleSpriteTexture(IN.texcoord + float2(0, -_MainTex_TexelSize.y)).a; 
                float m = max(max(leftPixel, upPixel), max(rightPixel, bottomPixel));

                // 2 pixel outline
                fixed leftPixel2 = SampleSpriteTexture(IN.texcoord + float2(2* -_MainTex_TexelSize.x, 0)).a;
				fixed upPixel2 = SampleSpriteTexture(IN.texcoord + float2(0, 2* _MainTex_TexelSize.y)).a;
				fixed rightPixel2 = SampleSpriteTexture(IN.texcoord + float2(2* _MainTex_TexelSize.x, 0)).a;
				fixed bottomPixel2 = SampleSpriteTexture(IN.texcoord + float2(0, 2* -_MainTex_TexelSize.y)).a; 
                m = max(m, max(max(leftPixel2, upPixel2), max(rightPixel2, bottomPixel2)));

				fixed outline = sprite.a < 0.3 && m > 0.3 ? m : 0; 
                
                float4 outlineColor = _OutlineColor;
                outlineColor.rgb *= outlineColor.a;
                
                sprite.rgb = lerp(sprite.rgb,  _InsideColorOverlay * (sprite.a > 0.1 ? 1 : 0), _InsideColorOverlay.a);

                return lerp(sprite, outlineColor, outline);

                // one pixel inner lerp smooth line
				// fixed leftPixel = SampleSpriteTexture(IN.texcoord + float2(-_MainTex_TexelSize.x, 0)).a;
				// fixed upPixel = SampleSpriteTexture(IN.texcoord + float2(0, _MainTex_TexelSize.y)).a;
				// fixed rightPixel = SampleSpriteTexture(IN.texcoord + float2(_MainTex_TexelSize.x, 0)).a;
				// fixed bottomPixel = SampleSpriteTexture(IN.texcoord + float2(0, -_MainTex_TexelSize.y)).a; 
				// fixed outline = (1 - leftPixel * upPixel * rightPixel * bottomPixel) * c.a; 
                // fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;               
                // c = lerp(c, _OutlineColor, outline);
                // return c;
    
                // original
                // fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                // c.rgb *= c.a;
                // return c;
            }


            ENDCG
        }
    }
}
