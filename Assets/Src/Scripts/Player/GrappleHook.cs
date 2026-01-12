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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            StartGrapple();

        if (Input.GetKeyUp(KeyCode.Mouse1))
            StopGrapple();

        crosshair.color = Physics.Raycast(camTransform.position, camTransform.forward, maxDistance, whatIsGround) ?
            Color.green : Color.red;

        if (spring)
        {
            lineRenderer.SetPosition(0, grapplePointVisual.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }

    void StartGrapple()
    {
        if (spring != null) Destroy(spring);

        if (Physics.Raycast(camTransform.position, camTransform.forward,
            out RaycastHit hit, maxDistance, whatIsGround))
        {
            grapplePoint = hit.point;

            spring = gameObject.AddComponent<SpringJoint>();
            spring.autoConfigureConnectedAnchor = false;
            spring.connectedAnchor = grapplePoint;
            spring.anchor = Vector3.zero;

            float distance = Vector3.Distance(playerRb.position, grapplePoint);
            distance = Mathf.Min(distance, maxDistance); // ensure it never exceeds maxDistance
            spring.maxDistance = distance * 0.8f;
            spring.minDistance = distance * 0.25f;

            spring.spring = springForce;
            spring.damper = damper;
            spring.massScale = massScale;

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }

    void StopGrapple()
    {
        if (spring)
            Destroy(spring);

        lineRenderer.enabled = false;
    }
}

