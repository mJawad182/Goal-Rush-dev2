using DG.Tweening;
using FStudio.Events;
using FStudio.UI.Events;
using FStudio.UI.MatchThemes;
using UnityEngine;
using UnityEngine.EventSystems;

public class AutoStartMatch : MonoBehaviour
{
    [SerializeField] private float startAfter = 3f;

    private void Start()
    {
        DOVirtual.DelayedCall(startAfter, () =>
        {
            EventManager.Trigger<LoadingEvent>(null);
            UpcomingMatchPanel UpcomingMatchPanelComponent = GetComponent<UpcomingMatchPanel>();
            UpcomingMatchPanelComponent?.StartMatch();
        });
    }
}
