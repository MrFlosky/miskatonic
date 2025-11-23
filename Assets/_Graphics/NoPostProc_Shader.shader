Shader "Hidden/AlphaComposite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
        // ZWrite Off fullscreen blit, depth buffer operations bypassed so this is useless for transparent objs to not block other transparent objects
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            

            fixed4 frag (v2f_img i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}