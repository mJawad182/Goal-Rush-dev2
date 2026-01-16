using DG.Tweening;
using FStudio.Events;
using FStudio.UI.Events;
using FStudio.UI.MatchThemes;
using FStudio.UI.Panels;
using UnityEngine;
using UnityEngine.EventSystems;

public class AutoMoveToMatchMaking : MonoBehaviour
{
    private MainMenuPanel mainMenuPanelComponent;

    private void Awake()
    {
        mainMenuPanelComponent = GetComponent<MainMenuPanel>();
    
    }

    private void Start()
    {
        EventManager.Trigger(new LoadingEvent());

        PlayerPrefs.SetInt("SETTING_SIDE", 1);
        PlayerPrefs.SetInt("SETTING_AILEVEL", 0);

        DOVirtual.DelayedCall(Time.deltaTime,() =>
        {
            mainMenuPanelComponent.Play();
        });
    }
}