using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasJumpVelocity {
    Vector3 jumpVelocity { get; }
}

public class VelocitySpecifiedJumpPoint : MonoBehaviour, IHasJumpVelocity
{
    // Start is called before the first frame update

    public Vector3 targetJumpVelocity;

    public Vector3 jumpVelocity {
        get {
            return targetJumpVelocity;
        }
    }

    void OnDrawGizmos() {

        Color c = Gizmos.color;

        Gizmos.color = new Color(1f, 0, 1f, 1f);
        Gizmos.DrawLine(transform.position, transform.position + targetJumpVelocity);

        Gizmos.color = c;

    }
}
