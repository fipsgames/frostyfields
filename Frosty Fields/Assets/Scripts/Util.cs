using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Text;
using System.IO;

namespace util
{
    public class Util
    {

        public static float HexaX = 0.75f;
        public static float HexaY = 0.433013f;

        public static float HexaRatio(){
            return HexaY / HexaX;
        }

        //Maps from 2d model coordinates to 3d world with offset for level player start point and putting y values in z
        public static Vector3 ConvertToHexa(float x, float y, float levelShiftX, float levelShiftY)
        {
            float x_new = (x - levelShiftX) * HexaX;
            float z_new = (y - levelShiftY) * (HexaY * 2) + (x - levelShiftX) * (HexaY);
            return new Vector3((float)x_new, 0, (float)z_new);
        }

        //Overloaded method for when there is no level offset eg. level creator
        public static Vector3 ConvertToHexa(float x ,float y){
            return ConvertToHexa(x, y, 0, 0);
        }

        // Strip whitespaces from string
        public static string RemoveWhitespace(string input)
        {
            return new string(input.Where(c => !Char.IsWhiteSpace(c)).ToArray());
        }

        // create a subset from a range of indices
        public static T[] RangeSubset<T>(T[] array, int startIndex, int length)
        {
            T[] subset = new T[length];
            Array.Copy(array, startIndex, subset, 0, length);
            return subset;
        }

        // swaps two items in a list
        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        //decrease values below 0.5 increase above 0.5
        public static float Squarify(float fraction){
            return (fraction * 2.0f) * (fraction * 2.0f) / 4.0f;
        }

        //maps from linear 0-1 to smooth sin 0-1
        public static float SmoothFraction(float fraction)
        {
            float result = (Mathf.Sin((fraction - 0.5f) * Mathf.PI) + 1.0f) / 2.0f;
            ////Logger.Log("Smooth Fraction Maps from: " + fraction + " to " + result);
            return result;
        }

        //maps from linear 0-1 to 0-1 / fast start / slow end
        public static float DeAccelerationFraction(float fraction)
        {
            float result = (Mathf.Sin((fraction / 2.0f) * Mathf.PI) + 1.0f) -1.0f;
            ////Logger.Log("DEAccelerate Fraction Maps from: " + fraction + " to " + result);
            return result;
        }

        //maps from linear 0-1 to 0-1 / slow start / fast end
        public static float AccelerationFraction(float fraction)
        {
            float result = Mathf.Sin(((fraction - 1.0f) / 2.0f) * Mathf.PI) + 1.0f;
            ////Logger.Log("Accelerate Fraction Maps from: " + fraction + " to " + result);
            return result;
        }

        public static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GetHash(hashAlgorithm, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }

        public static string Encrypt(string textToEncrypt)
        {
            try
            {
                string ToReturn = "";
                string publickey = "12345678";
                string secretkey = "87654321";
                byte[] secretkeyByte = { };
                secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public static string Decrypt(string textToDecrypt)
        {
            try
            {
                string ToReturn = "";
                string publickey = "12345678";
                string secretkey = "87654321";
                byte[] privatekeyByte = { };
                privatekeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    ToReturn = encoding.GetString(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ae)
            {
                throw new Exception(ae.Message, ae.InnerException);
            }
        }

        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
        
    }
}
