using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamera : MonoBehaviour
{
    public Transform target;
    public float lerpSpeed = 5f; // 控制跟隨的平滑度
    public Vector3 offset = new Vector3(3f, 0f, -10f);

    void LateUpdate() // 必須用 LateUpdate [cite: 33]
    {
        if (target == null) return;

        // 目標位置：只追隨 X 軸，Y 與 Z 保持攝影機原有的
        Vector3 targetPos = new Vector3(target.position.x + offset.x, transform.position.y, transform.position.z);
        
        // 使用 Lerp 進行平滑插值，數字越小越慢（越不抖）
        transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
    }
}