using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class CameraController : MonoBehaviour
    {
        public GameObject Target;

        public float MoveSpeed;

        private Vector3 targetPosition;

        public void Update()
        {
            if(Target == null)
                return;

            targetPosition = new Vector3(
                Target.transform.position.x, 
                Target.transform.position.y, 
                transform.position.z);

            transform.position = Vector3.Lerp(transform.position, targetPosition, MoveSpeed * Time.deltaTime);
        }
    }
}