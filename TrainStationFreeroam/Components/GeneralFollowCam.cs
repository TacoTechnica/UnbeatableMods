using System;
using KinematicCharacterController;
using Rewired;
using UnityEngine;

namespace TrainStationFreeroam.Components
{
    public class GeneralFollowCam : MonoBehaviour
    {
        private Transform _target;
        private static readonly Vector3 Offset = new Vector3(0, 4, -16);
        private float _rotation = 180;
        private float _zoom = 1;

        private Vector3 _lookTrueTarget;

        private Player rewired;

        private void Awake()
        {
            _target = FindObjectOfType<KinematicCharacterMotor>().transform;
            rewired = ReInput.players.GetPlayer(0);
        }

        private void Update()
        {
            Vector3 trueOffset = Quaternion.AngleAxis(_rotation, Vector3.up) * Offset;
            trueOffset *= _zoom;
            Vector3 trueTarget = _target.position + trueOffset;

            Vector3 lookCenter = _target.position + Vector3.up * 1f;

            _lookTrueTarget += (lookCenter - _lookTrueTarget) * (Time.deltaTime * 3f);

            // Simple easing
            transform.position += (trueTarget - transform.position) * (Time.deltaTime * 3f);
            // Look
            transform.LookAt(_lookTrueTarget);
            
            // Angle
            float rotationOffset = (Input.GetKey(KeyCode.RightArrow)?1:0) - (Input.GetKey(KeyCode.LeftArrow)?1:0);
            rotationOffset += (Input.GetKey(KeyCode.Joystick1Button7) ? 1 : 0) -
                              (Input.GetKey(KeyCode.Joystick1Button6) ? 1 : 0);
            rotationOffset = Mathf.Clamp(rotationOffset, -1, 1);
            _rotation += rotationOffset * Time.deltaTime * 180f;

            // Zoom
            _zoom = Mathf.Max(_zoom, 0.4f);
            float zoomOffset = (Input.GetKey(KeyCode.DownArrow) ? 1 : 0) - (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);
            _zoom += zoomOffset * _zoom * Time.deltaTime;
        }
    }
}
