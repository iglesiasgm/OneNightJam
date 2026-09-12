using UnityEngine;

public class TrainDerailAnimation : MonoBehaviour
{
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private float forceAmplitude;
    public GameObject trainPhysicsObject;

    public void BeginAnimation(Transform trainTransform, float speed)
    {
        var updatePos = trainTransform.transform.position + spawnOffset;
        
        GameObject impostor = Instantiate(trainPhysicsObject, updatePos, trainTransform.rotation);
        Rigidbody trainRb = impostor.GetComponent<Rigidbody>();
        trainRb.AddForce(trainTransform.transform.forward * (speed * forceAmplitude), ForceMode.Impulse);
    }
}
