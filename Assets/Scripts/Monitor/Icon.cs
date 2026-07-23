using UnityEngine;
using UnityEngine.EventSystems;


public class Icon : MonoBehaviour
{
    [SerializeField]
    private GameObject tabPrefab;
    [SerializeField]
    private GameObject lowPriorityGroup;
    public void SpawnTab()
    {
        GameObject tab = Instantiate(tabPrefab, transform);
        tab.transform.SetParent(lowPriorityGroup.transform);
        tab.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }
}
