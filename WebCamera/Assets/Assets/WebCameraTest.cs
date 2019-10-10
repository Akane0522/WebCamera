using System;
using System.IO;
using UnityEngine;
using System.Drawing.Imaging;
using System.Drawing;

public class WebCameraTest : MonoBehaviour
{

    WebCamTexture webCamTexture;
    int x = 1920;
    int y = 1080;
    int fps = 30;
    private int num = 0;
    private int difnum = 0;
    private int exp = 0;
    private int exsum = 0;
    private int level = 1;
    private int userlevel;
    private string username;

    /*tanipai
        グローバル変数のusername、userlevelをaset/userdata.txt
        の内部から呼び出し格納を行う。
        テキストファイルは1行目がusername,2行目がuserlevelとなっている。
         
    */
    void Textread()
    {
        string[] allText = File.ReadAllLines(Application.dataPath + "/userdata.txt");
        username = allText[0];
        userlevel = Int32.Parse(allText[1]);

        Debug.Log("username :" + username);
        Debug.Log("userlevel :" + userlevel);
    }

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webCamTexture = new WebCamTexture(devices[0].name, x, y, fps);
        print(webCamTexture);
        GetComponent<Renderer>().material.mainTexture = webCamTexture;
        webCamTexture.Play();

        Textread();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("keydownspace");
            if (webCamTexture != null)
            {
                SaveToPNGFile(webCamTexture.GetPixels(), Application.dataPath + "/../SavedScreen" + num + ".png");
                num++;

                if (num == 2)
                {
                    Compare(Application.dataPath + "/../SavedScreen0.png", Application.dataPath + "/../SavedScreen1.png", 
                            Application.dataPath + "/../diffScreen" + num + ".png");
                    ExperiencePoint(difnum);
                   // print("Save picture. Diffnum : " + difnum + " Exp : " + "Level : " + level);
                    num = 0;
                }
            }
        }
    }

    void SaveToPNGFile(UnityEngine.Color[] texData, string filename)
    {
        Texture2D takenPhoto = new Texture2D(x, y, TextureFormat.ARGB32, false);

        takenPhoto.SetPixels(texData);
        takenPhoto.Apply();

        byte[] png = takenPhoto.EncodeToPNG();
        Destroy(takenPhoto);

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(filename, png);
    }

    /// <summary>
    /// 1ピクセルずつ画像を比較して、差分の画像を返す。
    /// </summary>
    /// <param name="bmp1Path">比較する画像1のファイルパス。</param>
    /// <param name="bmp2Path">比較する画像2のファイルパス。</param>
    /// <param name="path">差分画像の保存先となるファイルパス。</param>
    /// <returns>2つの画像が同じであればtrue、そうでなければfalseを返す。</returns>
    void Compare(string bmp1Path, string bmp2Path, string path)
    {
        // 画像を比較する際に「大きい方の画像」のサイズに合わせて比較する。
        Bitmap bmp1 = new Bitmap(bmp1Path);
        Bitmap bmp2 = new Bitmap(bmp2Path);
        int width = Math.Max(bmp1.Width, bmp2.Width);
        int height = Math.Max(bmp1.Height, bmp2.Height);

        Bitmap diffBmp = new Bitmap(width, height);         // 返却する差分の画像。
        System.Drawing.Color diffColor = System.Drawing.Color.Black;                        // 画像の差分に付ける色。
        System.Drawing.Color DiffColor = System.Drawing.Color.White;

        // 全ピクセルを総当りで比較し、違う部分があればfalseを返す。
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                try
                {
                    System.Drawing.Color color1 = bmp1.GetPixel(i, j);
                    System.Drawing.Color color2 = bmp2.GetPixel(i, j);

                    byte R = color1.R;
                    byte G = color1.G;
                    byte B = color1.B;

                    byte r = color2.R;
                    byte g = color2.G;
                    byte b = color2.B;

                    if (R - r < 25 && G - g < 25 && B - b < 25)
                    {
                        diffBmp.SetPixel(i, j, DiffColor);
                    }
                    else
                    {
                        diffBmp.SetPixel(i, j, diffColor);
                        difnum++;
                    }
                }
                catch
                {
                    // 画像のサイズが違う時は、ピクセルを取得できずにエラーとなるが、ここでは「差分」として扱う。
                    diffBmp.SetPixel(i, j, diffColor);
                }
            }
        }
        diffBmp.Save(path, ImageFormat.Png);
    }

    void ExperiencePoint(int d)
    { 
        exp = (int)((d -70000)*0.0001);
        if(exp < 0)
        {
            exp = 0;
        }
        exsum += exp;

        while (exsum >= (level * 10))
        {
            exsum -= (level * 10);
            level++;
        }
        print("Save picture. Diffnum : " + difnum + " Exp : " + exp + " ExpSum : " + exsum + " Level : " + level);

    }
}