using UnityEngine;

namespace StarterAssets
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactDistance = 2.0f;
        [SerializeField] private LayerMask interactableLayer;

        private Camera _mainCamera;
        private StarterAssetsInputs _input;
        private Interactable _lastDetectedInteractable;

        private void Start()
        {
            _mainCamera = Camera.main;
            _input = GetComponent<StarterAssetsInputs>();
        }

        private void Update()
        {
            PerformRaycast();
        }

        void PerformRaycast()
        {
        
            Ray cameraRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            RaycastHit hit;

            Vector3 rayStartPoint = transform.position + Vector3.up * 1.0f;

            Debug.DrawRay(rayStartPoint, cameraRay.direction * interactDistance, Color.red);

            if (Physics.Raycast(rayStartPoint, cameraRay.direction, out hit, interactDistance, interactableLayer))
            {
                Interactable interactable = hit.collider.GetComponent<Interactable>();

                if (interactable != null)
                {
                    if (_lastDetectedInteractable != interactable)
                    {
                        Debug.Log(interactable.GetInteractionMessage());
                        _lastDetectedInteractable = interactable;
                    }

                    if (_input != null && _input.interact)
                    {
                        Debug.Log($"{interactable.name} 상호작용 성공!");
                        _input.interact = false;
                    }
                }
                else
                {
                    ResetInteraction();
                }
            }
            else
            {
                ResetInteraction();
            }
        }

        void ResetInteraction()
        {
            if (_lastDetectedInteractable != null)
            {
                Debug.Log("대상 사라짐");
                _lastDetectedInteractable = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_mainCamera == null) return;
            Gizmos.color = Color.yellow;
            Vector3 rayStartPoint = transform.position + Vector3.up * 1.5f;
            Gizmos.DrawLine(rayStartPoint, rayStartPoint + _mainCamera.transform.forward * interactDistance);
        }
    }
}