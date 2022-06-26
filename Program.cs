using System;
using System.Diagnostics;
using System.IO;

namespace texture_converter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (CheckRequirements())
            {
                string[] sourceFolders = Directory.GetDirectories("texture_sources", "*", SearchOption.AllDirectories);
                foreach (string folder in sourceFolders)
                {
                    ProcessFolder(folder);
                }
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.WriteLine("Done. Press ENTER to exit.");
            Console.ResetColor();
            Console.ReadLine();
        }

        static bool CheckRequirements()
        {
            string[] nvcompress = { "nvcompress.exe", "cudart.dll", "jpeg62.dll", "libpng12.dll", "nvtt.dll", "zlib1.dll" };
            foreach (string filename in nvcompress)
            {
                if (!File.Exists("nvcompress/" + filename))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("NVIDIA Compression tool not found.");
                    return false;
                }
            }
            if (!Directory.Exists("texture_sources"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Folder texture_sources does not exist.");
                return false;
            }
            return true;
        }

        static void ProcessFolder(string folder)
        {
            if (!folder.StartsWith("texture_sources", StringComparison.InvariantCultureIgnoreCase)) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(folder);
            Console.ResetColor();

            string[] sourceFiles = Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly);
            foreach (string fileName in sourceFiles)
            {
                ProcessFile(folder, fileName);
            }
        }

        static void ProcessFile(string folder, string fullName)
        {
            if (File.Exists("texture_converter.dds")) File.Delete("texture_converter.dds");

            if (fullName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                || fullName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                || fullName.EndsWith(".tga", StringComparison.InvariantCultureIgnoreCase))
            {
                // PNG, JPG and TGA are converted using the NVIDIA tool.
                Process nvcompress = new Process();
                nvcompress.StartInfo.FileName = "nvcompress/nvcompress.exe";
                nvcompress.StartInfo.Arguments = "-bc1 " + fullName + " texture_converter.dds";
                nvcompress.StartInfo.RedirectStandardOutput = true;
                nvcompress.Start();
                nvcompress.WaitForExit();
                WriteTexture(folder, fullName);
                return;
            }

            if (fullName.EndsWith(".dds", StringComparison.InvariantCultureIgnoreCase))
            {
                // DDS files are copied as-is and we just attach the header.
                File.Copy(fullName, "texture_converter.dds");
                WriteTexture(folder, fullName);
                return;
            }
        }

        static void WriteTexture(string folder, string fullName)
        {
            if (!File.Exists("texture_converter.dds")) return;
            Console.Write("    " + Path.GetFileName(fullName));

            // Assumes the full path starts with texture_sources and ends with a 3-character extension.
            string textureName = "texture_library" + fullName.Substring(15).Replace("\\", "/").Substring(0, fullName.Length - 19);

            // Read the converted DDS file and get the image dimensions.
            byte[] ddsData = File.ReadAllBytes("texture_converter.dds");
            int ddsWidth = BitConverter.ToInt32(ddsData, 16);
            int ddsHeight = BitConverter.ToInt32(ddsData, 12);
            int[] texFlags = { 0, 0, 0, 844649472 };

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" " + ddsWidth.ToString() + "x" + ddsHeight.ToString());
            Console.ResetColor();

            Directory.CreateDirectory("texture_library" + folder.Substring(15));
            using (FileStream fs = new FileStream(textureName + ".texture", FileMode.Create))
            {
                char[] internalName = (textureName + ".dds").ToCharArray();
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(textureName.Length + 37);
                bw.Write(ddsData.Length);
                bw.Write(ddsWidth);
                bw.Write(ddsHeight);
                foreach (int i in texFlags) bw.Write(i);
                bw.Write(internalName);
                bw.Write((byte)0);
                bw.Write(ddsData);
                bw.Close();
            }
            File.Delete("texture_converter.dds");
        }
    }
}
