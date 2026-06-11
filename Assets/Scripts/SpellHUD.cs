using UnityEngine;
using UnityEngine.UI;

public class SpellHUD : MonoBehaviour
{
    [System.Serializable]
    public class SpellSlot
    {
        public GameObject root;
        public Image icon;                  // Tinted to show selection state
        public Image cooldownOverlay;       // Image Type: Filled, Radial360, Fill Origin: Top, Clockwise
    }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private AbilityData abilities;

    [Header("Spell Slots")]
    [SerializeField] private SpellSlot arcaneBlastSlot;
    [SerializeField] private SpellSlot fireballSlot;
    [SerializeField] private SpellSlot iceBoltSlot;

    [Header("Teleport Indicator")]
    [SerializeField] private GameObject teleportIndicator;

    [Header("Icons (optional — leave empty to use color only)")]
    [SerializeField] private Sprite arcaneBlastIcon;
    [SerializeField] private Sprite fireballIcon;
    [SerializeField] private Sprite iceBoltIcon;

    [Header("Visuals")]
    [SerializeField] private Color selectedColor   = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color cooldownTint    = new Color(0f, 0f, 0f, 0.65f);

    private void Start()
    {
        AssignIcon(arcaneBlastSlot, arcaneBlastIcon);
        AssignIcon(fireballSlot,    fireballIcon);
        AssignIcon(iceBoltSlot,     iceBoltIcon);

        SetFillAmount(arcaneBlastSlot, 0f);
        SetFillAmount(fireballSlot,    0f);
        SetFillAmount(iceBoltSlot,     0f);
    }

    private void Update()
    {
        if (player == null || abilities == null) return;

        SpecialType? selected = player.CurrentSpecial;

        UpdateSlot(arcaneBlastSlot, SpecialType.ArcaneBlast, true,                 selected, player.ArcaneBlastCooldown01);
        UpdateSlot(fireballSlot,    SpecialType.Fireball,    abilities.hasFireball, selected, player.FireballCooldown01);
        UpdateSlot(iceBoltSlot,     SpecialType.IceBolt,     abilities.hasIceBolt,  selected, player.IceBoltCooldown01);

        if (teleportIndicator != null)
            teleportIndicator.SetActive(abilities.hasTeleport);
    }

    private void UpdateSlot(SpellSlot slot, SpecialType type, bool unlocked, SpecialType? selected, float cooldown01)
    {
        if (slot.root == null) return;

        slot.root.SetActive(unlocked);
        if (!unlocked) return;

        if (slot.icon != null)
            slot.icon.color = selected == type ? selectedColor : unselectedColor;

        if (slot.cooldownOverlay != null)
        {
            slot.cooldownOverlay.color      = cooldownTint;
            slot.cooldownOverlay.fillAmount = cooldown01;
        }
    }

    private void AssignIcon(SpellSlot slot, Sprite sprite)
    {
        if (slot.icon != null && sprite != null)
            slot.icon.sprite = sprite;
    }

    private void SetFillAmount(SpellSlot slot, float amount)
    {
        if (slot.cooldownOverlay != null)
            slot.cooldownOverlay.fillAmount = amount;
    }
}
