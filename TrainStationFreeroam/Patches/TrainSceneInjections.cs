using Cinemachine;
using KinematicCharacterController;
using Platformer;
using TrainStationFreeroam.Components;
using UnityEngine;

namespace TrainStationFreeroam.Patches
{
    public static class TrainSceneInjections
    {
        public static void Inject()
        {

            // Disable exit collider 
            foreach (BoxCollider collider in Object.FindObjectsOfType<BoxCollider>())
            {
                if (collider.gameObject.name == "Collider (11)")
                {
                    collider.gameObject.SetActive(false);
                    break;
                }
            }

            // Character sprinting
            KinematicCharacterMotor character = Object.FindObjectOfType<KinematicCharacterMotor>();
            character.gameObject.AddComponent<CharacterSprinter>();

            // Create custom camera
            GameObject newCamObject = new GameObject();
            CinemachineVirtualCamera newCam = newCamObject.AddComponent<CinemachineVirtualCamera>();
            newCam.Priority = 2;
            newCam.m_Lens.FieldOfView = 40;
            newCam.transform.position = character.transform.position;
            newCamObject.AddComponent<GeneralFollowCam>();
            

            // Get parking lot camera trigger, override to include our custom camera.
            foreach (OverrideCameraTrigger camOverride in Object.FindObjectsOfType<OverrideCameraTrigger>())
            {
                if (camOverride.gameObject.name == "ParkingLotCameraTrigger")
                {
                    camOverride.cartOnExit = null;
                    camOverride.vCamOnExit = newCam;
                }
                if (camOverride.gameObject.name == "ParkingLotCameraTrigger (9)")
                {
                    camOverride.followPath = false;
                    camOverride.cart = null;
                    camOverride.vCam = newCam;
                }
            }
        }
    }
}