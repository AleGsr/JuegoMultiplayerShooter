using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour

{
    public float lifetime = 5;
    public float speed = 5;
    public float damage = 35;


    //quien disparo este proyecto
    public PlayerController instigator;
    public Vector3 direction;

    //efecto de impacto
    public GameObject impactPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;

        //dfesaparecer el proyectil despues de cierto tiempo
        if ( lifetime < 0 )
        {
            //despawnear en las instancias conectadas
            GetComponent<NetworkObject>().Despawn(true);
        }

        if(IsServer)
        {
            //calcular el movimiento
            transform.position += direction * speed * Time.deltaTime;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsServer)
        {
            PlayerController pc = other.GetComponent<PlayerController>();

            if(pc != null) //instigator != pc
            {
                pc.TakeDamage((int)damage);
                //notificamos a los clientes para que hagan efectos visuales
                OnImpactRpc();
                GetComponent<NetworkObject>().Despawn(true);
            }

        }
    }

    //efectos de contacto
    [Rpc(SendTo.ClientsAndHost)]
    public void OnImpactRpc()
    {
        if (impactPrefab != null)
        {
            GameObject impact = Instantiate(impactPrefab, transform.position, Quaternion.identity);
            Destroy(impact, 3);
        }
    }

}
