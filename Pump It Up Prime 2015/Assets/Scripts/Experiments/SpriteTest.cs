using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTest : MonoBehaviour {

    public Texture2D image;
    public SpriteRenderer test;

    void Start() {
        Debug.Log(image.width / 200f);
        Vector2 inst = new Vector2(image.width / 200f, image.height / 200f);

        //inst = new Vector2(inst.x / 320f, inst.y / 128f); 
        Debug.Log(inst);

        test.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f,0.5f));
        Debug.Log(test.sprite.pivot);
        //test.transform.position -= new Vector3(test.bounds.extents.x, test.bounds.extents.y, 0);
    }

    void Update() {

    }
}
