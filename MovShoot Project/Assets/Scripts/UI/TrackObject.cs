using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TrackObject : MonoBehaviour
{
    public GameObject obj;
    public Camera cam;
    public RectTransform rt;
    public Image image;
    Vector2 pos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }
    
    // Update is called once per frame
    void LateUpdate()
    {
        if (obj.activeSelf)
        {
            image.enabled = true;
            Vector3 targetPos = obj.transform.position;
                Vector3 camForward = cam.transform.forward;
                Vector3 camPos = cam.transform.position + camForward;

                float distInFrontOfCamera = Vector3.Dot(targetPos - camPos, camForward);
                if (distInFrontOfCamera < 0f)
                {
                    targetPos -= camForward * distInFrontOfCamera;
                }
        
            pos = RectTransformUtility.WorldToScreenPoint(cam, targetPos);
            rt.position = pos;

        }
        else
        {
            image.enabled = false;
        }
    }
}
