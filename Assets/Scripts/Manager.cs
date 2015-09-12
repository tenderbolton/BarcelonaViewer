using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


public class Manager : MonoBehaviour {

	public GameObject cylinderShortPrefab = null;
	public GameObject cylinderLongPrefab = null;
	private List<Pixel> pixels;
	private List<GameObject> pixelsGO;
	private XmlDocument xmlDoc = null;	
	private string configFileName = "config.xml";
	private bool loadOK = false;
	private UDP_DTFrameReceiver commReceiver;


	// Use this for initialization
	void Start () {

		commReceiver = this.gameObject.GetComponent<UDP_DTFrameReceiver> ();

		pixels = new List<Pixel> ();
		pixelsGO = new List<GameObject> ();
		loadXML ();
		if (loadOK) {
			loadConfigXML ();
		}
	}
	
	// Update is called once per frame
	void Update () {

		DTFrame currentFrame = commReceiver.getLatestFrame ();
		if (currentFrame != null) {
			this.processDTFrame(currentFrame);
		}

	}

	void loadXML(){
		xmlDoc = new XmlDocument();
		
		//Debug.Log (" Persisten : " + Application.persistentDataPath);
		//Debug.Log (" Get path : " + getPath());
		
		if(System.IO.File.Exists(getPath()))
		{
			Debug.Log("CONFIG FILE FOUND !!!");
			xmlDoc.LoadXml(System.IO.File.ReadAllText(getPath()));
			loadOK=true;
		}
		else
		{
			Debug.Log("CONFIG FILE NOT FOUND !!!");
			loadOK = false;
			//textXml = (TextAsset)Resources.Load(this.configFileName, typeof(TextAsset));
			//xmlDoc.LoadXml(textXml.text);
		}
	}

	private string getPath(){
		
		#if UNITY_EDITOR
		return Application.dataPath + "/Resources/" + configFileName;
		#elif UNITY_ANDROID
		return Application.persistentDataPath + "/" + configFileName;
		#elif UNITY_IPHONE
		return GetiPhoneDocumentsPath() + "/" + configFileName;
		#else
		return Application.dataPath + "/" + configFileName;
		#endif
		
	}

	void loadConfigXML(){
		if (xmlDoc != null) {
			
			//local configuration
			foreach (XmlElement node in xmlDoc.SelectNodes("Configuration/FrameConf/Pixel")) {

				string pixelType = "";
				Vector3 front = new Vector3();
				Vector3 up = new Vector3();
				Vector3 pos = new Vector3();

				foreach (XmlElement nodeRender in node.SelectNodes("Render")){
					pixelType = nodeRender.GetAttribute ("mesh");
					foreach (XmlElement nodeFront in nodeRender.SelectNodes("Front")){
						front.x = float.Parse(nodeFront.GetAttribute ("x"));
						front.y = float.Parse(nodeFront.GetAttribute ("y"));
						front.z = float.Parse(nodeFront.GetAttribute ("z"));
					}
					foreach (XmlElement nodeUp in nodeRender.SelectNodes("Up")){
						up.x = float.Parse(nodeUp.GetAttribute ("x"));
						up.y = float.Parse(nodeUp.GetAttribute ("y"));
						up.z = float.Parse(nodeUp.GetAttribute ("z"));
					}
					foreach (XmlElement nodePos in nodeRender.SelectNodes("Position")){
						pos.x = float.Parse(nodePos.GetAttribute ("x"))/100.0f;
						pos.y = float.Parse(nodePos.GetAttribute ("y"))/100.0f;
						pos.z = float.Parse(nodePos.GetAttribute ("z"))/100.0f;
					}
				}

				front.Normalize();
				up.Normalize();

				Quaternion rot = Quaternion.FromToRotation(new Vector3(1.0f,0.0f,0.0f), front);

				//Debug.Log ("configuring local settings");
				int pixelId = int.Parse(node.GetAttribute ("id"));
								
				byte r = byte.Parse (node.GetAttribute ("r"));
				byte g = byte.Parse (node.GetAttribute ("g"));
				byte b = byte.Parse (node.GetAttribute ("b"));
				byte a = byte.Parse (node.GetAttribute ("a"));
				
				Color32 pixelColor = new Color32 (r, g, b, a);

				GameObject newPixel= null;

				if(pixelType.CompareTo("cylinderShort") == 0){
					newPixel = (GameObject)Instantiate(cylinderShortPrefab, new Vector3(0.0f,0.0f,0.0f), rot);
				}
				else{
					newPixel = (GameObject)Instantiate(cylinderLongPrefab, new Vector3(0.0f,0.0f,0.0f), rot);
				}

				newPixel.transform.SetParent(this.gameObject.transform);

				Pixel p = newPixel.GetComponent<Pixel>();
				p.initializePixel(pixelId,pixelColor.r,pixelColor.g,pixelColor.b,pos,front,up);

				pixels.Insert(pixels.Count, p);
				pixelsGO.Insert(pixelsGO.Count, newPixel);

			}
		}
	}

	private void processDTFrame(DTFrame currentFrame){

		for (int i = 0; i<currentFrame.pixels.Length; i++) {
			this.pixels[i].setColor(currentFrame.pixels[i]);	
		}

	}
}
