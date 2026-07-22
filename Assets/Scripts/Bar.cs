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
    [SerializeField] private Robot robot;

    void Update()
    {
        if (Energy > 0)
        {
            Energy -= EnergyDrainRate * Time.deltaTime;

            if (robot.ismoving == true) 
                Energy -= RobotEnergyDrainRate * Time.deltaTime;

            EnergyBar.fillAmount = Energy / MaxEnergy;
        }
        else
            Energy = 0;
    }
}
