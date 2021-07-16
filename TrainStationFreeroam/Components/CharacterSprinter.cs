using System;
using Platformer.Components;
using UnityEngine;

namespace TrainStationFreeroam.Components
{
    public class CharacterSprinter : MonoBehaviour
    {
        private float _regularSpeed;
        private float _sprintSpeed;
        private BaseCharacterController3D _controller;
        private void Awake()
        {
            _controller = GetComponent<BaseCharacterController3D>();
            _regularSpeed = _controller.maxGroundMoveSpeed;
            _sprintSpeed = _regularSpeed * 2.5f;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Joystick1Button4))
            {
                // Sprint
                _controller.maxGroundMoveSpeed = _sprintSpeed;
            }
            else
            {
                _controller.maxGroundMoveSpeed = _regularSpeed;
            }
        }
    }
}