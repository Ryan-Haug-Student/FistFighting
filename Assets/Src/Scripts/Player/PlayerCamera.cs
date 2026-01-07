using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    public float sens;

    public Transform orientation;

    public Camera playerCam;
    public AudioListener audioListener;

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

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);   
    }
}
