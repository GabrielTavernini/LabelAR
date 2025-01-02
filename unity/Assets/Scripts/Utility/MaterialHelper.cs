using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public static class MaterialHelper
{

    public static void SetSolid(Material material)
    {
        Color tempColor = material.color;
        tempColor.a = 1;

        material.color = tempColor;
        SetToOpaque(material);
        material.shader = Shader.Find("Standard");
    }

    public static void SetTransparent(Material material, float alpha = 0.5f)
    {
        Color tempColor = material.color;
        tempColor.a = alpha;

        material.color = tempColor;
        SetToTransparent(material);
        material.shader = Shader.Find("Standard");
    }

    public static void SetShader(Material material, Shader shader)
    {
        Color color = material.color;
        material.shader = shader;
        material.color = color;
    }

    private static void SetToOpaque(Material material)
    {
        material.SetFloat("_Mode", 0); // 0 = Opaque, 1 = Cutout, 2 = Fade, 3 = Transparent
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1; // Use default rendering queue
    }

    private static void SetToTransparent(Material material)
    {
        material.SetFloat("_Mode", 3); // 0 = Opaque, 1 = Cutout, 2 = Fade, 3 = Transparent
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
}