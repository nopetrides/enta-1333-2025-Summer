
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;  // fill image  to show health bar

    private IDamageable target;
    private Camera mainCam;

    public void Attach(IDamageable target) // health bar attached to all IDamageable objects
    {
        this.target = target;
        mainCam = Camera.main;
        UpdateHealthBar();
    }

    void Update()
    {
        if (target == null || !target.IsAlive)
        {
            Destroy(gameObject); // remove if target is gone
            return;
        }
  
        transform.position = target.GetTransform().position + Vector3.up * 2.0f;         // position above target, and face the camera
        transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                         mainCam.transform.rotation * Vector3.up);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (target == null) return;
        fillImage.fillAmount = (float)target.CurrentHealth / target.MaxHealth;
    }
}
