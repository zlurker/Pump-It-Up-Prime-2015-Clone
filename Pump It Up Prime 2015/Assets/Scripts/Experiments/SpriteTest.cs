using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTest : MonoBehaviour {

    public Texture2D image;
    public SpriteRenderer test;
    // Use this for initialization
    void Start() {
        test.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(-test.transform.localScale.x/4, -test.transform.localScale.y / 4));
    }

    // Update is called once per frame
    void Update() {

    }
}
