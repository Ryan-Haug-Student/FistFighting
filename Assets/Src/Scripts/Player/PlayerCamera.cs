using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    public float sens;
    public int defaultFov;

    public Transform orientation;

    public Camera playerCam;
    public AudioListener audioListener;
    public Movement player;

    float xRotation;
    float yRotation;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            playerCam.enabled = false;
            audioListener.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerCam.enabled = true;
        audioListener.enabled = true;
    }

    private void Update()
    {
        if (!IsOwner) return;

        LookControl();
        FovControl();
    }

    private void LookControl()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void FovControl()
    {
        if (player.state == Movement.MS.sprinting || player.state == Movement.MS.air && Input.GetKey(KeyCode.LeftShift))
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, defaultFov + 10, .1f);

        else if (player.state == Movement.MS.sliding)
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, defaultFov + 15, .1f);
        else
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, defaultFov, .1f);
    }
}
