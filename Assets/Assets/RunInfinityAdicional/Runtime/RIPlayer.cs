using UnityEngine;

namespace RunInfinity.Adicional
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class RIPlayer : MonoBehaviour
    {
        public float velocidadSalto = 10f;
        public Collider2D suelo;
        private Rigidbody2D cuerpo;
        private Collider2D caja;
        private Vector3 inicio;
        private bool solicitarSalto;
        private RIGame juego;
        private SpriteRenderer dibujo;
        private Color colorOriginal;

        public void Initialize(RIGame owner)
        {
            juego = owner;
            dibujo = GetComponent<SpriteRenderer>();
            if (dibujo != null) colorOriginal = dibujo.color;
            cuerpo = GetComponent<Rigidbody2D>();
            caja = GetComponent<Collider2D>();
            inicio = transform.position;
            cuerpo.bodyType = RigidbodyType2D.Dynamic;
            cuerpo.gravityScale = 3;
            cuerpo.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            caja.isTrigger = false;
            ResetPlayer(false);
        }

        public void ResetPlayer(bool simulate)
        {
            solicitarSalto = false;
            if (dibujo != null) dibujo.color = colorOriginal;
            cuerpo.simulated = false;
            transform.position = inicio;
            cuerpo.position = inicio;
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0;
            cuerpo.rotation = 0;
            cuerpo.simulated = simulate;
        }

        public void StopPlayer()
        {
            solicitarSalto = false;
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.simulated = false;
        }

        public void RequestJump()
        {
            if (juego != null && juego.IsPlaying) solicitarSalto = true;
        }

        private void FixedUpdate()
        {
            if (!solicitarSalto) return;
            solicitarSalto = false;
            if (!juego.IsPlaying || suelo == null) return;
            if (caja.IsTouching(suelo) && cuerpo.linearVelocity.y <= 0.05f)
            {
                cuerpo.linearVelocity = new Vector2(0, velocidadSalto);
                juego.Audio.PlayJump();
            }
        }

        private void Update()
        {
            if (dibujo == null || juego == null) return;
            Color color = colorOriginal;
            if (juego.IsInvulnerable && juego.State != RIGame.Estado.Menu)
                color.a *= Mathf.PingPong(Time.time * 8, 1) < 0.5f ? 0.35f : 1f;
            dibujo.color = color;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (juego == null || !juego.IsPlaying) return;
            RIObstacle obstacle = other.GetComponentInParent<RIObstacle>();
            if (obstacle != null && obstacle.gameObject.activeSelf) juego.TakeHit(obstacle);
        }
    }
}
