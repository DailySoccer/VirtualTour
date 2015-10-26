using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIContentScreen : UIScreen {

	public Button NextButton;
	public Button PrevButton;
	public ContentCubemapController ContentCubeMapUI;
	public ContentImageController ContentImageUI;
	public ContentModel3DController ContentModel3DUI;
	public ContentVideoController ContentVideoUI;

	public override void Awake () {
		base.Awake ();
		_background = GetComponent<Image>();
	}

	public override void Update () {
		base.Update ();

		if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Open")) {
			if (!_loading) {
				_loading = true;
				OpenContent ();
			}
		}
		else {
			if (_loading) {
				_loading = false;
				CloseContent ();
			}
		}
	}

	void OpenContent() {
		NextButton.gameObject.SetActive(false);
		PrevButton.gameObject.SetActive(false);
		
		if (ContentManager.Instance.ContentNear != null) {
			StartCoroutine(ContentImageUI.ShowContents());
		}
		else if (ContentCubeMap.ContentSelected != null) {
			StartCoroutine(ContentCubeMapUI.ShowContents());
		}
		else if (ContentModels.ContentSelected != null) {
			_background.enabled = false;
			StartCoroutine(ContentModel3DUI.ShowContents());
		}
		else if (ContentVideo.ContentSelected != null) {
			_background.enabled = false;
			StartCoroutine(ContentVideoUI.ShowContents());
		}

		UpdateButtons ();
	}
	
	void CloseContent() {
		if (ContentManager.Instance.ContentNear != null) {
			StartCoroutine(ContentImageUI.HideContents());
		}
		
		if (ContentCubeMap.ContentSelected != null) {
			StartCoroutine(ContentCubeMapUI.HideContents());
		}
		
		if (ContentModels.ContentSelected != null) {
			_background.enabled = true;
			StartCoroutine(ContentModel3DUI.HideContents());
		}

		if (ContentVideo.ContentSelected != null) {
			_background.enabled = true;
			StartCoroutine(ContentVideoUI.HideContents());
		}
	}
	
	public void PrevContent() {
		if (ContentImageUI.gameObject.activeSelf) {
			ContentImageUI.Prev();
		}

		if (ContentModel3DUI.gameObject.activeSelf) {
			ContentModel3DUI.Prev();
		}

		UpdateButtons();
	}

	public void NextContent() {
		if (ContentImageUI.gameObject.activeSelf) {
			ContentImageUI.Next();
		}

		if (ContentModel3DUI.gameObject.activeSelf) {
			ContentModel3DUI.Next();
		}

		UpdateButtons();
	}
	
	void UpdateButtons() {
		if (ContentImageUI.gameObject.activeSelf) {
			PrevButton.gameObject.SetActive(!ContentImageUI.Empty && !ContentImageUI.IsFirst);
			NextButton.gameObject.SetActive(!ContentImageUI.Empty && !ContentImageUI.IsLast);
		}

		if (ContentModel3DUI.gameObject.activeSelf) {
			PrevButton.gameObject.SetActive(!ContentModel3DUI.Empty && !ContentModel3DUI.IsFirst);
			NextButton.gameObject.SetActive(!ContentModel3DUI.Empty && !ContentModel3DUI.IsLast);
		}
	}

	private bool _loading = false;
	private Image _background;
}
