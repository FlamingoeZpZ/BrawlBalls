using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Stats", menuName = "Stats/WeaponStats", order = 3)]
public class WeaponStats : ScriptableObject
{
    [field: SerializeField,TextArea] public string Description { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float Mass { get; private set; }
    [field: SerializeField] public Vector3 Range { get; private set; }
    [field: SerializeField] public bool ForceBasedDamage { get; private set; }
    
    [field: Header("In Game")]
    [field: SerializeField] public float BaseDist { get; private set; }
    

}
