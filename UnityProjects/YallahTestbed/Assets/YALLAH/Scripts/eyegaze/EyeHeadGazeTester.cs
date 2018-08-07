using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EyeHeadGazeController))]
public class EyeHeadGazeTester : MonoBehaviour
{

	public GameObject gazeTarget;

    public bool lookNow = false;

    public bool stare = true;

    public bool stopLooking = false;

    private EyeHeadGazeController gazescript;

	// Use this for initialization
	void Start()
    {
        this.gazescript = gameObject.GetComponent<EyeHeadGazeController>();
	}
	
	// Update is called once per frame
	void Update()
    {

        Vector3 target_position = this.gazeTarget.transform.position;

        if (this.lookNow || this.stare)
        {
            gazescript.LookAtPoint(target_position);

            this.lookNow = false;
        }

        if (this.stopLooking)
        {
            this.stare = false;
            this.gazescript.StopLooking();
            this.stopLooking = false;
        }

	}
	
}
