using UnityEngine;

public class GrappleRope : MonoBehaviour
{
    [Header("Spiral stats")]
    public Transform object1;
    public Transform object2;
    private LineRenderer lineRenderer;
    const int NUMBER_OF_POINTS = 100;
    const int NUMBER_OF_REVOLUTIONS = 5;
    const float SPIRAL_WIDTH = 2.0f;


    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = NUMBER_OF_POINTS;
    }
    private void Start()
    {
        //CreateCurveShape();
    }
    private void CreateCurveShape()
    {
        float objectDistance = Vector3.Distance(object1.position, object2.position);
        Vector3[] points = new Vector3[NUMBER_OF_POINTS];
        for (int i = 0; i < NUMBER_OF_POINTS; i++)
        {
            // Define plane with normal pointing to second object
            Vector3 planeForwardDirection = (object2.position - object1.position);
            Vector3 planeRightDirection = Quaternion.Euler(90.0f, 0.0f, 0.0f) * planeForwardDirection;
            Vector3 planeUpDirection = Vector3.Cross(planeForwardDirection, planeRightDirection);

            // Some additional debug to visualize our plane
            Debug.DrawLine(object1.position, planeForwardDirection, Color.red, 1000);
            Debug.DrawLine(object1.position, planeUpDirection, Color.cyan, 1000);
            Debug.DrawLine(object1.position, planeRightDirection, Color.green, 1000);

            // Distance along the object to object axis
            float currentDistanceNormalized = (float)i / (float)NUMBER_OF_POINTS;
            float currentDistance = currentDistanceNormalized * objectDistance;

            // New coords on our plane facing the second object
            Vector3 spiralCoords = SPIRAL_WIDTH * planeUpDirection.normalized * Mathf.Sin(NUMBER_OF_REVOLUTIONS * currentDistanceNormalized * 2 * Mathf.PI) +
                                   SPIRAL_WIDTH * planeRightDirection.normalized * (Mathf.Cos(NUMBER_OF_REVOLUTIONS * currentDistanceNormalized * 2 * Mathf.PI) - 1.0f) +
                                   planeForwardDirection.normalized * currentDistance;
            //points *= spiralCoords;*
                { lineRenderer.SetPositions(points); }
        }
    }

}
