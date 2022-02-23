using UnityEngine;
using UnityEngine.EventSystems;

public class VRInputModule : BaseInputModule {
	public Camera cam;

	private GameObject _currentObject = null;
	PointerEventData _data = null;
	private bool keyDown = false;

	protected override void Awake() {
		base.Awake();

		_data = new PointerEventData(eventSystem);
	}

	public override void Process() {
		_data.Reset();
		_data.position = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);

		eventSystem.RaycastAll(_data, m_RaycastResultCache);
		_data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
		_currentObject = _data.pointerCurrentRaycast.gameObject;

		m_RaycastResultCache.Clear();

		HandlePointerExitAndEnter(_data, _currentObject);

		if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.D)) {
			keyDown = true;
			ProcessPress(_data);
		}
		if (!Input.anyKey && keyDown) {
			keyDown = false;
			ProcessRelease(_data);
		}
	}

	public PointerEventData GetData() {
		return _data;
	}

	private void ProcessPress(PointerEventData data) {
		//Debug.Log("Button Pressed");
		data.pointerPressRaycast = data.pointerCurrentRaycast;
		GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(_currentObject, data, ExecuteEvents.pointerDownHandler);
		if (newPointerPress == null) {
			newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(_currentObject);
		}

		data.pressPosition = data.position;
		data.pointerPress = newPointerPress;
		data.rawPointerPress = _currentObject;
	}

	private void ProcessRelease(PointerEventData data) {
		//Debug.Log("Button Released");
		ExecuteEvents.ExecuteHierarchy(_currentObject, data, ExecuteEvents.pointerUpHandler);
		GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(_currentObject);
		if (data.pointerPress == pointerUpHandler) {
			ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
		}
		eventSystem.SetSelectedGameObject(null);

		data.pressPosition = Vector2.zero;
		data.pointerPress = null;
		data.rawPointerPress = null;
	}
}
