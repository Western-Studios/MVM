using UnityEngine;

public class AnimationOffset : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float offset = 0f;
    [SerializeField] private bool randomize = false;

    private void Start()
    {
        float t = randomize ? Random.value : offset;
        GetComponent<Animator>().Play(0, 0, t);
    }
}
