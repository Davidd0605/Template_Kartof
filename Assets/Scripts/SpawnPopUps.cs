using UnityEngine;

public class SpawnPopUps : MonoBehaviour
{
    [SerializeField] 
    private GameObject[] popUpPrefab;
    private GameObject popUp;
    private Vector3 position_start;
    private Vector3 position_end;
    void Update()
    {
        popUp = Instantiate(popUpPrefab[Random.Range(0, popUpPrefab.Length)]);
        popUp.transform.position = new Vector3();
    }
}
