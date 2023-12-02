using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


namespace KaizenApp
{
    public class Cube : MonoBehaviour
    {
        private float _targetScale = 1f;
        private Vector3 _scaleVelocity;
        private Quaternion _targetRotation;

        private void OnEnable()
        {
            CubeMainMenuView.ScaleChanged += OnScaleChanged;
            CubeMainMenuView.SpinClicked += OnSpinClicked;
        }

       

        private void OnDisable()
        {
            CubeMainMenuView.ScaleChanged -= OnScaleChanged;
            CubeMainMenuView.SpinClicked -= OnSpinClicked;
        }

        private void OnScaleChanged(float scale)
        {
            _targetScale = scale;
        }

        private void OnSpinClicked()
        {
           _targetRotation = transform.rotation * Quaternion.Euler(Random.insideUnitSphere * 360);
        }

        private void Update()
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, 
                _targetScale * Vector3.one, ref _scaleVelocity, 0.15f);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * 5);
        }

    }

}
