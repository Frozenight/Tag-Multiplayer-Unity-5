using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Cinemachine;
using UnityEngine.UI;

public class UIVirtualTouchZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] CinemachineFreeLook cineCam;
    [SerializeField] float SenstivityX = 2f;
    [SerializeField] float SenstivityY = 2f;

    [System.Serializable]
    public class Event : UnityEvent<Vector2> { }

    [Header("Rect References")]
    public RectTransform containerRect;
    public RectTransform handleRect;

    [Header("Settings")]
    public bool clampToMagnitude;
    public float magnitudeMultiplier = 1f;
    public bool invertXOutputValue;
    public bool invertYOutputValue;

    //Stored Pointer Values
    private Vector2 pointerDownPosition;
    private Vector2 currentPointerPosition;

    [Header("Output")]
    public Event touchZoneOutputEvent;

    private float FinalValueY = 0;
    private float FinalValueX = 0;

    private bool canDrag = false;

    void Start()
    {
        FinalValueX = cineCam.m_XAxis.Value;
        FinalValueY = cineCam.m_YAxis.Value;
        SetupHandle();
    }

    public void TurnOnScreen()
    {
        GetComponent<Image>().raycastTarget = true;
    }
    public void TurnOffScreen()
    {
        GetComponent<Image>().raycastTarget = false;
    }

    public void TurnOffMobileScreen()
    {
        cineCam.m_XAxis.m_InputAxisName = null;
        cineCam.m_YAxis.m_InputAxisName = null;
    }


    private void SetupHandle()
    {
        if(handleRect)
        {
            SetObjectActiveState(handleRect.gameObject, false); 
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (FinalValueX != cineCam.m_XAxis.Value)
            cineCam.m_XAxis.Value = FinalValueX;
        if (FinalValueY != cineCam.m_YAxis.Value)
            cineCam.m_YAxis.Value = FinalValueY;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out pointerDownPosition);

        if(handleRect)
        {
            canDrag = true;
            SetObjectActiveState(handleRect.gameObject, true);
            UpdateHandleRectPosition(pointerDownPosition);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {

        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out currentPointerPosition);
        
        Vector2 positionDelta = GetDeltaBetweenPositions(pointerDownPosition, currentPointerPosition);

        Vector2 clampedPosition = ClampValuesToMagnitude(positionDelta);
        
        Vector2 outputPosition = ApplyInversionFilter(clampedPosition);

        OutputPointerEventValue(outputPosition * magnitudeMultiplier);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDownPosition = Vector2.zero;
        currentPointerPosition = Vector2.zero;

        OutputPointerEventValue(Vector2.zero);

        if(handleRect)
        {
            canDrag = false;
            SetObjectActiveState(handleRect.gameObject, false);
            UpdateHandleRectPosition(Vector2.zero);
        }
        FinalValueX = cineCam.m_XAxis.Value;
        FinalValueY = cineCam.m_YAxis.Value;
    }

    void OutputPointerEventValue(Vector2 pointerPosition)
    {
        if (!canDrag)
            return;
        cineCam.m_XAxis.Value += pointerPosition.x * 200 * SenstivityX * Time.deltaTime;
        cineCam.m_YAxis.Value += pointerPosition.y * SenstivityY * Time.deltaTime;
    }

    void UpdateHandleRectPosition(Vector2 newPosition)
    {
        handleRect.anchoredPosition = newPosition;
    }

    void SetObjectActiveState(GameObject targetObject, bool newState)
    {
        targetObject.SetActive(newState);
    }

    Vector2 GetDeltaBetweenPositions(Vector2 firstPosition, Vector2 secondPosition)
    {
        return secondPosition - firstPosition;
    }

    Vector2 ClampValuesToMagnitude(Vector2 position)
    {
        return Vector2.ClampMagnitude(position, 1);
    }

    Vector2 ApplyInversionFilter(Vector2 position)
    {
        if(invertXOutputValue)
        {
            position.x = InvertValue(position.x);
        }

        if(invertYOutputValue)
        {
            position.y = InvertValue(position.y);
        }

        return position;
    }

    float InvertValue(float value)
    {
        return -value;
    }
    
}
