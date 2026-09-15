using UnityEngine;

public class TrainDerailAnimation : MonoBehaviour
{
    [SerializeField] private TrainConsist trainConsist;
    
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private float forceAmplitude;
    public GameObject trainPhysicsObject;

    public void BeginAnimation(Transform trainTransform, float speed)
    {
        var forceDirection = trainTransform.forward;
        
        GameObject impostor = PreparePrefab(trainTransform);
        Rigidbody trainRb = impostor.transform.GetChild(0).GetComponent<Rigidbody>();
        
        trainRb.AddForce(forceDirection * (speed * forceAmplitude), ForceMode.Impulse);
    }
    
    private GameObject PreparePrefab(Transform trainTransform)
    {
        // TODO construir el prefab en funcion de los vagones

        //int wagonsCount = trainConsist.GetWagons().Count;
        
        var updatePos = trainTransform.position + spawnOffset;
        GameObject impostor = Instantiate(trainPhysicsObject, updatePos, trainTransform.rotation);
        
        return impostor;
    }
}
