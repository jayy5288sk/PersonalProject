using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponDataLoader : MonoBehaviour
{
    [SerializeField] private string _weaponTsvUrl;
    
    private List<WeaponData> _weapons = new();
    private Dictionary<int, WeaponData> _weaponDataById = new();
}
