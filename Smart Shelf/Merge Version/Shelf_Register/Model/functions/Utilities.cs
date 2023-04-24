using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shelf_Register
{
    public static class Utilities
    {
        public static int getRowbyAntenName(string antena)
        {
            int row = 0;
            if (antena == "1" || antena == "2")
            {
                row = 1;
            }
            else if (antena == "3" || antena == "4")
            {
                row = 2;
            }
            else if (antena == "5" || antena == "6")
            {
                row = 3;
            }
            else if (antena == "7" || antena == "8")
            {
                row = 4;
            }
            else if (antena == "9" || antena == "10")
            {
                row = 5;
            }
            else if (antena == "11" || antena == "12")
            {
                row = 6;
            }

            return row;
        }

        public static Image LoadImage(string base64_data)
        {
            if (base64_data != Const.no_image && base64_data != Const.blank_image)
            {
                byte[] bytes = Convert.FromBase64String(base64_data);

                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }

                return image;
            }
            else
            {
                Global.mainForm.pictureBox.Load(Const.no_image);
                return Global.mainForm.pictureBox.Image;
            }
        }

        public static bool CheckValidUrlNoLocal(string url)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        public static string ImageToBase64_Online(string _imagePath)
        {
            string encodedUrl = Convert.ToBase64String(Encoding.Default.GetBytes(_imagePath));

            using (var client = new WebClient())
            {
                byte[] dataBytes = client.DownloadData(new Uri(_imagePath));
                string encodedFileAsBase64 = Convert.ToBase64String(dataBytes);
                return encodedFileAsBase64;
            }

        }

        public static string ImageToBase64(string _imagePath)
        {
            string _base64String = null;

            using (System.Drawing.Image _image = System.Drawing.Image.FromFile(_imagePath))
            {
                using (MemoryStream _mStream = new MemoryStream())
                {
                    _image.Save(_mStream, _image.RawFormat);
                    byte[] _imageBytes = _mStream.ToArray();
                    _base64String = Convert.ToBase64String(_imageBytes);
                    return _base64String;
                }
            }
        }

        public static bool IsBase64(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0
               || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception)
            {
                // Handle the exception
            }
            return false;
        }

    }
}
