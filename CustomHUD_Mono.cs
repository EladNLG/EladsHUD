using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameNetcodeStuff;
using CustomHUD;

public enum PocketFlashlightOptions
{
    Disabled,
    Vanilla,
    Separate
}
public enum StaminaTextOptions
{
    Disabled,
    PercentageOnly,
    Full
}
class CustomHUD_Mono : MonoBehaviour
{
    public static CustomHUD_Mono instance;

    // PUBLIC, MAKE SURE ITS THE SAME AS IN DUMMY SCRIPT
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
    // PRIVATE, NO NEED TO SYNC
    private Color staminaColor;
    private Color staminaWarnColor = new Color(255, 0, 0);
    private float colorLerp;
    private int lastHealth = 100;
    private float lastHealthChange = 0;

    private void Awake()
    {
        if (instance != null)
            throw new System.Exception("2 instances of CustomHUD_Mono!");
        instance = this;
    }

    void Start()
    {
        staminaColor = staminaBar.color;
    }
    
    public void UpdateFromPlayer(PlayerControllerB player)
    {
        lastHealthChange += Time.deltaTime;
        // only show healthbar if we took damage
        if (player.health < lastHealth)
        {
            Debug.Log("o shit");
            Debug.Log(player.health);
            Debug.Log(lastHealth);
            lastHealthChange = 0f;
        }
        lastHealth = player.health;
        int health = player.health;
        float stamina = player.sprintMeter;
        float sprintTime = player.sprintTime;
        
        if (stamina < 0.3)
            colorLerp = Mathf.Clamp01(colorLerp + Time.deltaTime * 8f);
        else
            colorLerp = Mathf.Clamp01(colorLerp - Time.deltaTime * 8f);
        staminaBar.color = Color.Lerp(staminaColor, staminaWarnColor, colorLerp);

        int staminaPercent = Mathf.FloorToInt(stamina * 100);

        float staminaOverTime = CalculateStaminaOverTime(player);
        float staminaPercentOverTime = staminaOverTime * 100;

        if (Plugin.detailedStamina.Value != StaminaTextOptions.Disabled)
        {
            staminaText.text = $"{staminaPercent}<size=75%><voffset=1>%</voffset></size>";
        }
        else staminaText.text = "";

        switch (Mathf.Sign(staminaOverTime))
        {
            case -1:
                staminaBar.fillAmount = stamina - Mathf.Abs(staminaOverTime);
                staminaBarChangeFG.color = Color.Lerp(Color.white, staminaWarnColor, colorLerp);
                staminaBarChangeFG.fillAmount = Mathf.Min(stamina, Mathf.Abs(staminaOverTime));
                staminaBarChangeFG.rectTransform.localPosition = new Vector3(1f + 276f * Mathf.Max(0, stamina - Mathf.Abs(staminaOverTime)) + 0.05f, 0);

                if (Plugin.detailedStamina.Value == StaminaTextOptions.Full)
                    staminaText.text += $" | {staminaPercentOverTime.ToString("0.0")}<size=75%>/sec</size>";

                break;
            case 0:
                staminaBar.fillAmount = stamina;
                staminaBarChangeFG.fillAmount = 0;

                if (Plugin.detailedStamina.Value == StaminaTextOptions.Full)
                    staminaText.text += $" | +0.0<size=75%>/sec</size>";

                break;
            default:
            case 1:
                staminaBar.fillAmount = stamina;
                staminaBarChangeFG.fillAmount = 0;

                if (Plugin.detailedStamina.Value == StaminaTextOptions.Full)
                    staminaText.text += $" | +{staminaPercentOverTime.ToString("0.0")}<size=75%>/sec</size>";

                break;
        }

        float weightDisplay = Mathf.RoundToInt(Mathf.Clamp(player.carryWeight - 1f, 0.0f, 100f) * 105f);
        if (Plugin.shouldDoKGConversion)
        {
            weightDisplay *= 0.453592f;
            carryText.text = string.Format("{0}<size=60%>kg</size>", weightDisplay);
        }
        else
            carryText.text = string.Format("{0}<size=60%>lb</size>", weightDisplay);

        // Health
        healthBar.fillAmount = health / 100f;
        healthText.text = health.ToString();
        if (healthText2 != null)
        {
            healthText2.text = health.ToString();
        }
        float delay = Plugin.healthbarHideDelay.Value;
        healthGroup.alpha = Plugin.autoHideHealthbar.Value ? Mathf.InverseLerp(delay + 1, delay, lastHealthChange) : 1f;

        flashlightGroup.gameObject.SetActive(UpdateFlashlight(player));
        batteryGroup.gameObject.SetActive(UpdateBattery(player));
    }

    bool UpdateFlashlight(PlayerControllerB player)
    {
        if (Plugin.pocketedFlashlightDisplayMode.Value != PocketFlashlightOptions.Separate)
            return false;

        if (!player.helmetLight.enabled)
            return false;

        GrabbableObject heldItem = player.pocketedFlashlight;

        if (heldItem == null)
            return false;

        if (!heldItem.itemProperties.requiresBattery)
            return false;

        flashlightBar.fillAmount = heldItem.insertedBattery.charge;
        int useTimeRemaining = Mathf.CeilToInt(heldItem.insertedBattery.charge * heldItem.itemProperties.batteryUsage);

        flashlightText.text = $"{Mathf.CeilToInt(heldItem.insertedBattery.charge * 100)}%";

        if (!Plugin.displayTimeLeft.Value)
            return true;

        flashlightText.text += $" <size=60%>{useTimeRemaining / 60}:{(useTimeRemaining % 60).ToString("D2")}";

        return true;
    }

    bool UpdateBattery(PlayerControllerB player)
    {
        GrabbableObject heldItem = player.currentlyHeldObjectServer;

        if (heldItem == null && Plugin.pocketedFlashlightDisplayMode.Value == PocketFlashlightOptions.Vanilla)
        {
            heldItem = player.pocketedFlashlight;
        }

        if (heldItem == null)
            return false;

        if (!heldItem.itemProperties.requiresBattery)
            return false;

        batteryBar.fillAmount = heldItem.insertedBattery.charge;
        int usesRemaining = (int)(heldItem.insertedBattery.charge / heldItem.itemProperties.batteryUsage);
        int useTimeRemaining = Mathf.CeilToInt(heldItem.insertedBattery.charge * heldItem.itemProperties.batteryUsage);
        batteryText.text = $"{Mathf.CeilToInt(heldItem.insertedBattery.charge * 100)}%";

        if (!Plugin.displayTimeLeft.Value)
            return true;

        if (heldItem.itemProperties.itemIsTrigger)
            batteryText.text += $" ({usesRemaining} uses remaining)";
        else
            batteryText.text += $" ({useTimeRemaining / 60}:{(useTimeRemaining % 60).ToString("D2")} remaining)";

        return true;
    }

    float CalculateStaminaOverTime(PlayerControllerB player)
    {
        if (player.sprintMeter == 1)
            return 0;

        bool isWalking = player.GetPrivateField<bool>("isWalking");
        float sprintTime = player.sprintTime;
        float result;

        float staminaCostMultiplier = 1f;
        if ((double)player.drunkness > 0.019999999552965164)
            staminaCostMultiplier *= Mathf.Abs(StartOfRound.Instance.drunknessSpeedEffect.Evaluate(player.drunkness) - 1.25f);

        if (player.isSprinting)
            result = -1f / sprintTime * player.carryWeight * staminaCostMultiplier;
        else if (player.isMovementHindered > 0 && isWalking)
            result = -1f / sprintTime * staminaCostMultiplier * 0.5f;
        else
        {
            if (isWalking)
                result = 1f / (sprintTime + 9f) * staminaCostMultiplier;
            else result = 1f / (sprintTime + 4f) * staminaCostMultiplier;
        }

        return result;
    }
}