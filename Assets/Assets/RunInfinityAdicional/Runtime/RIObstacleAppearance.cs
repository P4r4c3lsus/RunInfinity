using UnityEngine;

namespace RunInfinity.Adicional
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class RIObstacleAppearance : MonoBehaviour
    {
        public Sprite[] variants;

        private void OnEnable()
        {
            if (variants == null || variants.Length == 0) return;
            GetComponent<SpriteRenderer>().sprite = variants[Random.Range(0, variants.Length)];
        }
    }
}
