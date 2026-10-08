using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovimientoAleatorio : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private float intervaloCambio = 2f;
    private Rigidbody2D rb;
    private Vector2 direccion;
    private float tiempoRestante;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ElegirNuevaDireccion();
    }

    private void Update()
    {
        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            ElegirNuevaDireccion();
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direccion * velocidad;
    }

    private void ElegirNuevaDireccion()
    {
        direccion = Random.insideUnitCircle.normalized;
        tiempoRestante = intervaloCambio;
    }
}