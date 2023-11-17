using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class CanvasMarker : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 5;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private RaycastHit _touch;
    private Canvas _canvas;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;
    
    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>();
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        _tipHeight = _tip.localScale.y;
    }

    void Update()
    {
        ProcessDrawing();
    }

    private void ProcessDrawing()
    {
        if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight) && _touch.transform.CompareTag("Canvas"))
        {
            HandleCanvasTouch();
        }
        else
        {
            _touchedLastFrame = false;
            _canvas = null;
        }
    }

    private void HandleCanvasTouch()
    {
        if (_canvas == null)
        {
            _canvas = _touch.transform.GetComponent<Canvas>();
        }

        _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

        var x = (int)(_touchPos.x * _canvas.textureSize.x - (_penSize / 2));
        var y = (int)(_touchPos.y * _canvas.textureSize.y - (_penSize / 2));

        if (x < 0 || x >= _canvas.textureSize.x || y < 0 || y >= _canvas.textureSize.y) return;

        DrawPixels(x, y);

        _lastTouchPos = new Vector2(x, y);
        _lastTouchRot = transform.rotation;
        _touchedLastFrame = true;
    }

    private void DrawPixels(int x, int y)
    {
        _canvas.texture.SetPixels(x, y, _penSize, _penSize, _colors);

        if (_touchedLastFrame)
        {
            for (float f = 0.01f; f < 1.00f; f += 0.01f)
            {
                var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                _canvas.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colors);
            }
        }
        
        _canvas.texture.Apply();
    }
}

