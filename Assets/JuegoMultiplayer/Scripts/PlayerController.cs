using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    Camera playerCamera;

    [Header("Weapon")]
    public GameObject ProjectilPrefab;
    //punto donde aparece o se spawnea el proyectil
    public Transform WeaponSocket;
    public float WeaponCadence = 2;
    float lastShoutTimer = 0; //Vuelve a esperar el tiempo para volver a disparar
    public bool FullAuto;


    [Header("Camera vars")]
    public Vector3 CameraOffset = new (0,2.5f,-2);

    //La salud es replicada
    //int Health = 100;
    //Primer parametro: valor inicial, permisos
    //Esta es una variable que puede leer cualquiera pero solo puede establecer (set) el servidor
    NetworkVariable<int> Health = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Sounds an FX")]
    public AudioClip DeathSound;
    public AudioClip DamageSound;
    public AudioClip AttackSound;

    //[Header("HUD")]
    //public RectTransform HUD;
    //public TMPro.TextMeshPro labelHealth;

    void Start()
    {
        playerCamera = Camera.main;

        if(IsOwner)
        {
            //HUD = GameObject.Find
            //HUD.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            //solo debe moverse si esta vivo
            if(IsAlive())
            {
                if (Input.GetKey(KeyCode.W))
                {
                    transform.position += new Vector3(0, 0, 1 * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    transform.position += new Vector3(0, 0, -1 * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    transform.position += new Vector3(-1 * Time.deltaTime, 0, 0);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    transform.position += new Vector3(1 * Time.deltaTime, 0, 0);
                }

                //disparo
                if(FullAuto)
                {
                    if (Input.GetButton("Fire1"))
                    {
                        FireWeaponRpc();
                    }
                }
                else
                {
                    if (Input.GetButtonDown("Fire1"))
                    {
                        FireWeaponRpc();
                    }
                }



            }
      
            //Solo modificamos la cámara si el jugador es el owner
            playerCamera.transform.position = transform.position + CameraOffset;
            playerCamera.transform.LookAt(transform.position);

            //Label con la salud
            //labelHealth.text = "" + Health.Value;

        }

        if (IsServer)
        {
            lastShoutTimer += Time.deltaTime;
        }

    }

    bool insideDamageVolume;
    float insideDVCounter = 0;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collision con " + other.name);
        DamageVolume dv = other.GetComponent<DamageVolume>();
        if(dv != null)
        {
            TakeDamage(dv.damagePerSec);
            insideDamageVolume = true;
            insideDVCounter = 0;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        DamageVolume dv = other.GetComponent<DamageVolume>();
        if (dv != null)
        {
            insideDVCounter += Time.deltaTime;
            if(insideDVCounter > 1) //si ya paso un segundo
            {
                insideDVCounter = 0;
                TakeDamage(dv.damagePerSec);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("Salgo de " + other.name);   
        DamageVolume dv = other.GetComponent<DamageVolume>();
        if(dv != null)
        {
            insideDamageVolume = false;
        }
    }

    //disparar el arma
    [Rpc(SendTo.Server)]
    public void FireWeaponRpc()
    {
        //Si no ha poasado el tiempo para volver a disparar, retornar
        if (lastShoutTimer < (60/WeaponCadence)) return;


        //instanciar un proyectil y dispararlo
        if(ProjectilPrefab != null)
        {
            GameObject go = Instantiate(ProjectilPrefab, WeaponSocket.position, WeaponSocket.rotation);
            Projectile proj = go.GetComponent<Projectile>();
            proj.direction = transform.forward;
            proj.instigator = this;

            //notificar la aparicion a los clientes
            proj.GetComponent<NetworkObject>().Spawn();
            lastShoutTimer = 0;

        }
    }


    //Llamada a procedimiento remoto para que el servidor calcule la nueva salud en base al danno
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int amount)
    {
        Debug.Log("TakeDamage en servidor");
        TakeDamage(amount);
    }

    public void TakeDamage(int damage)
    {
        if(IsAlive())
        {
            //Si no es llamada en un servidor, noificarlo
            if (!IsServer)
            {
                TakeDamageRpc(damage);
                return;
            }

            Health.Value -= damage;
            if (Health.Value <= 0)
            {
                //Pos me muero
                OnDeath();
            }
            Debug.Log("health: " + Health);
        }
        
    }

    //para reproducir efectos cuando muere el personaje

    void OnDeath()
    {
        //particle.play
        //sound.play
        Debug.Log("Me muero XD");
        GetComponent<AudioSource>(). clip = DeathSound;
        GetComponent<AudioSource>(). Play();
    }

    public bool IsAlive()
    {
        return Health.Value > 0;
    }
    public bool IsDead() 
    { 
        return !IsAlive(); 
    }

}
