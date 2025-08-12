using System.Globalization;
using UnityEngine;
using Unity.Netcode;
using TMPro;

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
    //id del nombre que escogio el jugador
    NetworkVariable<int> nameId = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    //id del accesorio de la cabeza
    NetworkVariable<int> hatId = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    [Header("Sounds an FX")]
    public AudioClip DeathSound;
    public AudioClip DamageSound;
    public AudioClip AttackSound;

    [Header("HUD")]
    public MenuManager menuManager;

    private TMP_Text playerLabel;


    Animator animator;
    Vector3 desiredDirection;
    public float speed = 2;


    void Start()
    {
        playerCamera = Camera.main;
        menuManager = GameObject.Find("GameManager").GetComponent<MenuManager>();
        animator = GetComponent<Animator>();

        if (IsOwner)
        {
            //HUD = GameObject.Find("")
            menuManager.HUD.gameObject.SetActive(true);

            //establecer nombre y accesorios seleccionados
            SetNameIDRpc(menuManager.selectedNameIndex);
        }

        if(IsClient)
        {
            playerLabel = Instantiate(menuManager.templatePlayerlabel, menuManager.HUD).GetComponent<TMP_Text>();
            playerLabel.gameObject.SetActive(true);

        }


    }


    //establecer nombre, notificarle al servidor
    [Rpc(SendTo.Server)]
    public void SetNameIDRpc(int idx)
    {
        nameId.Value = idx;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            desiredDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            desiredDirection.Normalize();

            //solo debe moverse si esta vivo
            if (IsAlive())
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

                float mag = desiredDirection.magnitude;
                //en el animator debe existir un parametro isWalking para activar la animacion de movimiento
                animator.SetBool("isWalking", mag > 0);
                if (mag > 0)
                {
                    //interpolar entre la rotacion actual y la deseada
                    Quaternion q = Quaternion.LookRotation(desiredDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 10);
                    //hay que declarar public float speed=2 mas arriba
                    transform.Translate(0, 0, speed * Time.deltaTime);

                }


                //disparo
                if (FullAuto)
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
            menuManager.labelHealth.text = "" + Health.Value;

        }

        if (IsServer)
        {
            lastShoutTimer += Time.deltaTime;
        }

        if (IsClient)
        {
            playerLabel.text = menuManager.allowedNames[nameId.Value];
            //posicion de la etiqueta cerca del jugador
            playerLabel.transform.position = playerCamera.WorldToScreenPoint(transform.position 
                + new Vector3(0,0.2f,0));
        }

    }

    bool insideDamageVolume;
    float insideDVCounter = 0;
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("collision con " + other.name);
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
                animator.SetBool("Die", true);
            }
            else
            {

            }
            //Debug.Log("health: " + Health);
        }
        
    }

    //para reproducir efectos cuando muere el personaje

    void OnDeath()
    {
        //particle.play
        //sound.play
        Debug.Log("Me muero XD");
        
        //GetComponent<AudioSource>(). clip = DeathSound;
        //GetComponent<AudioSource>(). Play();
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
