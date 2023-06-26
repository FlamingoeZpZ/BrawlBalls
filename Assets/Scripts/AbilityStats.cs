using UnityEngine;

[CreateAssetMenu(fileName = "Ball Stats", menuName = "Stats/BallStats", order = 1)]
public class AbilityStats : ScriptableObject
{
    [field: SerializeField,TextArea] public string Description { get; private set; }
    [field: SerializeField] public int Capacity { get; private set; }
    [field: SerializeField] private float cooldown;
    public readonly WaitForSeconds Cooldown;

    AbilityStats()
    {
        //Memory Optimization
        Cooldown = new WaitForSeconds(cooldown);
    }

}