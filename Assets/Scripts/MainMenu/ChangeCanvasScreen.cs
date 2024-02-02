using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChangeCanvasScreen : MonoBehaviour
{
    [Serializable]
    public class CanvasObject {
        public string objectName;
        public GameObject objectToInteract;
    }


    public static ChangeCanvasScreen Instance;
    private ChangeCanvasScreen()
    {
        Instance = this;
    }



    public CanvasObject[] canvasObjects;
    [SerializeField] string defoultObjectToShow;



    void Start()
    {
        LoadObject(defoultObjectToShow);
    }


    public void LoadObject(string name)
    {
        foreach (var obj in canvasObjects)
        {
            obj.objectToInteract.gameObject.SetActive(false);
        }


        canvasObjects.Where(obj => obj.objectName == name).First().objectToInteract.gameObject.SetActive(true);
    }
}
