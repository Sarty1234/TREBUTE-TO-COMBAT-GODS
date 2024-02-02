using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public bool CanSpawn = true;



    public Collider[] listOfCollidersInside;
    public Collider[] notFullListOfCollidersInside;



    private void OnTriggerStay(Collider other)
    {
        if (CanSpawn)
        {
            CanSpawn = false;
            Debug.Log($"{gameObject.name} became unactive");
        }
        
        
        notFullListOfCollidersInside = listOfCollidersInside.Where(obj => obj == other).ToArray();


        if (notFullListOfCollidersInside.Length == 0)
        {
            listOfCollidersInside = listOfCollidersInside.Append(other).ToArray();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        listOfCollidersInside = listOfCollidersInside.Where(obj => obj != other).ToArray();
        if (listOfCollidersInside.Length == 0)
        {
            CanSpawn = true;
            Debug.Log($"{gameObject.name} became active");
        }
    }
}
