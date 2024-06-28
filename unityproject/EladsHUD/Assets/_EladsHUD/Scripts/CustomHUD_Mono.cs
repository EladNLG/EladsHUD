using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CustomHUD_Mono : MonoBehaviour
{
    public static CustomHUD_Mono instance;

    // For future maintainers.
    // Keep this in the same ORDER as the CustomHUD_Mono script in the main project.
    // In the main script, properties and fields may be APPENDED, but nothing else may change
    // otherwise since unity will shit itself probably.
    [Header("Health")]
    public CanvasGroup healthGroup;
    public Image healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthText2;
    [Header("Stamina")]
    public CanvasGroup staminaGroup;
    public Image staminaBar;
    public Image staminaBarChangeFG;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI carryText;
    [Header("Battery")]
    public CanvasGroup batteryGroup;
    public Image batteryBar;
    public TextMeshProUGUI batteryText;
    [Header("Flashlight")]
    public CanvasGroup flashlightGroup;
    public Image flashlightBar;
    public TextMeshProUGUI flashlightText;

    private void Awake()
    {
        if (instance != null)
            throw new System.Exception("2 instances of CustomHUD_Mono!");
        instance = this;
    }
}