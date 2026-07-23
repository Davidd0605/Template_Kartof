using UnityEngine;
using TMPro;

public class LogIn : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private GameObject mainCanvas;
    [SerializeField]
    private GameObject bootupCanvas;
    
    public void CheckPassword()
    {
        if (inputField.text == "password")
        {
            mainCanvas.SetActive(true);
            bootupCanvas.SetActive(false);
        }
    }   
}