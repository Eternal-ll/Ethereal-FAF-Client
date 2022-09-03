namespace Ethereal.FA.Scmap
{
    public static class DDSImage
    {
        public static unsafe void ConvertToPng(byte[] data, string targetFile)
        {
            using var ms = new MemoryStream(data);
            using var image = Pfim.Pfimage.FromStream(ms);

            fixed (byte* ptr = image.Data)
            {
                using var bitmap = new System.Drawing.Bitmap(image.Width, image.Height, image.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, (IntPtr)ptr);
                bitmap.Save(targetFile, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
