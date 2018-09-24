using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMechanic : MonoBehaviour {
    [Range(.08f, 1f)]
    public float fireRate = .5f;
    [Range(1f, 25f)]
    public float fireSpeed = 25f;
    public int simultaneous = 1;
    public float rampSpeed = 50;
    public Transform firePositionDirectionAnchor;
    public Rigidbody2D shooter;
    public GameObject projectilePrefab;
    public float recoilForce = .1f;
    private Coroutine firing;
    private GameObject[] m_ProjectilePool = new GameObject[50];
    private int m_PoolIndex;
    private float m_CurrentVel = 0;

    void Awake()
    {
        BuildProjectilePool();
    }

    void Start()
    {
        firing = StartCoroutine(Forward());
    }

    private void BuildProjectilePool()
    {
        for (int i = 0; i < m_ProjectilePool.Length; i++)
        {
            if (m_ProjectilePool[i] != null)
                Destroy(m_ProjectilePool[i]);

            m_ProjectilePool[i] = Instantiate(projectilePrefab, transform);
            m_ProjectilePool[i].SetActive(false);
        }
        m_PoolIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (firing != null)
                StopCoroutine(firing);
            firing = StartCoroutine(Discharge());
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (firing != null)
                StopCoroutine(firing);
            firing = StartCoroutine(Forward());
        }
    }

    IEnumerator Forward()
    {
        while (true)
        {
            shooter.velocity = (shooter.transform.up * Mathf.SmoothDamp(shooter.velocity.magnitude, recoilForce, ref m_CurrentVel, rampSpeed * Time.deltaTime));
            yield return new WaitForSeconds(fireRate);
        }
    }
    IEnumerator Discharge()
    {
        while(true)
        {
            ShootProjectile();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void ShootProjectile()
    {
        if (GameManager.instance.GetGameState() == GameManager.GameState.GameOver)
            return;

        float spread = Wrj.Utils.Remap(simultaneous, 1, 7, 0, 22.5f);
        for (int i = 0; i < simultaneous; i++)
        {
            float angle = Wrj.Utils.Remap(i, 0, simultaneous - 1, -spread, spread) - firePositionDirectionAnchor.eulerAngles.z;
            Vector3 angleDir = new Vector3( Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle), 0);
            GameObject currentProjectile = m_ProjectilePool[m_PoolIndex];
            currentProjectile.transform.position = firePositionDirectionAnchor.position;
            currentProjectile.SetActive(true);
            currentProjectile.GetComponent<Rigidbody2D>().velocity = angleDir.normalized * fireSpeed;
            m_PoolIndex = (m_PoolIndex + 1) % m_ProjectilePool.Length;       
        }
        shooter.velocity = (-shooter.transform.up * Mathf.SmoothDamp(shooter.velocity.magnitude, recoilForce, ref m_CurrentVel, rampSpeed * Time.deltaTime));
    }
}
