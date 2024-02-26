using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActions : MonoBehaviour
{
    [SerializeField] float pickUpRadius;
    [SerializeField] GameObject pickUpButton;
    [SerializeField] GameObject closestObjectHighlight;
    GameObject createdHighlight;
    Button pickUpButtonComponent;

    bool isPickingUp = false;

    bool noErrors = true;

    void Start()
    {
        pickUpButtonComponent = pickUpButton.GetComponent<Button>();

        if (pickUpButtonComponent != null)
        {
            // Dodaj funkcjê obs³uguj¹c¹ klikniêcie przycisku
            pickUpButtonComponent.onClick.AddListener(OnPickUpButtonClick);
        }
        else
        {
            Debug.LogError("Button component not found on pickUpButton.");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (noErrors)
        {
            Vector3 point1Capsule = transform.position + new Vector3(0.0f, 10.0f, 0.0f);
            Vector3 point2Capsule = transform.position - new Vector3(0.0f, 10.0f, 0.0f);
            Collider[] colliders = Physics.OverlapCapsule(point1Capsule, point2Capsule, pickUpRadius);
            float closestDistance = 0;
            GameObject closestPickableObject = null;

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.CompareTag("Pickable"))
                {
                    if (closestPickableObject == null)
                    {
                        closestDistance = Vector3.Distance(transform.position, collider.gameObject.transform.position);
                        closestPickableObject = collider.gameObject;
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, collider.gameObject.transform.position) < closestDistance)
                        {
                            closestDistance = Vector3.Distance(transform.position, collider.gameObject.transform.position);
                            closestPickableObject = collider.gameObject;
                        }
                    }
                }
            }

            if (closestPickableObject != null)
            {
                pickUpButton.SetActive(true);

                //Instantiate highlight below object
                Ray rayToGround = new Ray(closestPickableObject.transform.position, - transform.up);
                float maxRaycastDistance = 100f;
                RaycastHit hitInfo;
                if (Physics.Raycast(rayToGround, out hitInfo, maxRaycastDistance))
                {
                    Vector3 groundPoint = hitInfo.point;
                    
                }

                if (isPickingUp)
                {
                    Destroy(closestPickableObject);
                    isPickingUp = false;
                }
            }
            else
                pickUpButton.SetActive(false);

            

            
        }
    }

    public void OnPickUpButtonClick()
    {
        isPickingUp=true;
    }
}
