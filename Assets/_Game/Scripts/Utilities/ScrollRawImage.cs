using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScrollRawImage : MonoBehaviour
{
    [SerializeField] private float _x, _y;

     private RawImage _imageToScroll;
    private void Awake()
    {
        _imageToScroll = GetComponent<RawImage>();
    }

    private void Update()
    {
        _imageToScroll.uvRect = new Rect(_imageToScroll.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _imageToScroll.uvRect.size);
    }
}
