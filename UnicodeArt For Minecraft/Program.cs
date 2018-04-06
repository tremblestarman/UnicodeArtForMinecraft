using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;

namespace UnicodeArt_For_Minecraft
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            Console.WriteLine("Input:");
            var command = Console.ReadLine();
            var commands = command.Split(' ').ToList();
            var charlist = new List<Chars.Achar>();
            var graylist = new List<string>();

            if (commands.Contains("-refresh:glyphs"))
            {
                charlist = new Chars().CharList;
                File.WriteAllText(Application.StartupPath + "\\glyphs.json", JsonConvert.SerializeObject(charlist));
            }
            else if (commands.Contains("-refresh:gray"))
            {
                if (charlist.Count == 0) charlist = JsonConvert.DeserializeObject<List<Chars.Achar>>(File.ReadAllText(Application.StartupPath + "\\glyphs.json"));
                charlist = charlist.OrderBy(c => c.gray).ToList();
                var _graylist = new string[256];
                for (int g = 0; g < 256; g++)
                {
                    var t = charlist.Find(c => c.gray == g && c.to == 6 && c.width == 6);
                    if (t != null) _graylist[g] = t.character.ToString();
                    else if (commands.Contains("-fgl")) // Fill grayList
                    {
                        for (int m = g; m >= 0; m--)
                        {
                            var t0 = charlist.Find(c => c.gray == m && c.to == 6 && c.width == 6);
                            if (t0 != null)
                            {
                                _graylist[g] = t0.character.ToString();
                                break;
                            }
                        }
                    }
                }
                graylist = _graylist.ToList();
                graylist.RemoveAll(g => g == null);

                File.WriteAllText(Application.StartupPath + "\\gray.json", JsonConvert.SerializeObject(graylist));
            }

            if (graylist.Count == 0) graylist = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Application.StartupPath + "\\gray.json"));

            if (!File.Exists(commands[0].Replace("\"", "")))
            {
                Console.WriteLine("File Not Exist");
                Console.ReadKey();
            }

            try
            {
                var tellraw = new List<Text>();
                var img = new Bitmap(commands[0].Replace("\"", ""));
                for (int y = 0; y < img.Height; y++)
                {
                    var t = new Text();
                    for (int x = 0; x < img.Width; x++)
                    {

                        int G = (img.GetPixel(x, y).R + img.GetPixel(x, y).G + img.GetPixel(x, y).B) / 3;
                        var selectI = ((G + 1) * (graylist.Count) / 256 - 1 >= 0) ? (G + 1) * (graylist.Count) / 256 - 1 : 0;
                        //MessageBox.Show(G + " " + selectI);
                        t.text += graylist[selectI];
                    }
                    tellraw.Add(t);
                }

                Clipboard.SetDataObject("tellraw @p " + JsonConvert.SerializeObject(tellraw), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class Chars
    {
        public List<Achar> CharList = new List<Achar>();
        public Chars()
        {
            var f = new FileInfo(Application.StartupPath + "\\glyphs.txt");
            if (f.Exists == true)
            {
                var chars = File.ReadAllLines(f.FullName);
                foreach (var c in chars)
                {
                    var ch = new Achar();

                    if (c[6] == ' ') continue;
                    else
                    {
                        var eles = c.Split(' ');
                        if (eles[0].Length == 4)
                        // UNICODE (example) = width# (start# to end#)
                        {
                            ch.unicode = eles[0];
                            ch.character = eles[1][1];
                            ch.width = int.Parse(eles[3]);
                            ch.from = int.Parse(eles[4].Replace("(", ""));
                            ch.to = int.Parse(eles[6].Replace(")", ""));

                            //Get Gray
                            var index = Convert.ToInt32(ch.unicode, 16);
                            var img_i = (index / 256).ToString("X2").ToLower();
                            var cha_i = index % 256;
                            var cha_v = (cha_i / 16) * 16;
                            var cha_h = cha_i % 16 * 16;
                            var img = new Bitmap(Application.StartupPath + "\\font\\unicode_page_00.png");
                            if (new FileInfo(Application.StartupPath + "\\font\\unicode_page_" + img_i + ".png").Exists)
                                img = new Bitmap(Application.StartupPath + "\\font\\unicode_page_" + img_i + ".png");
                            else continue;
                            var gray = 0;
                            for (int v = cha_v; v < cha_v + 16; v ++)
                            {
                                for (int h = cha_h; h < cha_h + 16; h++)
                                {
                                    if (img.GetPixel(h,v).A != 0)
                                    {
                                        gray++;
                                    }
                                }
                            }
                            ch.gray = gray;
                            CharList.Add(ch);
                        }
                    }
                }
            }
        }
        public class Achar
        {
            public string unicode { get; set; }
            public char character { get; set; }
            public int width { get; set; }
            public int from { get; set; }
            public int to { get; set; }

            public int gray { get; set; }
        }
    }

    public class Text
    {
        public string text { get; set; }
    }
}