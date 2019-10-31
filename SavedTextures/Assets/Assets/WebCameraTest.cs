using System;
using System.IO;
using UnityEngine;
using System.Drawing.Imaging;
using System.Drawing;
using UnityEngine.UI;
using OpenCvSharp;
using Size = OpenCvSharp.Size;
using Point = OpenCvSharp.Point;

public class WebCameraTest : MonoBehaviour
{

    WebCamTexture webCamTexture;
    int x = 1024;
    int y = 768;
    private int num = 0;
    private int difnum = 0;
    private int exp = 0;
    private int exsum = 0;
    private int level = 1;
    private int userlevel;
    private string username;
    public Point[] rectpoint;
    
    public Point drawrect;
    public int rectwidth, rectheight;

    public GameObject num_object = null; // Textオブジェクト

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webCamTexture = new WebCamTexture(devices[0].name, x, y);
        GetComponent<Renderer>().material.mainTexture = webCamTexture;
        webCamTexture.Play();
    }

      // 更新
    void Update () {

        Text num_text = num_object.GetComponent<Text> ();

        num_text.text = "Num:" + num;
        
        if(num == 2)
        {
            num = 0;
        }

    }

    void SaveToJPGFile(UnityEngine.Color[] texData, string filename)
    {
        Texture2D takenPhoto = new Texture2D(1024, 768, TextureFormat.RGBA32, true);

        takenPhoto.SetPixels(texData);
        takenPhoto.Apply();

        byte[] jpg = takenPhoto.EncodeToJPG();
        Destroy(takenPhoto);

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(filename, jpg);

    }

    public void OnClick()
    {
        String Android_path0 = Application.temporaryCachePath + "/SavedScreen";
        String Android_path1 = Application.temporaryCachePath + "/BufScreen";
        String Android_path2 = Application.temporaryCachePath + "/rectScreen";

        if (webCamTexture != null)
        {
            SaveToJPGFile(webCamTexture.GetPixels(0 , 0, 1024, 768), Android_path0 + num + ".jpg");
            num++;
        }
    }
}