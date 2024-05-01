using System;
using System.IO;

namespace DSAppearancePresetTool
{
    public class AppearanceData
    {
        public byte sex;
        public byte physique;
        public int hairstyle;
        public byte[] hairColor; //12
        public byte[] eyeColor; //12
        public byte[] faceData; //50
        public byte[] skinColor; //50

        public void Write(string filePath)
        {
            using(FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                try
                {
                    BinaryWriter writer = new BinaryWriter(stream);

                    writer.Write(sex);
                    writer.Write(physique);
                    writer.Write(hairstyle);
                    writer.Write(hairColor);
                    writer.Write(eyeColor);
                    writer.Write(faceData);
                    writer.Write(skinColor);
                } //try
                catch
                {
                    throw;
                } //catch
                finally
                {
                    stream.Close();
                } //finally
            } //using
        } //Write

        public void Read(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    BinaryReader reader = new BinaryReader(stream);

                    sex = reader.ReadByte();
                    physique = reader.ReadByte();
                    hairstyle = reader.ReadInt32();
                    hairColor = reader.ReadBytes(12);
                    eyeColor = reader.ReadBytes(12);
                    faceData = reader.ReadBytes(50);
                    skinColor = reader.ReadBytes(50);
                } //try
                catch
                {
                    throw;
                } //catch
                finally
                {
                    stream.Close();
                } //finally
            } //using
        } //Read

        //DEBUG
        public void PrintInfo()
        {
            Console.WriteLine($"Sex: {sex}");
            Console.WriteLine($"Physique: {physique}");
            Console.WriteLine($"Hair Style: {hairstyle}");
            Console.Write("Hair Color: ");

            foreach (byte b in hairColor)
            {
                Console.Write($"{b.ToString("x")} ");
            } //foreach

            Console.WriteLine();

            Console.Write("Eye Color: ");

            foreach (byte b in eyeColor)
            {
                Console.Write($"{b.ToString("x")} ");
            } //foreach

            Console.WriteLine();

            Console.Write("Face Data: ");

            foreach (byte b in faceData)
            {
                Console.Write($"{b.ToString("x")} ");
            } //foreach

            Console.WriteLine();

            Console.Write("Skin Color: ");

            foreach (byte b in skinColor)
            {
                Console.Write($"{b.ToString("x")} ");
            } //foreach

            Console.WriteLine();
        } //PrintInfo
    } //class
} //namespace
