using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{

    [SerializeField] private Image EnergyBar;
    [SerializeField] private float Energy = 100f;
    [SerializeField] private float MaxEnergy = 100f;
    [SerializeField] private float EnergyDrainRate = 1f;
    [SerializeField] private float RobotEnergyDrainRate = 2f;

    [Header("Robot connected to this bar")]
    [SerializeField] private Robot[] robots;

    void Start()
    {
        robots = FindObjectsByType<Robot>(FindObjectsSortMode.None);
    }

    void Update()
    {
        float totalDrain = EnergyDrainRate;

        foreach (Robot robot in robots)
        {
            if (robot.ismoving)
            {
                totalDrain += RobotEnergyDrainRate;
            }
        }

        Energy -= totalDrain * Time.deltaTime;
        Energy = Mathf.Clamp(Energy, 0f, MaxEnergy);
        EnergyBar.fillAmount = Energy / MaxEnergy;
        
        // if (Energy > 0)
        // {
        //     Energy -= EnergyDrainRate * Time.deltaTime;

        //     if (robot.ismoving == true) 
        //         Energy -= RobotEnergyDrainRate * Time.deltaTime;

        //     EnergyBar.fillAmount = Energy / MaxEnergy;
        // }
        // else
        //     Energy = 0;
    }
}
