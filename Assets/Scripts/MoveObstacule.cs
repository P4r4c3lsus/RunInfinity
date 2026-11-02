using UnityEngine;

public class MoveObstacule : MonoBehaviour
{
    [SerializeField] private float velocidad = 4f;
    [SerializeField] private float limiteIzquierdo = -12f;

    private void Update()
    {
        transform.position += Vector3.left * velocidad * Time.deltaTime;

        if (transform.position.x < limiteIzquierdo)
        {
            Destroy(gameObject);
        }
    }
}
