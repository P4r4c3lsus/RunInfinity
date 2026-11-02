using UnityEngine;

namespace RunInfinity.Adicional
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class RIObstacle : MonoBehaviour
    {
        private RIGame juego;
        private Rigidbody2D cuerpo;

        public void Initialize(RIGame owner)
        {
            juego = owner;
            cuerpo = GetComponent<Rigidbody2D>();
            cuerpo.bodyType = RigidbodyType2D.Kinematic;
            cuerpo.gravityScale = 0;
            cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void FixedUpdate()
        {
            if (juego == null || !juego.IsPlaying) return;
            cuerpo.MovePosition(cuerpo.position + Vector2.left * juego.Speed * Time.fixedDeltaTime);
            if (cuerpo.position.x < juego.LeftEdge - 2f)
            {
                juego.RemoveObstacle(this);
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
}
