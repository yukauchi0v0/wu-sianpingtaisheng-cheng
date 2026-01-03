using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
    // 如果平台位置落後相機 15 個單位，就毀滅它
    if (transform.position.x < Camera.main.transform.position.x - 15f) {
        Destroy(gameObject);
    }
}
}
