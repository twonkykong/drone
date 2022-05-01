using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class drone : MonoBehaviour
{
    public GameObject cam1, cam2, slider, timeScale, grabPoint, prop, prefab, joystick;
    public GameObject[] propellers;
    public bool rising, grab, pause, viewing;
    public float riseValue, roty;

    Vector3 FirstPoint, SecondPoint, pos1, pos2;
    float xAngle, yAngle, xAngleTemp, yAngleTemp, startTimer, rotSpeed = 0.2f;

    private void Update()
    {
        if (pause)
        {
            if (Time.timeScale > 0)
            {
                Time.timeScale -= 0.1f;
                if (Time.timeScale < 0.2f) Time.timeScale = 0;
            }
        }
        else
        {
            if (Time.timeScale < 1) Time.timeScale += 0.1f;
        }

        cam1.GetComponentInChildren<Camera>().fieldOfView = 60 + slider.GetComponent<Slider>().value * 10;
        cam2.GetComponent<Camera>().fieldOfView = 60 + slider.GetComponent<Slider>().value * 10;

        //rotate cam
        if (Input.touchCount > 0)
        {
            if (!viewing)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    FirstPoint = Input.GetTouch(0).position;
                    xAngleTemp = xAngle;
                    yAngleTemp = yAngle;
                }
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    startTimer = 0.1f;
                    SecondPoint = Input.GetTouch(0).position;
                    xAngle = xAngleTemp + (SecondPoint.x - FirstPoint.x) * 180 / Screen.width;
                    yAngle = yAngleTemp + (SecondPoint.y - FirstPoint.y) * 90 / Screen.height;
                    cam1.transform.rotation = Quaternion.Euler(-yAngle, xAngle, 0.0f);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!viewing)
        {
            if (startTimer > 0)
            {
                startTimer += 0.1f;
                if (startTimer >= 15)
                {
                    cam1.transform.rotation = Quaternion.Slerp(cam1.transform.rotation, Quaternion.Euler(Vector3.zero), rotSpeed);
                    rotSpeed -= 0.001f;
                    if (cam1.transform.rotation == Quaternion.Euler(Vector3.zero))
                    {
                        startTimer = 0;
                        rotSpeed = 0.2f;
                        xAngle = xAngleTemp = yAngle = yAngleTemp = 0;
                    }
                }
            }
        }
        //roty += joystick.transform.localPosition.x / 150 / 10;
        //riseValue = joystick.transform.localPosition.y / 150 / 10;
        cam1.transform.position = transform.position;
        cam2.transform.LookAt(transform.position);
        transform.Translate(new Vector3(Input.acceleration.x, 0, Input.acceleration.y) / 2, Space.World);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(Input.acceleration.y, roty, -Input.acceleration.x) * 90), 0.2f);

        int layerMask = 1 << 9;
        layerMask = ~layerMask;

        if (!Physics.CheckBox(transform.position - transform.up * 0.5f, Vector3.one * 0.5f, transform.rotation, layerMask) || !Physics.CheckBox(transform.position + transform.up * 0.5f, Vector3.one * 0.5f, transform.rotation, layerMask))
        {
            transform.Translate(new Vector3(0, riseValue, 0));
        }

        if (Physics.CheckBox(transform.position - transform.up * 0.5f, Vector3.one * 0.5f, transform.rotation, layerMask))
        {
            transform.position += transform.up * 0.01f;
            riseValue = 0;
        }

        if (Physics.CheckBox(transform.position + transform.up * 0.5f, Vector3.one * 0.5f, transform.rotation, layerMask))
        {
            transform.position -= transform.up * 0.01f;
            riseValue = 0;
        }

        Debug.DrawRay(transform.position, -transform.up);

        if (grab)
        {
            GetComponent<LineRenderer>().positionCount = 2;
            GetComponent<LineRenderer>().SetPositions(new Vector3[2] { transform.position, grabPoint.transform.position });
        }
        else GetComponent<LineRenderer>().positionCount = 0;
    }

    public void RiseStart(float value)
    {
        riseValue = value;
    }

    public void RiseEnd()
    {
        riseValue = 0;
    }

    public void ChangeView()
    {
        if (cam1.activeInHierarchy)
        {
            cam1.SetActive(false);
            cam2.SetActive(true);
        }
        else
        {
            cam1.SetActive(true);
            cam2.SetActive(false);
        }
    }

    public void Grab()
    {
        if (!grab)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, 2f))
            {
                if (hit.collider.tag == "prop")
                {
                    grab = true;
                    prop = hit.collider.gameObject;
                    grabPoint.transform.position = hit.point;
                    Destroy(prop.GetComponent<Rigidbody>());
                    prop.transform.SetParent(grabPoint.transform);
                    grabPoint.AddComponent<ConfigurableJoint>();
                    grabPoint.GetComponent<ConfigurableJoint>().connectedBody = GetComponent<Rigidbody>();
                    grabPoint.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Limited;
                    grabPoint.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Limited;
                    grabPoint.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Limited;
                    prop.tag = "Untagged";
                    prop.layer = 9;
                }
            }
        }
        else
        {
            grab = false;
            prop.tag = "prop";
            prop.layer = 0;
            Destroy(grabPoint.GetComponent<ConfigurableJoint>());
            prop.transform.parent = null;
            prop.AddComponent<Rigidbody>();
            prop.GetComponent<Rigidbody>().velocity = grabPoint.GetComponent<Rigidbody>().velocity;
            Destroy(grabPoint.GetComponent<Rigidbody>());
        }
    }

    public void Create()
    {
        GameObject g = Instantiate(prefab, new Vector3(0, 1, 0), Quaternion.identity);
        g.transform.localScale = new Vector3(Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f));
    }

    public void Pause()
    {
        if (pause == false) pause = true;
        else pause = false;
    }

    public void SetBool(bool value)
    {
        viewing = value;
    }
}
