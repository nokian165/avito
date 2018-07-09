using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace topface.Models
{
    public class PhoneRecognizer
    {
        private string _digitPath;

        public Dictionary<int, string> DigitHash;

        public static int MinY = 13;
        /**
         * Bottom of text line (containing digits) 
         */
        public static int MaxY = 43;


        public PhoneRecognizer(string digitPath)
        {
            if (digitPath == null)
            {
                throw new ArgumentException("Path to folder with images of avito digits isn't defined");
            }
            this._digitPath = digitPath;

            DigitHash = new Dictionary<int, string>();
            
            PrepareDigits();
        }


        protected void PrepareDigits()
        {

		// Process digits from 0 to 9
		for (int i = 0; i< 10; i++) {

            PrepareDigitHash(_digitPath + "/" + i + ".png", i.ToString());
		}
        // Process "-" sign
        PrepareDigitHash(_digitPath + "/-.png", "-");

            DigitHash.Add(520774719, "0");
            DigitHash.Add(-1211395094, "1");
            DigitHash.Add(-835331661, "2");
            DigitHash.Add(783539677, "3");
            DigitHash.Add(919548879, "4");
            DigitHash.Add(1535080993, "5");
            DigitHash.Add(-524377782, "6");
            DigitHash.Add(228530383, "7");
            DigitHash.Add(-131751147, "8");
            DigitHash.Add(1103594460, "9");
            DigitHash.Add(-1229097553, "-");
        }


        protected void PrepareDigitHash(string digitFilePath, string digitName)
        {

            var test = new Bitmap(digitFilePath);

            //var ms = new MemoryStream(digitImgBytes);
            var ms = new MemoryStream();
            test.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var bytes = ms.ToArray();

            var hash = 0;

                using (var stream = new MemoryStream(bytes))
                   hash = MurMurHash3.Hash(stream);

            //DigitHash.Add(hash, digitName);



        }


        protected bool CheckFilledColumn(Bitmap image, int columnIndex)
        {
            // Search for filled pixel within meaningful range				
            for (int j = MinY; j < MaxY; j++)
            {
                var color = image.GetPixel(columnIndex, j);
                if (color.R != 255 && color.G != 255 && color.B != 255)
                    return true;
            }
            return false;
        }

        protected string RecognizeDigit(Bitmap phoneImg, int startX, int finishX)  //string
        {
            // Get subImage which corresponds to specified image block
            var rect = new Rectangle
            {
                X = startX,
                Y = MinY,
                Width = finishX - startX,
                Height = MaxY - MinY + 1
            };

            var img = phoneImg.Clone(rect, phoneImg.PixelFormat);


            //var ms = new MemoryStream(digitImgBytes);
            var ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            //img.Save(@"C:\result\first.png");

            var bytes = ms.ToArray();

            var hash = 0;

            using (var stream = new MemoryStream(bytes))
                hash = MurMurHash3.Hash(stream);

            string result;

            DigitHash.TryGetValue(hash, out result);

            return result;



            //          // Convert subImage to array of bytes
            //          ByteArrayOutputStream baos = new ByteArrayOutputStream();
            //      ImageIO.write(img, "jpg", baos);
            //byte[] bytes = baos.toByteArray();
            //      // Calculate hash of subImage
            //      Long hash = Murmur3.hash_x86_32(bytes, bytes.length, HASH_SEED);
            //// Return string of digit which corresponds to specified hash
            //return digitHash.get(hash);
        }


    public string Recognize(Bitmap image)
        {
            // Preallocate string for resulting phone number
            string res = "";
        // Width of image to be recognized 		
            int imgWidth = image.Width;
        // Flag - that indicates we're waiting for the last column with "filled" pixels 
        var wait = false;
        // Init variables to store x coordinates of a digit
        var xStart = 0;
        var xFinish = 0;		
		// Iterate over all x
		for (int i = 0; i<imgWidth; i++) {
			if (wait) {				
				if (!CheckFilledColumn(image, i)) {					
					// *** We found last pixels of a digit ***
					// Store last x coordinate of a digit
					xFinish = i;
					// Append recognized digit to resulting string
					res = res + RecognizeDigit(image, xStart, xFinish); //----------------
        // Wait for first x coordinate of next digit
        wait = false;
				}
            } else {
				if (CheckFilledColumn(image, i)) {
					// *** We found first pixels of a digit ***
					// Store first x coordinate of a digit
					xStart = i;
  				    // Wait for last x coordinate of a digit
					wait = true;
				}
			}

		}
		return res;
	}
	



}
}