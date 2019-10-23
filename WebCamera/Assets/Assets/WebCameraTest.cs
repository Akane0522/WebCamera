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
    int x = 1920;
    int y = 1080;
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
        webCamTexture = new WebCamTexture(devices[0].name);
        Debug.Log(webCamTexture);
        GetComponent<Renderer>().material.mainTexture = webCamTexture;
        webCamTexture.Play();

        //Textread();
    }

    void Update()
    {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
        
            if (touch.phase == TouchPhase.Began)
            {
                //print("keydownspace");
                if (webCamTexture != null)
                {
                String file_path0 = Application.dataPath + "/Assets/Resources/SavedScreen";
                String file_path1 = Application.dataPath + "/Assets/Resources/BufScreen";
                String file_path2 = Application.dataPath + "/Assets/Resources/rectScreen";

                //SaveToPNGFile(webCamTexture.GetPixels(), Application.dataPath + "/../SavedScreen" + num + ".png");
                while (!SaveToPNGFile(webCamTexture.GetPixels(), file_path0 + num + ".png"))
                    {

                    }
                    Process(file_path0 + num + ".png");
                    create_rect(rectpoint);
                    if(num == 0){
                        cut_rect(rectpoint);
                    }
                    getCenterClippedTexture((Texture2D)ReadTexture(file_path0 + num + ".png", x, y));
                    num++;

                    if (num == 2)
                    {
                        Compare(file_path2 + "0.png", file_path2 + "1.png", file_path1 + "2.png");
                        ExperiencePoint(difnum);
                    // print("Save picture. Diffnum : " + difnum + " Exp : " + "Level : " + level);
                        num = 0;
                    }
                }
            }
        }
    }

    bool SaveToPNGFile(UnityEngine.Color[] texData, string filename)
    {
        Texture2D takenPhoto = new Texture2D(x, y, TextureFormat.ARGB32, false);

        takenPhoto.SetPixels(texData);
        takenPhoto.Apply();

        byte[] png = takenPhoto.EncodeToPNG();
        Destroy(takenPhoto);

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(filename, png);

        return true;
    }

       bool RectSaveToPNGFile(UnityEngine.Color[] texData, string filename)
    {
        Texture2D takenPhoto = new Texture2D(rectwidth, rectheight, TextureFormat.ARGB32, false);

        takenPhoto.SetPixels(texData);
        takenPhoto.Apply();

        byte[] png = takenPhoto.EncodeToPNG();
        Destroy(takenPhoto);

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(filename, png);

        return true;
    }

    // 四角形で囲んだ部分を切り取り、保存する
    Texture2D getCenterClippedTexture(Texture2D texture)
    {
        UnityEngine.Color[] pixel;
        Texture2D clipTex = new Texture2D(rectwidth, rectheight);

        // GetPixels (x, y, width, height) で切り出せる
        pixel = texture.GetPixels(drawrect.X, drawrect.Y, rectwidth, rectheight);

        clipTex.SetPixels(pixel);
        clipTex.Apply();

        String file_path = Application.dataPath + "/Assets/Resources/rectScreen";

        RectSaveToPNGFile(clipTex.GetPixels(), file_path + num + ".png");
        return clipTex;
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

    /// ペーパースキャナーで四角を算出したが長方形でないため。長方形のサイズを決定する。
    private void create_rect(Point[] pointrect){
        //decide rect size
        if(pointrect[0].X < pointrect[3].X){
            drawrect.X = pointrect[0].X;
        }else{
            drawrect.X = pointrect[3].X;
        }

        if(pointrect[0].Y < pointrect[1].Y){
            drawrect.Y = pointrect[0].Y;
        }else{
            drawrect.Y = pointrect[1].Y;
        }
    }
    private void cut_rect(Point[] pointrect){
        int h1 = pointrect[3].Y - pointrect[0].Y;
        int h2 = pointrect[2].Y - pointrect[1].Y;

        if(h1 < h2){
            rectheight = h2;
        }
        else{
            rectheight = h1;
        }

        int w1 = pointrect[1].X - pointrect[0].X;
        int w2 = pointrect[2].X - pointrect[3].X;

        if(w1 < w2){
            rectwidth = w2;
        }
        else{
            rectwidth = w1;
        }

        
        Debug.Log(drawrect.X);
        Debug.Log(drawrect.Y);
        Debug.Log(rectheight);
        Debug.Log(rectwidth);
        

    }

    private PaperScanner scanner = new PaperScanner();

    #region Boring code that combines output image with OpenCV

    /// <summary>
    /// Combines original and processed images into a new twice wide image
    /// </summary>
    /// <param name="original">Source image</param>
    /// <param name="processed">Processed image</param>
    /// <param name="detectedContour">Contour to draw over original image to show detected shape</param>
    /// <returns>OpenCV::Mat image with images combined</returns>
    private Mat CombineMats(Mat original, Mat processed, Point[] detectedContour)
    {
        Size inputSize = new Size(original.Width, original.Height);

        // combine fancy output image:
        // - create new texture twice as wide as input
        // - copy input into the left half
        // - draw detected paper contour over original input
        // - put "scanned", un-warped and cleared paper to the right, centered in the right half
        var matCombined = new Mat(new Size(inputSize.Width, inputSize.Height), original.Type(), Scalar.FromRgb(64, 64, 64));

        // copy original image with detected shape drawn over
        original.CopyTo(matCombined.SubMat(0, inputSize.Height, 0, inputSize.Width));
        if (null != detectedContour && detectedContour.Length > 2)
            matCombined.DrawContours(new Point[][] { detectedContour }, 0, Scalar.FromRgb(255, 255, 0), 3);

        // copy scanned paper without extra scaling, as is
        if (null != processed)
        {
            double hw =inputSize.Width * 0.5;
            double hh = inputSize.Height * 0.5;
            Point2d center = new Point2d(inputSize.Width * 0.5, inputSize.Height * 0.5);
            Mat roi = matCombined.SubMat(
                (int)(center.Y - hh), (int)(center.Y + hh),
                (int)(center.X - hw), (int)(center.X + hw)
            );
            processed.CopyTo(roi);
        }
        
        //四角の座標を参照するポイント型配列をclass内変数に代入しておく。
        rectpoint = scanner.pointing_C;
        
        Debug.Log("point1 x " + scanner.pointing_C[0].X + ", y: " + scanner.pointing_C[0].Y);
        Debug.Log("point2 x " + scanner.pointing_C[1].X + ", y: " + scanner.pointing_C[1].Y);
        Debug.Log("point3 x " + scanner.pointing_C[2].X + ", y: " + scanner.pointing_C[2].Y);
        Debug.Log("point4 x " + scanner.pointing_C[3].X + ", y: " + scanner.pointing_C[3].Y);
        
        return matCombined;
    }

    #endregion
    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        UnityEngine.Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new UnityEngine.Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    byte[] ReadPngFile(string path){
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

        bin.Close();

        return values;
    }

    Texture ReadTexture(string path, int width, int height){
        byte[] readBinary = ReadPngFile(path);

        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(readBinary);

        return texture;
    }

    // Use this for initialization
    public void Process(string filename)
    {
 
        Texture2D inputTexture = duplicateTexture((Texture2D)ReadTexture(filename, x, y));
        Debug.Log(inputTexture);
        // first of all, we set up scan parameters
        // 
        // scanner.Settings has more values than we use
        // (like Settings.Decolorization that defines
        // whether b&w filter should be applied), but
        // default values are quite fine and some of
        // them are by default in "smart" mode that
        // uses heuristic to find best choice. so,
        // we change only those that matter for us
        scanner.Settings.NoiseReduction = 0.7;                                          // real-world images are quite noisy, this value proved to be reasonable
        scanner.Settings.EdgesTight = 0.9;                                              // higher value cuts off "noise" as well, this time smaller and weaker edges
        scanner.Settings.ExpectedArea = 0.2;                                            // we expect document to be at least 20% of the total image area
        scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.Grayscale;   // color -> grayscale conversion mode

        // process input with PaperScanner
        Mat result = null;

        scanner.Input = OpenCvSharp.Unity.TextureToMat(inputTexture);

        // should we fail, there is second try - HSV might help to detect paper by color difference
        if (!scanner.Success)
            // this will drop current result and re-fetch it next time we query for 'Success' flag or actual data
            scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.HueGrayscale;

        // now can combine Original/Scanner image
        result = CombineMats(scanner.Input, scanner.Output, scanner.PaperShape);

        // apply result or source (late for a failed scan)
        //rawImage.texture = OpenCvSharp.Unity.MatToTexture(result);

        // var transform = gameObject.GetComponent<RectTransform>();
        // transform.sizeDelta = new Vector2(result.Width, result.Height);

        Texture2D outputTexture = OpenCvSharp.Unity.MatToTexture(result);
        String file_path = Application.dataPath + "/Assets/Resources/BufScreen";

        SaveToPNGFile(outputTexture.GetPixels(),file_path + num + ".png");
    }
}