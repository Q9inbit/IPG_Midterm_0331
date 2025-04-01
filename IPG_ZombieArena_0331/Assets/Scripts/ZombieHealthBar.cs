using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] private Slider healthBar;

    public void SetHealthBar(float value)
    {
        healthBar.value = value;
    }

    private void LateUpdate()
    {
        canvas.transform.forward = Camera.main.transform.forward;
    }
}
