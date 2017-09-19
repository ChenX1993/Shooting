using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUDBehaviour : MonoBehaviour {

    public static HUDBehaviour instance;

    private PlayerBehaviour pB;
    public GameObject weaponInfo;
    public Text weaponAmmo;
    public Image weaponIcon;
    public Image centerCross;
    private WeaponBase currentWeapon;
    public int maxBulletHolesInScene = 256;
    [HideInInspector]
    public int nextBulletHole;

    [HideInInspector]
    public List<GameObject> bulletHoles = new List<GameObject>(); 

	void Start () {

        instance = this;
        GameObject _bulletHoleRepository = new GameObject("Bullet Hole Repository");
        GameObject _bulletHolePrefab = (GameObject)Resources.Load("Prefabs/Particles/BulletHole");

        for(int i = 0; i < maxBulletHolesInScene; i++)
        {
            GameObject _hole = Instantiate(_bulletHolePrefab, _bulletHoleRepository.transform.position, Quaternion.identity) as GameObject;
            _hole.SetActive(false);
            _hole.transform.parent = _bulletHoleRepository.transform;
            bulletHoles.Add(_hole);
        }

        pB = GetComponent<PlayerBehaviour>();
        pB.OnWeaponSwitch += GetWeaponSprites;
	}
	
	void Update () {

        WeaponInfo();


    }
    void WeaponInfo()
    {
        if (currentWeapon && pB.equippedWeapon)
        {
            if (pB.aim)
            {
                centerCross.enabled = true;
            }
            else
            {
                centerCross.enabled = false;
            }
            weaponInfo.SetActive(true);

            float _s = currentWeapon.currentRecoil * 4;
            centerCross.transform.localScale = new Vector3(_s + 0.3f, _s + 0.3f, 1);

            weaponAmmo.text = currentWeapon.currentAmmo.ToString() + " / " + currentWeapon.reloadBullets.ToString();
        }
        else
        {
            weaponInfo.SetActive(false);
            centerCross.enabled = false;
        }

    }
    void GetWeaponSprites()
    {
        currentWeapon = pB.currentWeapon;

        if (currentWeapon.centerCross)
        {
            centerCross.sprite = currentWeapon.centerCross;
        }

        if (currentWeapon.icon)
        {
            weaponIcon.enabled = true;
            weaponIcon.sprite = currentWeapon.icon;
        }
        else
        {
            weaponIcon.enabled = false;
        }
    }
}
