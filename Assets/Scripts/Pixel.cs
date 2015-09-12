using UnityEngine;
using System.Collections;

public class Pixel : MonoBehaviour {


	int pixelId;
	Color32 color;
	Vector3 position;
	Vector3 front;
	Vector3 up;
	MeshRenderer pixelRenderer;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void initializePixel(int _id, byte _r, byte _g, byte _b, Vector3 _position ,Vector3 _front, Vector3 _up){
		this.pixelId = _id;
		color = new Color32(_r,_g,_b,255);
		this.position = new Vector3 (_position.x, _position.y, _position.z);
		this.front = new Vector3 (_front.x, _front.y, _front.z);
		this.up = new Vector3 (_up.x, _up.y, _up.z);
		this.pixelRenderer = this.gameObject.GetComponent <MeshRenderer> ();

		this.gameObject.transform.localPosition = this.position;
		this.setColor (this.color);
	}

	public void setColor(Color32 newColor){
		this.color.r = newColor.r;
		this.color.g = newColor.g;
		this.color.b = newColor.b;

		this.pixelRenderer.material.color = this.color;
	}
}
