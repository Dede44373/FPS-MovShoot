using UnityEngine;

public class Week7 : MonoBehaviour
{
    public LayerMask layerMask;
    GameObject raycastObj;
    Vector3 hitPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool passedThrough = RaycastCheck();
    }

    
    void SnapObject()
    {
        transform.position = hitPos;
    }

    bool RaycastCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))
        {
            raycastObj = hit.collider.gameObject;
            hitPos = hit.point;
            return false;
        }
        else
        {
            if (raycastObj != null)
            {
                raycastObj = null;
                Debug.Log("passed through something");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
