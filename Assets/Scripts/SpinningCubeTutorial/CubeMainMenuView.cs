using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class CubeMainMenuView
    : MonoBehaviour
{
    [SerializeField] UIDocument _mainMenuDocument;
    [SerializeField] StyleSheet _mainMenuStyles;
    [SerializeField] float _validateFloat;

    public static event Action<float> ScaleChanged;
    public static event Action SpinClicked;


    private void Start()
    {
        StartCoroutine(Generate());
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        yield return null;
        var root = _mainMenuDocument.rootVisualElement;
        root.Clear();
        root.styleSheets.Add(_mainMenuStyles);

        var container = Create("container");

        var viewBox = Create("view-box", "bordered-box");

        container.Add(viewBox);
        var controlBox = Create("control-box", "bordered-box");

        var spinButton = Create<Button>();
        spinButton.text = "Spin";
        spinButton.clicked += () =>
        {
            SpinClicked?.Invoke();
        };
        controlBox.Add(spinButton);

        var scaleSlider = Create<Slider>();
        scaleSlider.lowValue = 0.5f;
        scaleSlider.highValue = 2f;
        scaleSlider.value = 1f;
        scaleSlider.RegisterValueChangedCallback((evt) =>
        {
            ScaleChanged?.Invoke(evt.newValue);
        });
        controlBox.Add(scaleSlider);

        container.Add(controlBox);

        root.Add(container);

    }

    private VisualElement Create(params string[] className)
    {
        var element = new VisualElement();
        foreach (var name in className)
        {
            element.AddToClassList(name);
        }
        return element;
    }

    private T Create<T>(params string[] className) where T : VisualElement, new()
    {
        var element = new T();
        foreach (var name in className)
        {
            element.AddToClassList(name);
        }

        return new T();
    }


}
