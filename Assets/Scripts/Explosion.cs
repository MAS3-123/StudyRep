using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private void endOfAnimation()
    {
        Destroy(gameObject);
    }

    public void SetAnimationSize(float _sizeX)
    {
        Vector3 scale = transform.localScale;
        scale *= _sizeX / 24.0f;
        transform.localScale = scale;
    }
}
