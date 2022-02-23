using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTransitioner : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {

	public bool overrideDefaultColor = false;
	public Color32 normalColor = Color.white;
	public Color32 hoverColor = Color.gray;
	public Color32 downColor = Color.white;

	private Color32 _defaultColor;
	private Image _image = null;

	private void Awake() {
		_image = GetComponent<Image>();
		_defaultColor = _image.color;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		if (overrideDefaultColor) {
			_image.color = hoverColor;
		} else
			_image.color = GetMultipliedColor32(_defaultColor, hoverColor);
	}

	public void OnPointerExit(PointerEventData eventData) {
		if (overrideDefaultColor) {
			_image.color = normalColor;
		} else
			_image.color = GetMultipliedColor32(_defaultColor, normalColor);
	}

	public void OnPointerDown(PointerEventData eventData) {
		if (overrideDefaultColor) {
			_image.color = downColor;
		} else
			_image.color = GetMultipliedColor32(_defaultColor, downColor);
	}

	public void OnPointerUp(PointerEventData eventData) {
		
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (overrideDefaultColor) {
			_image.color = hoverColor;
		} else
			_image.color = GetMultipliedColor32(_defaultColor, hoverColor);
	}

	Color GetMultipliedColor32(Color a, Color b) {
		return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
	}
}
