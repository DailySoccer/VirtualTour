using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIGameScreen : GUIScreen {

	public Text RoomTitle;
	
	public override void Awake () {
		base.Awake ();
		RoomManager.Instance.OnSceneReady += HandleOnSceneChange;
	}
	
	public override void Start () {
		base.Start ();
		_viewContentButton = GameObject.Find("View Content Button").transform.FindChild("Button").gameObject;
		UpdateRoomTitle();
	}

	void HandleOnSceneChange () {
		UpdateRoomTitle();
	}

	void UpdateRoomTitle() {
		if (RoomManager.Instance.Room != null) {
			RoomTitle.text = RoomManager.Instance.Room.Name;
		}
	}
	
	void OnEnable() {
	}
	
	public override void OpenWindow() {
		GameObject titleObj = GameObject.Find ("Top Center Menu").gameObject;
		if (titleObj != null) {
			titleObj.GetComponent<Animator>().SetBool("IsOpen", false);
		}
	}
	
	public override void CloseWindow() {
		GameObject titleObj = GameObject.Find ("Top Center Menu").gameObject;
		if (titleObj != null) {
			titleObj.GetComponent<Animator>().SetBool("IsOpen", true);
		}
	}
	
	public override void Update () {
		base.Update ();
		
		bool activate = NeedViewButton;
		
		if (_viewContentButton != null &&
		    _viewContentButton.activeSelf != activate) {

			_viewContentButton.SetActive(activate);
			
			if (activate) {
				_viewContentButton.transform.localPosition = Vector3.zero;
				_viewContentTween = Go.from (
					_viewContentButton.transform, 2, new GoTweenConfig()
					.position(_viewContentButton.transform.position + new Vector3(300,0,0))
					.setEaseType(GoEaseType.ElasticOut)
					);
			}
		}
	}
	
	bool NeedViewButton {
		get {
			return 	ContentManager.Instance.ContentNear != null || 
					ContentCubeMap.ContentSelected != null		||
					ContentModels.ContentSelected != null 		||
					ContentVideo.ContentSelected != null;
		}
	}
	
	GameObject _viewContentButton;
	protected AbstractGoTween _viewContentTween;
}
