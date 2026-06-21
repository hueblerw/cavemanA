using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CavemanLand.Models;
using CavemanLand.Utility;

public class DateController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI yearText;
    public TextMeshProUGUI dayText;
    public Button incrementYearButton;
    public Button incrementDayButton;

    // The date we're currently viewing (can be different from world.currentDate)
    private int viewingYear = 1;
    private int viewingDay = 1;

    private World world;

    // Singleton pattern for easy access from TileInspector
    public static DateController Instance { get; private set; }

    // Event fired when the viewing day changes
    public event System.Action OnDayChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Get world reference
        WorldController1 worldController = FindAnyObjectByType<WorldController1>();
        if (worldController != null)
        {
            world = worldController.GetWorld();
            if (world != null && world.currentDate != null)
            {
                viewingYear = world.currentDate.year;
                viewingDay = world.currentDate.day;
            }
        }

        // Setup button listeners
        if (incrementYearButton != null)
        {
            incrementYearButton.onClick.AddListener(IncrementYear);
        }

        if (incrementDayButton != null)
        {
            incrementDayButton.onClick.AddListener(IncrementDay);
        }

        UpdateDisplay();
    }

    void IncrementYear()
    {
        viewingYear++;
        UpdateDisplay();
        OnDayChanged?.Invoke();
    }

    void IncrementDay()
    {
        viewingDay++;
        if (viewingDay > WorldDate.DAYS_PER_YEAR)
        {
            viewingDay = 1;
            viewingYear++;
        }
        UpdateDisplay();
        OnDayChanged?.Invoke();
    }

    void UpdateDisplay()
    {
        if (yearText != null)
        {
            yearText.text = $"Year: {viewingYear}";
        }

        if (dayText != null)
        {
            dayText.text = $"Day: {viewingDay}";
        }
    }

    // Public accessors for TileInspector
    public int GetViewingYear()
    {
        return viewingYear;
    }

    public int GetViewingDay()
    {
        return viewingDay;
    }

    // Get 0-indexed day for array access
    public int GetViewingDayIndex()
    {
        return viewingDay - 1;
    }

    // Reset to current world date
    public void ResetToWorldDate()
    {
        if (world != null && world.currentDate != null)
        {
            viewingYear = world.currentDate.year;
            viewingDay = world.currentDate.day;
            UpdateDisplay();
        }
    }
}
