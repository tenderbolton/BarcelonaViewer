using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDP_DTFrameReceiver : MonoBehaviour {

	//constants
	public const int BUFF_LEN = 1024;
	
	// read Thread
	Thread receiveThread;
	
	// udpclient object
	UdpClient client;
	
	// port number
	public int port = 11999;

	List<DTFrame> framesBuffer;
	//SHA_ToDo declare BUFF_LEN constant
	byte[] dataBuffer;
	private int dataBufferIndex = 0;

	private int sequence = 65535;

	private DTFrame parcialFrame = null;
	private bool buildingParcialFrame = false;
	private int errorQty = 0;
	private int errorWindow = 10000;

	private System.Object lockObject;
	
	// Use this for initialization
	void Start () {
		this.lockObject = new System.Object ();
		dataBuffer = new byte[BUFF_LEN];
		this.framesBuffer = new List<DTFrame> ();
		init();
	}

	void init(){
		print("UDPSend.init()");
		
		// define port
		port = 11999;
		
		// status
		print("Sending to 127.0.0.1 : "+port);
		print("Test-Sending to this Port: nc -u 127.0.0.1  "+port+"");

		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
	}
	
	// Unity Update Function
	void Update()
	{
		// check button "s" to abort the read-thread
		if (Input.GetKeyDown("q"))
			stopThread();
	}

	// Unity Application Quit Function
	void OnApplicationQuit()
	{
		stopThread();
	}


	// Stop reading UDP messages
	private void stopThread()
	{
		if (receiveThread.IsAlive)
		{
			receiveThread.Abort();
		}
		client.Close();
	}

	// receive thread
	private  void ReceiveData()
	{
		
		client = new UdpClient(port);
		while (true)
		{
			
			try
			{
				if(client.Available > 0){
					// Bytes
					IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = client.Receive(ref anyIP);

					if((this.dataBufferIndex + data.Length)<BUFF_LEN){
						//SHA_ToDo find memcpy in C#
						//memcpy(&(this->binaryBuffer)+this->intBinaryBuffIndex,&buff,receivedBytes);
						Array.Copy(data,0,this.dataBuffer, this.dataBufferIndex,data.Length);
						//aumento el index del buffer
						this.dataBufferIndex += data.Length;
					}

					//Cuando hay OVERFLOW, se pierden paquetes--> se da el mismo tratamiento que frente a un error
					//--> se resincroniza mandando mensaje de error

					if(Monitor.TryEnter(this.lockObject)){
					// intento parsear el primer paquete que figura en el buffer principal
						int firstOcurrence = -1;
						firstOcurrence = searchPacketDelimiter(0);

						if(firstOcurrence == 0){

							int secondOccurence = searchPacketDelimiter(1);

							// packet starts at index 8
							// packet header is
							//
							/*
							*
							uint8_t  crc; //1 bytes
						    uint8_t  id[8]; //9 bytes
						    uint8_t  ver; // 10 bytes
						    uint8_t  sequenceHi; //11 bytes
						    uint8_t  sequence; //12 bytes
						    uint8_t  data;
						 	* 
						 	*/
							if(secondOccurence>0){
								//We have a whole packet
								DTFrame newFrame = new DTFrame();
								//parsing frame

								int cont = 0;
								int dataIndex = firstOcurrence + 8 + 12;
								for (int q=dataIndex; q<secondOccurence; q+=3){
									byte cR= this.dataBuffer[q];
									byte cG= this.dataBuffer[q + 1];
									byte cB= this.dataBuffer[q + 2];
									
									//we pass 0 as ledTypeId since is dummy information
									Color32 newPixel = new Color32(cR, cG, cB, 255);
									newFrame.addPixel (newPixel);
									cont++;
								}
								
								this.framesBuffer.Add(newFrame);

								clearBinaryBuffer(secondOccurence + 1);
							}

						}
						else{
							if(firstOcurrence>0){
								clearBinaryBuffer(firstOcurrence + 1);
							}
						}
						Monitor.Exit(lockObject);
					}
					// Bytes UTF8
					//string text = Encoding.UTF8.GetString(data);

					//print(">> " + text);
				}

				Thread.Sleep(20);
				
			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}
	}

	public void clearBinaryBuffer(int size){
		int totalSize = size; //header + data
		int bufferSwapIndex = 0;
		for (int i = totalSize - 1; i<(BUFF_LEN-1); i++){
			this.dataBuffer[bufferSwapIndex] = this.dataBuffer[i];
			bufferSwapIndex++;
		}
		this.dataBufferIndex = this.dataBufferIndex - (totalSize - 1);
	}

	public DTFrame getLatestFrame(){
		DTFrame returningFrame = null;

		if (Monitor.TryEnter (this.lockObject)) {

			if(this.framesBuffer.Count>0){
				returningFrame = this.framesBuffer[framesBuffer.Count-1];
				framesBuffer.Clear ();
			}

			Monitor.Exit(lockObject);
		}

		return returningFrame;
	}

	public int searchPacketDelimiter(int startIndex){
		bool found = false;
		int index = startIndex;

		while (!found && (index + 8) < BUFF_LEN) {
			if (this.dataBuffer [index + 0] == '_' && this.dataBuffer [index + 1] == 'P' && this.dataBuffer [index + 2] == 'A' && 
				this.dataBuffer [index + 3] == 'C' && this.dataBuffer [index + 4] == 'K' && this.dataBuffer [index + 5] == 'E' &&
				this.dataBuffer [index + 6] == 'T' && this.dataBuffer [index + 7] == '_') {
				found = true;
			}
			else{
				index ++;
			}
		}
			
		if (!found) {
			index = -1;
		}

		return index;
	}
}
