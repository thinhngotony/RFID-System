using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

//new 20220910: get product from local file

namespace Shelf_Register
{
    public class ShelfProduct
    {
        public string rfid { get; set; }
        public string jancode { get; set; }
        public string goods_name { get; set; }
        public string path_img { get; set; }
    }

    class ShelfLocal
    {
        public static ShelfProduct getProductByRFID(string rfid, DataTable dataList)
        {
            DataRow[] dr = dataList.Select(String.Format("rfid = '{0}'", rfid));

            if (dr.Length == 0)
            {
                return null;
            }

            ShelfProduct product = new ShelfProduct();
            product.rfid = dr.First()["rfid"].ToString();
            product.jancode = dr.First()["jancode"].ToString();
            product.goods_name = dr.First()["goods_name"].ToString();
            product.path_img = dr.First()["path_img"].ToString();

            return product;
        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();

                    if (rows.Length == headers.Length)
                    {
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                        }
                        dt.Rows.Add(dr);
                    }
                    else { }
                }
            }
            return dt;
        }

        public static bool DownloadImage(string imageUrl, string filename, ImageFormat format)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);

            Bitmap bitmap;
            bitmap = new Bitmap(stream);

            if (bitmap != null)
                bitmap.Save(filename, format);

            stream.Flush();
            stream.Close();
            client.Dispose();

            return File.Exists(filename);
        }

        public static void UpdateLocalCSV(string file_path, ShelfProduct product)
        {
            string new_data = String.Format("\n{0},{1},{2},{3}", product.rfid, product.jancode, product.goods_name, product.path_img);
            File.AppendAllText(file_path, new_data);
        }

    }
}
