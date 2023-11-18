using System.Linq;
using UnityEngine;

namespace Canvas
{
    public class CanvasScript : MonoBehaviour
    {
        public Texture2D texture;
        public Vector2 textureSize = new Vector2(2048, 2048);
        private bool isWhite = false;

        void Start()
        {
            var r = GetComponent<Renderer>();
            texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
            r.material.mainTexture = texture;
            if (isWhite)
            {
                return;
            }
            texture.SetPixels(Enumerable.Repeat(Color.white, (int) textureSize.x * (int) textureSize.y).ToArray());
            texture.Apply();
            isWhite = true;
        }
    }
}
