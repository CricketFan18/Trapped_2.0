
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WeighingScale : MonoBehaviour, IInteractable
{
    
    private string interactionPrompt = "Press E to Weight";
    public string InteractionPrompt => interactionPrompt;
    public Transform scalePivot;
    public Transform leftScale;
    public Transform rightScale;
    private Vector3 balancedRot;
    private Vector3 leftRot = new Vector3(74f, 90f, -270f);
    private Vector3 rightRot = new Vector3(107.082f, 90f, -270f);
    private int battery = 3;
    private float cooldown = 0;
    public Transform PanelScreen;
    public Image BatteryImage;


    private void Start()
    {
        balancedRot = transform.rotation.eulerAngles;
    }

    bool IInteractable.Interact(Interactor interactor)
    {
        if (cooldown > 0) return false;
        
        // GetComponent<AudioSource>().Play();
        
        int leftWeight = CalculateWeight(leftScale);
        int rightWeight = CalculateWeight(rightScale);
        if (leftWeight == rightWeight)
        {
            scalePivot.DORotate(balancedRot, 0.3f); 
        }
        else if (leftWeight > rightWeight) UpdateScale(true);
        else UpdateScale(false);
        battery--;
        UpdateBattery();
        return true;
    }

    void UpdateBattery()
    {
        float fill = 0;
        switch (battery)
        {
            case 3: fill = 1; break;
            case 2: fill = 0.58f; break;
            case 1: fill = 0.31f; break;
            case 0: fill = 0.58f; break;
        }
        BatteryImage.fillAmount = fill;
        if (battery == 0)
        {
            Image[] c = PanelScreen.GetComponentsInChildren<Image>();
            foreach (Image i in c)
            {
                i.color = new Color(0.7132075f, 0.1915112f, 0.1547525f);
            }
            PanelScreen.GetComponent<Image>().color = new Color(0.3882353f, 0.117647f, 0.1513995f);

            cooldown = 60;
            DOTween.To(()=>cooldown, x => cooldown = x, 9.5f, 60f)
                .OnUpdate(() =>
                {
                    BatteryImage.fillAmount = 1 - (cooldown / 60f);
                }).OnComplete(() =>
                {
                    cooldown = 0;
                    RechargeBattery();
                });
        }
    }

    void RechargeBattery()
    {
        battery = 3;
        Image[] c = PanelScreen.GetComponentsInChildren<Image>();
        foreach (Image i in c)
        {
            i.color = new Color(0.4627451f, 0.7450981f, 0.9019608f);
        }
        PanelScreen.GetComponent<Image>().color = new Color(0.1195372f, 0.2804174f, 0.3886792f);
    }

    void UpdateScale(bool leftHeavy)
    {
        float moveAmount = (leftHeavy)? 15 : -15;
        scalePivot.DORotate((leftHeavy)? leftRot : rightRot, 0.3f);
    }
    
    int CalculateWeight(Transform t)
    {
        int weight = 0;
        Vector3 checkPos = t.position; 
        Collider[] hits = Physics.OverlapSphere(checkPos, 0.75f);
        foreach (Collider c in hits)
        {
            Gem g = c.GetComponent<Gem>();
            if (g != null)
            {
                weight += g.weight;
            }
        }
        Debug.Log(weight);
        return weight;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftScale.position, 0.4f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rightScale.position, 0.4f);
    }
}
