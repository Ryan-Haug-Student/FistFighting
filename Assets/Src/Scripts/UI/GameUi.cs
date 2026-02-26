using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUi : MonoBehaviour
{
    [Header("UI Refs")]
    public Image healthBarFill;
    public TMP_Text weaponTxt;
    public TMP_Text weaponCountTxt;
    public TMP_Text timeTxt;
    public TMP_Text speedStateTxt;
    public TMP_Text scoreTxt;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Slider sensSlider;
    public TMP_Text sensTxt;

    [Header("Player Refs")]
    public Movement pm;
    public PlayerHealth ph;
    public PlayerCamera pc;
    public WeaponManager wm;

    public Image croshair;
    public Transform camTransform;
    public LayerMask whatIsGround;

    [Header("Misc")]
    public static GameUi Instance; 
    public GameManager gm;

    private bool binded = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseMenu();

        if (binded)
            UpdateStats();
    }

    // ========================

    public void BindToPlayer(GameObject player)
    {
        pm = player.GetComponentInChildren<Movement>();
        ph = player.GetComponentInChildren<PlayerHealth>();
        wm = player.GetComponentInChildren<WeaponManager>();

        pc = player.GetComponentInChildren<PlayerCamera>();

        camTransform = pc.gameObject.transform;

        sensSlider.value = pc.sens / 1000;
        sensTxt.text = $"{pc.sens / 1000}";

        binded = true;
    }

    private void UpdateStats()
    {
        healthBarFill.transform.localScale = new Vector2((float)(ph.currentHealth / (float)ph.maxHealth), 1);
        healthBarFill.transform.localPosition = new Vector2(-175 * (1 - (float)(ph.currentHealth / (float)ph.maxHealth)), 0);

        weaponTxt.text = wm.weapon ? wm.weapon.GetType().Name : "err 404";
        weaponCountTxt.text = wm.weapon ? $"{wm.weapon.currentCount}/{wm.weapon.maxCount}" : "err 404";

        croshair.color = Physics.Raycast(camTransform.position, camTransform.forward, 12, whatIsGround) ?
            Color.green : Color.red;

        speedStateTxt.text = $"{Mathf.Round(pm.rb.linearVelocity.magnitude * 10) / 10} - {pm.state}";

        scoreTxt.text = $"K/O - 0";
        timeTxt.text = $"{gm.secondsLeft.Value}s Remain";
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu.activeSelf == false)
        {
            pauseMenu.SetActive(true);

            pc.enabled = false;
            wm.weapon.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            pauseMenu.SetActive(false);

            pc.enabled = true;
            wm.weapon.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void UpdateSens()
    {
        pc.sens = sensSlider.value * 1000;
        sensTxt.text = $"Sensitivity - {Mathf.Round(sensSlider.value * 100) / 10}";
    }
}
