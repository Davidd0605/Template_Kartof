using System.Collections;
using UnityEngine;

public class SpawnPopUps : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject[] popUpPrefabs;
    [SerializeField] private int[] x = new int[2];
    [SerializeField] private int[] y = new int[2];
    
    [Header("Timing")]
    [SerializeField] private float minSpawnTime = 4f;
    [SerializeField] private float maxSpawnTime = 6f;
    
    private bool canSpawn = true;

    void Update()
    {
        if (canSpawn)
        {
            StartCoroutine(SpawnPopUpRoutine());
        }
    }

    IEnumerator SpawnPopUpRoutine()
    {
        canSpawn = false;
        
        GameObject newPopUp = Instantiate(popUpPrefabs[Random.Range(0, popUpPrefabs.Length)], transform, false);
        
        RectTransform rectTransform = newPopUp.GetComponent<RectTransform>();
        
        Vector2 randomTargetPos = new Vector2(Random.Range(x[0], x[1]), Random.Range(y[0], y[1])); 
        rectTransform.anchoredPosition = randomTargetPos;
        
        float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
        yield return new WaitForSeconds(waitTime);
        
        canSpawn = true;
    }
}