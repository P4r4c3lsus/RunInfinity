using UnityEngine;

namespace RunInfinity.Adicional
{
    // Animates only the visual child; the existing Rigidbody and collider stay unchanged.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class RICharacterAnimation : MonoBehaviour
    {
        public RIPlayer player;
        public RIGame game;
        private Animator animator;
        private SpriteRenderer visual, original;
        private Rigidbody2D body;
        private Collider2D colliderBody;
        private string current;
        private bool wasGrounded;
        private float landingRemaining;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            visual = GetComponent<SpriteRenderer>();
            if (player == null) player = GetComponentInParent<RIPlayer>();
            if (game == null) game = FindAnyObjectByType<RIGame>();
            if (player == null) { enabled = false; return; }
            body = player.GetComponent<Rigidbody2D>();
            colliderBody = player.GetComponent<Collider2D>();
            original = player.GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (game == null || body == null || colliderBody == null) return;
            if (game.State == RIGame.Estado.Pausa || game.State == RIGame.Estado.Derrota)
            { animator.speed = 0; return; }
            animator.speed = 1;
            if (!game.IsPlaying)
            {
                landingRemaining = 0;
                wasGrounded = false;
                Play("Idle");
                return;
            }
            bool grounded = player.suelo != null && colliderBody.IsTouching(player.suelo)
                && body.linearVelocity.y <= 0.05f;
            if (grounded && !wasGrounded) landingRemaining = 0.09f;
            wasGrounded = grounded;
            if (!grounded)
            {
                landingRemaining = 0;
                Play(body.linearVelocity.y > 0.05f ? "Jump" : "Fall");
            }
            else if (landingRemaining > 0)
            {
                landingRemaining -= Time.deltaTime;
                Play("Land");
            }
            else Play("Run");
        }

        private void LateUpdate()
        {
            // RIPlayer owns the invulnerability flash, including its alpha.
            if (original != null) visual.color = original.color;
        }

        private void Play(string state)
        {
            if (current == state) return;
            current = state;
            animator.Play(state, 0, 0);
        }
    }
}
