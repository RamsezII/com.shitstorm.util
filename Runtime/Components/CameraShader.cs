using UnityEngine;

namespace _UTIL_
{
    public class CameraShader : MonoBehaviour
    {
        [SerializeField] Material[] shaders;
        [SerializeField] bool wireframe;

        bool current_wf;

        //--------------------------------------------------------------------------------------------------------------

        private void OnPreRender()
        {
            current_wf = GL.wireframe;
            GL.wireframe = wireframe;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            for (int i = 0; i < shaders.Length; i++)
                Graphics.Blit(source, destination, shaders[i]);
        }

        private void OnPostRender()
        {
            GL.wireframe = current_wf;
        }
    }
}