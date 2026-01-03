using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 這裡之後要拖入你的「方塊臘腸狗」
    public float smoothing = 5f;
    Vector3 offset;

    void Start() {
        if (target != null) offset = transform.position - target.position;
    }

    void LateUpdate() {
        if (target == null) return;
        // 只追蹤 X 軸移動
        Vector3 targetCamPos = new Vector3(target.position.x + offset.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    }
}