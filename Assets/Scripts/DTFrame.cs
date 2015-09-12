using UnityEngine;
using System.Collections;

[System.Serializable]
public class DTFrame{

	public int pixelQuantity = 90; //fixed length for Barcelona.
	public int currentIndex = 0;
	public Color32[] pixels;

	public DTFrame(){
		
		this.pixels = new Color32[pixelQuantity];
		
	}

	public void addPixel(Color32 pix){
		this.pixels [currentIndex] = pix;
		this.currentIndex += 1;
	}

}
