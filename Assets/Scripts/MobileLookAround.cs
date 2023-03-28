using Cinemachine;
using UnityEngine;

public class MobileLookAround : MonoBehaviour
{
    private Vector2 _lookInput;

    [SerializeField] private float _touchSpeedSensitivityX = 3f;
    [SerializeField] private float _touchSpeedSensitivityY = 3f;

    private string _touchXMapTo = "Mouse X";
    private string _touchYMapTo = "Mouse Y";

    private void Start()
    {
        CinemachineCore.GetInputAxis = GetInputAxis;
    }

    private float GetInputAxis(string axisName)
    {
        //_lookInput = _touchInput.PlayerJoystickOutputVector();

        if (axisName == _touchXMapTo)
            return _lookInput.x / _touchSpeedSensitivityX;

        if (axisName == _touchYMapTo)
            return _lookInput.y / _touchSpeedSensitivityY;
        return Input.GetAxis(axisName);
    }

}
