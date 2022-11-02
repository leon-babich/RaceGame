using UnityEngine;

public class TestGizmos : MonoBehaviour
{
    public Transform[] pathElements;
    private void OnDrawGizmos()
    {
        if (pathElements == null) return;

        Gizmos.color = Color.red;
        foreach(var element in pathElements) {
            Gizmos.DrawSphere(element.position, 2);
        }

        if (pathElements.Length < 2) return;

        Gizmos.color = Color.white;
        for (int i=1; i<pathElements.Length; i++) {
            Gizmos.DrawLine(pathElements[i-1].position, pathElements[i].position);
        }
    }
}
