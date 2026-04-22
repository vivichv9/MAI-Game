using UnityEngine;

namespace MAIGame.Echo
{
    public sealed class EchoVisual : MonoBehaviour
    {
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private Color normalColor = new(0.25f, 0.8f, 1f, 0.35f);
        [SerializeField] private Color highlightedColor = new(1f, 0.95f, 0.35f, 0.55f);

        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<Renderer>();
            }

            SetHighlighted(false);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (targetRenderers == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            Color color = highlighted ? highlightedColor : normalColor;

            foreach (Renderer targetRenderer in targetRenderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", color);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}

