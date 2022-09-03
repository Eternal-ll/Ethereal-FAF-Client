using System.Text;

namespace Ethereal.FA.Scmap
{
    /// <summary>
    /// 
    /// </summary>
    public class ScmapBinaryReader : BinaryReader
    {
        public ScmapBinaryReader(Stream input) : base(input) { }
        public ScmapBinaryReader(Stream input, Encoding encoding) : base(input, encoding) { }
        public ScmapBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Point ReadVector2() => new Point()
        {
            X = ReadSingle(),
            Y = ReadSingle()
        };
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Point3D ReadVector3() => new Point3D()
        {
            X = ReadSingle(),
            Y = ReadSingle(),
            Z = ReadSingle()
        };
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Point4D ReadVector4() => new Point4D()
        {
            X = ReadSingle(),
            Y = ReadSingle(),
            Z = ReadSingle(),
            W = ReadSingle()
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public string ReadString(int lenght) =>
            Encoding.ASCII.GetString(ReadBytes(lenght));
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadStringNull()
        {
            StringBuilder sb = new StringBuilder();
            while (BaseStream.Position != BaseStream.Length)
            {
                var byteValue = ReadByte();
                if (byteValue == 0) break;
                sb.Append((char)byteValue);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<char> ReadStringNull2()
        {
            while (BaseStream.Position != BaseStream.Length)
            {
                var byteValue = ReadByte();
                if (byteValue == 0) break;
                yield return (char)byteValue;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public short[] ReadInt16Array(int lenght)
        {
            short[] array = new short[lenght];
            for (int i = 0; i < lenght; i++)
            {
                array[i] = ReadInt16();
            }
            return array;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public int[] ReadInt32Array(int lenght)
        {
            int[] array = new int[lenght];
            for (int i = 0; i < lenght; i++)
            {
                array[i] = ReadInt32();
            }
            return array;
        }
        /// <summary>
        /// 
        /// </summary>
        public void SeekNull()
        {
            while (!(ReadByte() == 0))
            { }
        }
        /// <summary>
        /// 
        /// </summary>
        public void SeekSkipNull()
        {
            SeekNull();
            BaseStream.Position += 1;
        }
    }
}
