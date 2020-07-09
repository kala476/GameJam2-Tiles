using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionChoice : MonoBehaviour
{
    //references
    public GameObject ChoicePanel;
    public Button topButton;
    public Button bottomButton;

    private bool popUpOpen;

    CharacterController characterController;

      // Start is called before the first frame update
    void Start()
    {
        characterController = FindObjectOfType<CharacterController>();    
    }

    // Update is called once per frame
    void Update()
    {   
        if (popUpOpen == false)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                characterController.SetMovementPaused(true);
                ShowChoiceWindow();
            }
        }

        if (popUpOpen) 
        {
            SelectButtonAccordingToCursor();

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                InvokeButtonAccordingToCursor();
                HideChoiceWindow();
                characterController.SetMovementPaused(false);


            }
        }
    }

    public void Assemble() 
    {
        Debug.Log("Assembling");
    }

    public void Disassemble() 
    {
        Debug.Log("Disassembling");
    }

    private void ShowChoiceWindow() 
    {
        ChoicePanel.gameObject.SetActive(true);
        popUpOpen = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideChoiceWindow()
    {
        ChoicePanel.gameObject.SetActive(false);
        popUpOpen = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SelectButtonAccordingToCursor() 
    {
        //setActiveButton
        if (CursorScreenPosition().y >= 0.5)
        {
            if (EventSystem.current.currentSelectedGameObject != topButton)
                EventSystem.current.SetSelectedGameObject(topButton.gameObject);
        }
        else 
        {
            if (EventSystem.current.currentSelectedGameObject != bottomButton)
                EventSystem.current.SetSelectedGameObject(bottomButton.gameObject);
        }
    }

    private void InvokeButtonAccordingToCursor() 
    {
        if (CursorScreenPosition().y >= 0.5f)
        {
            topButton.onClick?.Invoke();
        }
        else 
        {
            bottomButton.onClick?.Invoke();
        }
    }

    private Vector2 CursorScreenPosition() 
    {
        Vector2 cursorPixelPosition = Input.mousePosition;
        return new Vector2( cursorPixelPosition.x/ Screen.width ,cursorPixelPosition.y / Screen.height);
    }

}
