using UnityEngine;
using UnityEngine.InputSystem;

public class JumpRunner : MonoBehaviour
{
    [SerializeField] private float velocidadSalto = 10f;
    [SerializeField] private Collider2D suelo;

    private Rigidbody2D cuerpo;
    private Collider2D miCollider;
    private bool saltoSolicitado;

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        miCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            saltoSolicitado = true;
        }
    }

    private void FixedUpdate()
    {
        if (!saltoSolicitado)
            return;

        saltoSolicitado = false;

        if (suelo == null)
            return;

        bool tocaSuelo = miCollider.IsTouching(suelo);

        if (tocaSuelo && cuerpo.linearVelocity.y <= 0.01f)
        {
            cuerpo.linearVelocity = new Vector2(
                cuerpo.linearVelocity.x,
                velocidadSalto
            );
        }
    }
}
