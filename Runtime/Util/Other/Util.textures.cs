using UnityEngine;

partial class Util
{
    public static void Clear(this RenderTexture renderTexture, in Color clearColor = default)
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, clearColor);
        RenderTexture.active = rt;
    }

    public static Texture2D GetTexture(this Camera camera, in int width, in int height)
    {
        RenderTexture temp = RenderTexture.GetTemporary(
            width,
            height,
            24,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB
            );

        RenderTexture cameraTarget = camera.targetTexture;
        camera.targetTexture = temp;
        camera.Render();
        camera.targetTexture = cameraTarget;

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = temp;

        Texture2D copy = new(width, height, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point,
        };
        copy.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
        copy.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temp);

        return copy;
    }
}