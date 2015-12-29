using UnityEngine;
using Assets.Scripts.Controller;
using GameActors;

public class Select : MonoBehaviour {

    private RuntimePlatform _platform;
    public CreatureHelper CreaturePanel;
	// Use this for initialization
	void Start () {
        _platform = Application.platform;
	}
	
	// Update is called once per frame
	void Update () {

	   if (_platform == RuntimePlatform.Android || _platform == RuntimePlatform.IPhonePlayer)
	    {
	        if (Input.touchCount <= 0) return;
	        if (Input.GetTouch(0).phase == TouchPhase.Began)
	        {
	            CheckTouch(Input.GetTouch(0).position);
	        }
	        return;
	    }

	    if (Input.GetMouseButtonDown(0))
        {
            CheckTouch(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            DisplayInfo(Input.mousePosition);
        }

	}

    private static void CheckTouch(Vector3 pos){
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(pos);
        if (!Physics.Raycast(ray, out hit)) return;
        if (hit.transform.gameObject.GetComponent<TileBehaviour>() != null)
        {
            hit.transform.gameObject.GetComponent<TileBehaviour>().Select();
        }
    }

    private static void DisplayInfo(Vector3 pos)
    {
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(pos);
        if (!Physics.Raycast(ray, out hit)) return;
        if (hit.transform.gameObject.GetComponent<CreatureComponent>() != null)
        {
            hit.transform.gameObject.GetComponent<CreatureComponent>().Display(pos);
        }
    }

    public void Close()
    {
        CreatureComponent.DisplayPanel = false;
        CreaturePanel.Panel.SetActive(false);
    }

}


