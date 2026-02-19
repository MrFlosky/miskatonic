using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// This class is responsible for turning objects the character collides with to transparent.
/// </summary>
public class Ghostify : MonoBehaviour
{
    /// <summary>
    /// Target alpha value for transparent object.
    /// </summary>
    private float transparentAlpha = 0.1f;

    /// <summary>
    /// Speed at which object turns transparent.
    /// </summary>
    [SerializeField] private float fadeSpeed = 5f;

    /// <summary>
    /// Dict containing active coroutines for both turning objects transparent then fading them out and
    /// turning them opaque for fading them back in.
    /// </summary>
    private Dictionary<Renderer, Coroutine> fadeCoroutines = new Dictionary<Renderer, Coroutine>();

    /// <summary>
    /// Dict storing original shadow mode of a faded out object. We turn off shadows for transparent objects
    /// to prevent dotted artifacts that were occuring.
    /// </summary>
    private Dictionary<Renderer, UnityEngine.Rendering.ShadowCastingMode> originalShadowModes = new Dictionary<Renderer, UnityEngine.Rendering.ShadowCastingMode>();

    /// <summary>
    /// Upon objects entering this character's trigger, grab all the mesh renderers in its children
    /// and start coroutines on each to turn them transparent,
    /// </summary>
    void OnTriggerEnter(Collider other)
    {   
        // Track that a new character has entered this ghostify collider.
        // This component is added to all ghostify objects under the room prefabs,
        // where ghostify obs hold all models that should fade.
        CharacterCollTracker collTrack = other.GetComponent<CharacterCollTracker>();
        if(collTrack == null)
        {
            return;
        }
        
        // Check if we should fade BEFORE incrementing
        bool shouldFade = collTrack.CanGhostify();
        
        // ALWAYS increment the count when a character enters
        collTrack.AddCharacter();
        
        // Only start fading if this was the first character
        if(!shouldFade)
        {
            return;
        }
        

        // Get all MeshRenderers in the collider's object and its children
        MeshRenderer[] renderers = other.GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer renderer in renderers)
        {
            // Stop any existing fade coroutine for this renderer
            if (fadeCoroutines.ContainsKey(renderer))
            {
                StopCoroutine(fadeCoroutines[renderer]);
            }
            
            // Start fading to transparent
            fadeCoroutines[renderer] = StartCoroutine(FadeToTransparent(renderer));
        }
    }


    /// <summary>
    /// Upon objects exiting this character's trigger, grab all the mesh renderers in its children
    /// and start coroutines on each to turn them opaque,
    void OnTriggerExit(Collider other)
    {
        // Track that a character has exited this ghostify collider
        CharacterCollTracker collTrack = other.GetComponent<CharacterCollTracker>();
        if(collTrack == null) // skip if this not a ghostify collider
        {
            return;
        }
        
        // ALWAYS decrement the count when a character exits
        collTrack.RemoveCharacter();
        
        // Only fade back to opaque if no characters are left inside
        if(collTrack.HasCharacters())
        {
            return;
        }

        // Get all MeshRenderers in the collider's object and its children
        MeshRenderer[] renderers = other.GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer renderer in renderers)
        {
            // Stop any existing fade coroutine for this renderer
            if (fadeCoroutines.ContainsKey(renderer))
            {
                StopCoroutine(fadeCoroutines[renderer]);
            }
            
            // Start fading back to opaque
            fadeCoroutines[renderer] = StartCoroutine(FadeToOpaque(renderer));
        }
    }

    /// <summary>
    /// Given a mesh renderer, grab its materials and set them to transparent rendering mode to use their
    /// albedo's alpha to fade them out.
    /// </summary>
    IEnumerator FadeToTransparent(MeshRenderer renderer)
    {
        Material[] materials = renderer.materials;
        
        // Store original shadow mode
        if (!originalShadowModes.ContainsKey(renderer))
        {
            originalShadowModes[renderer] = renderer.shadowCastingMode;
        }
        
        // Disable shadow casting when transparent
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        
        // Set all materials to fade mode
        foreach (Material mat in materials)
        {
            mat.ToFadeMode();
        }
        
        // Store original colors
        Color[] originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].GetColor("_BaseColor");
        }
        
        // Fade to transparent
        float elapsed = 0f;
        while (elapsed < 1f) 
        {
            elapsed += Time.deltaTime * fadeSpeed;
            
            for (int i = 0; i < materials.Length; i++)
            {
                Color color = originalColors[i];
                color.a = Mathf.Lerp(originalColors[i].a, transparentAlpha, elapsed);
                materials[i].SetColor("_BaseColor", color);
            }
            
            yield return null;
        }
        
        fadeCoroutines.Remove(renderer);
    }

    /// <summary>
    /// Given a mesh renderer thats currently in transparent render mode, use the albedo alpha to fade them
    /// back in. Then set render mode back to opaque.
    /// </summary>
    IEnumerator FadeToOpaque(MeshRenderer renderer)
    {
        Material[] materials = renderer.materials;
        
        // Store current colors
        Color[] currentColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            currentColors[i] = materials[i].GetColor("_BaseColor");
        }
        
        // Fade to opaque
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * fadeSpeed;
            
            for (int i = 0; i < materials.Length; i++)
            {
                Color color = currentColors[i];
                color.a = Mathf.Lerp(currentColors[i].a, 1f, elapsed);
                materials[i].SetColor("_BaseColor", color);
            }
            
            yield return null;
        }
        
        // Restore original shadow casting mode
        if (originalShadowModes.ContainsKey(renderer))
        {
            renderer.shadowCastingMode = originalShadowModes[renderer];
            originalShadowModes.Remove(renderer);
        }
        
        // Set all materials back to opaque mode
        foreach (Material mat in materials)
        {
            mat.ToOpaqueMode();
        }
        
        fadeCoroutines.Remove(renderer);
    }
}

/// <summary>
/// Class for extension methods for materials, needed to set standard shader's rendering mode.
/// Found here: https://discussions.unity.com/t/change-rendering-mode-via-script/667727/6
/// </summary>
/// 
public static class MaterialExtensions
{
    public static void ToFadeMode(this Material material)
    {
        material.SetFloat("_Surface", 1f);          // Transparent
        material.SetFloat("_Blend", 0f);            // Alpha
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_BlendModePreserveSpecular", 0f);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.DisableKeyword("_ALPHAMODULATE_ON");

        material.renderQueue = (int)RenderQueue.Transparent;
        material.SetShaderPassEnabled("ShadowCaster", false);
    }

    public static void ToOpaqueMode(this Material material)
    {
        material.SetFloat("_Surface", 0f);          // Opaque
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_ZWrite", 1f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.One);
        material.SetFloat("_DstBlend", (float)BlendMode.Zero);
        material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        material.SetFloat("_DstBlendAlpha", (float)BlendMode.Zero);
        material.SetFloat("_BlendModePreserveSpecular", 1f);

        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.DisableKeyword("_ALPHAMODULATE_ON");

        material.renderQueue = (int)RenderQueue.Geometry;
        material.SetShaderPassEnabled("ShadowCaster", true);
    }
}