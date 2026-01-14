using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GrappleHook : NetworkBehaviour
{
    [Header("References")]
    public Transform camTransform;
    public Transform grapplePointVisual;
    public LineRenderer lineRenderer;
    public Rigidbody playerRb;
    public Image crosshair;

    [Header("Grapple Settings")]
    public float maxDistance;
    public LayerMask whatIsGround;

    [Header("Spring Settings")]
    public float springForce;
    public float damper;
    public float massScale;

    private SpringJoint spring;
    private Vector3 grapplePoint;

    // For networked clients
    private bool isGrappling = false;
    private Vector3 networkGrapplePoint;

    void Update()
    {
        if (IsOwner)
            UpdateOwnerLine();
        else
            UpdateOtherClientLine();
    }
    void UpdateOwnerLine()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            StartGrapple();

        if (Input.GetKeyUp(KeyCode.Mouse1))
            StopGrapple();

        if (spring)
        {
            lineRenderer.SetPosition(0, grapplePointVisual.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }

        crosshair.color = Physics.Raycast(camTransform.position, camTransform.forward, maxDistance, whatIsGround) ?
            Color.green : Color.red;
    }

    void UpdateOtherClientLine()
    {
        if (!isGrappling) return;

        lineRenderer.SetPosition(0, grapplePointVisual.position);
        lineRenderer.SetPosition(1, networkGrapplePoint);
        lineRenderer.enabled = true;
    }

    void StartGrapple()
    {
        if (spring != null) Destroy(spring);

        if (Physics.Raycast(camTransform.position, camTransform.forward,
            out RaycastHit hit, maxDistance, whatIsGround))
        {
            grapplePoint = hit.point;

            spring = gameObject.AddComponent<SpringJoint>()
            ;
            spring.autoConfigureConnectedAnchor = false;
            spring.connectedAnchor = grapplePoint;
            spring.anchor = Vector3.zero;

            float distance = Vector3.Distance(playerRb.position, grapplePoint);
            distance = Mathf.Min(distance, maxDistance);
            spring.maxDistance = distance * 0.8f;
            spring.minDistance = 0;

            spring.spring = springForce;
            spring.damper = damper;
            spring.massScale = massScale;

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(1, grapplePoint);

            // Notify other clients
            StartGrappleServerRpc(grapplePoint);
        }
    }

    void StopGrapple()
    {
        if (spring) Destroy(spring);

        lineRenderer.enabled = false;

        // Notify other clients
        StopGrappleServerRpc(grapplePoint);
    }

    // Networking
    [ServerRpc]
    private void StartGrappleServerRpc(Vector3 point)
    {
        ToggleGrappleClientRpc(true, point);
    }

    [ServerRpc]
    private void StopGrappleServerRpc(Vector3 point)
    {
        ToggleGrappleClientRpc(false, point);
    }

    [ClientRpc]
    private void ToggleGrappleClientRpc(bool enabled, Vector3 point)
    {
        isGrappling = enabled;
        networkGrapplePoint = point;

        if (!enabled)
            lineRenderer.enabled = false;
    }
}
