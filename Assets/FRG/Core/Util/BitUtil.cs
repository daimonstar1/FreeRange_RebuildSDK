using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FRG.Core
{
    public static class BitUtil
    {
        /// <summary>
        /// Aggressive inlining. Probably no effect in Unity, but will affect server code.
        /// </summary>
        public const MethodImplOptions AggressiveInlining = (MethodImplOptions)256;

        [MethodImpl(AggressiveInlining)]
        public static int LowestBit(int bits)
        {
            return (int)((uint)bits & ~((uint)bits - 1u));
        }

        // from http://stackoverflow.com/questions/12171584/what-is-the-fastest-way-to-count-set-bits-in-uint32-in-c-sharp
        public static int CountBits( uint i ) {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (int)((((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24);
        }

        /// <summary>
        /// Combines two 32-bit hash codes.
        /// </summary>
        /// <remarks>
        /// Low-quality hash combining. A better way to hash is to append all values, then finalize, but
        /// the source hash codes are usually pretty bad, so a great combiner function isn't worth it.
        /// </remarks>
        [MethodImpl(AggressiveInlining)]
        public static int CombineHashCodes(int accumulation, int value)
        {
            unchecked {
                uint v0 = (uint)accumulation;
                uint v1 = (uint)value;

                // AXR: addition rotation xor
                // This is the first part of a Chaskey round
                // Also nearly equivalent to FNV-1 using the traditional multipler of 33, but incorporates more bits of both values
                // FNV-1: (v0 * 33) ^ v1 = (v0 << 5 + v0) ^ v1
                // This: rotate_left(v0, 5) ^ (v0 + v1)
                v1 += v0;
                v0 = v0 << 5 | v0 >> 27;
                v0 = v0 ^ v1;

                // Add in a constant value because otherwise zeroes don't work well for repeated application
                // Doesn't really matter what it is; this constant taken from SipHash
                return (int)(v0 ^ 0x70736575);
            }
        }

        /// <summary>
        /// Combines two 64-bit hash codes.
        /// </summary>
        /// <remarks>
        /// Low-quality hash combining. A better way to hash is to append all values, then finalize.
        /// </remarks>
        [MethodImpl(AggressiveInlining)]
        public static long CombineHashCodes(long accumulation, long value)
        {
            unchecked {
                ulong v0 = (ulong)accumulation;
                ulong v1 = (ulong)value;

                // AXR: addition rotation xor
                // This is the first part of a SipHash round
                // Assumes a 64-bit hash code is going to be higher quality
                v1 += v0;
                v0 = v0 << 13 | v0 >> 51;
                v0 = v0 ^ v1;

                // Add in a constant value because otherwise zeroes don't work well for repeated application
                // Doesn't really matter what it is; this constant taken from SipHash
                return (long)(v0 ^ 0x736f6d6570736575);
            }
        }

        /// <summary>
        /// Reduces a 64-bit hash code into 32-bits.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static int ReduceHashCode(long hashCode)
        {
            unchecked {
                return CombineHashCodes((int)(uint)hashCode, (int)(uint)((ulong)hashCode >> 32));
            }
        }

        public static string ToHex(byte[] bytes)
        {
#if !MAX_COMPATIBILTY
            using (Pooled<StringBuilder> pooled = RecyclingPool.SpawnStringBuilder(bytes.Length * 2)) {
            StringBuilder builder = pooled.Value;
#else
            StringBuilder builder = new StringBuilder();
            {
#endif
            foreach (byte b in bytes) {
                    builder.AppendFormat("{0:x2}", b);
                }
                return builder.ToString();
            }
        }

        public static string ToHex(byte value)
        {
            return value.ToString("x2");
        }

        public static string ToHex(sbyte value)
        {
            return value.ToString("x2");
        }

        public static string ToHex(short value)
        {
            return value.ToString("x4");
        }

        public static string ToHex(ushort value)
        {
            return value.ToString("x4");
        }

        public static string ToHex(int value)
        {
            return value.ToString("x8");
        }

        public static string ToHex(uint value)
        {
            return value.ToString("x8");
        }

        public static string ToHex(long value)
        {
            return value.ToString("x16");
        }
        
        public static string ToHex(ulong value)
        {
            return value.ToString("x16");
        }

        /// <summary>
        /// Create an MD5 hash of the input. Do not use this for anything secure.
        /// </summary>
        /// <remarks>
        /// If you are using this at runtime, you might need to add a link.xml to the Assets folder for AOT builds like iOS.
        /// http://blogs.msdn.com/b/csharpfaq/archive/2006/10/09/how-do-i-calculate-a-md5-hash-from-a-string_3f00_.aspx
        /// </remarks>
        public static string CalculateMD5(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            return ToHex(hash);
        }
        
        /// <summary>
        /// Calculates a SHA-256.
        /// </summary>
        /// <param name="input">Arbitrary-length byte array.</param>
        /// <returns>32 bytes (256 bits) of hashed data.</returns>
        /// <remarks>Can be computationally expensive and allocation-heavy.</remarks>
        public static byte[] CalculateSha256(byte[] input, int startIndex, int length)
        {
            using (SHA256 sha = SHA256.Create()) {
                return sha.ComputeHash(input, startIndex, length);
            }
        }

        /// <summary>
        /// Calculates a SHA-256 and truncates down from 32 to 8 bytes (64 bits).
        /// </summary>
        /// <remarks>
        /// Can be computationally expensive and allocation-heavy.
        /// </remarks>
        public static long CalculateSha256TruncatedChecksum(string input)
        {
#if !MAX_COMPATIBILTY
            using (Pooled<MemoryStream> pooledStream = RecyclingPool.SpawnMemoryStream()) {
                MemoryStream stream = pooledStream.Value;
                using (Pooled<StreamWriter> pooledWriter = RecyclingPool.SpawnStreamWriter(stream)) {
                    StreamWriter writer = pooledWriter.Value;
#else
            using (MemoryStream stream = new MemoryStream()) {
                using (StreamWriter writer = new StreamWriter(stream)) {
#endif
                    writer.Write(input);
                }

                if (stream.Length > int.MaxValue) { throw new ArgumentException("Input string is far too long.", "input"); }
                return CalculateSha256TruncatedChecksum(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        /// <summary>
        /// Calculates a SHA-256 and truncates down from 32 to 8 bytes (64 bits).
        /// </summary>
        /// <remarks>
        /// <para>Can be computationally expensive and allocation-heavy.</para>
        /// <para>It is safe to truncate SHA-256, but this is used for code generation purposes and not as a cryptographic hash function.
        /// Instead, we use it as a checksum, to verify whether two pieces of generated data are almost certainly the same.</para>
        /// <para>General rule is that collision resistance of SHA-224/256/512 is 2^(bitlength / 2), so SHA-256 is 2^128 and truncated it should be 2^32, meaning
        /// if there are less than 4 billion different values, they are very unlikely to collide.
        /// <a href="http://crypto.stackexchange.com/questions/24732/probability-of-sha256-collisions-for-certain-amount-of-hashed-values">See article.</a></para>
        /// </remarks>
        public static long CalculateSha256TruncatedChecksum(byte[] input)
        {
            if (input == null) { throw new ArgumentNullException("input"); }
            return CalculateSha256TruncatedChecksum(input, 0, input.Length);
        }

        /// <summary>
        /// Calculates a SHA-256 and truncates down from 32 to 8 bytes (64 bits).
        /// </summary>
        /// <remarks>
        /// Can be computationally expensive and allocation-heavy.
        /// </remarks>
        public static long CalculateSha256TruncatedChecksum(byte[] input, int startIndex, int length)
        {
            byte[] output = CalculateSha256(input, startIndex, length);
            return BitConverter.ToInt64(output, 0);
        }

#if UNITY_EDITOR_WIN || !UNITY_EDITOR && UNITY_STANDALONE_WIN
        [DllImport("msvcrt.dll")]
        private static extern int memcmp(byte[] arr1, byte[] arr2, int cnt);
#endif

        /// <summary>
        /// Compares two byte arrays for equality.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static bool ByteArrayEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length) return false;

            return ByteArrayEqual(left, right, left.Length);
        }

        /// <summary>
        /// Compares two byte arrays for equality.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static bool ByteArrayEqual(byte[] left, byte[] right, int length)
        {
            return (ByteArrayCompare(left, right, length) == 0);
        }

        /// <summary>
        /// Returns the comparison of the first differing byte.
        /// </summary>
        public static int ByteArrayCompare(byte[] left, byte[] right, int length)
        {
            if (left.Length < length) throw new ArgumentOutOfRangeException("length", length, "left array is smaller than length.");
            if (right.Length < length) throw new ArgumentOutOfRangeException("length", length, "right array is smaller than length.");

#if UNITY_EDITOR_WIN || !UNITY_EDITOR && UNITY_STANDALONE_WIN
            return memcmp(left, right, length);
#else
            for (int i = 0; i < length; ++i)
            {
                if (left[i] != right[i])
                {
                    return left[i].CompareTo(right[i]);
                }
            }
            return 0;
#endif
        }

        /// <summary>
        /// Memset, for primitive types only.
        /// </summary>
        public static void AssignPrimitive<T>(T[] buffer, T value)
            where T : struct, IConvertible, IEquatable<T>
        {
            const int BlockLength = 64;
            int totalLength = Buffer.ByteLength(buffer);
            int valueLength = totalLength / buffer.Length;

            int initialLength = Math.Min(buffer.Length, (BlockLength + valueLength - 1) / valueLength);
            if (initialLength == 0) { initialLength = buffer.Length; }
            for (int i = 0; i < initialLength; ++i)
            {
                buffer[i] = value;
            }
            
            if (initialLength == buffer.Length)
            {
                // Early out
                return;
            }
            
            int alignedLength = totalLength - (totalLength % BlockLength);

            int writtenLength = initialLength * valueLength;
            while (writtenLength < alignedLength)
            {
                Buffer.BlockCopy(buffer, 0, buffer, writtenLength, BlockLength);
                writtenLength += BlockLength;
            }

            if (writtenLength < totalLength)
            {
                Buffer.BlockCopy(buffer, 0, buffer, writtenLength, totalLength - writtenLength);
            }
        }

        /// <summary>
        /// Parses a string or substring into an integer, very quickly. Returns 0 for non-numeric strings.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static int? ParseIntFast(string numericString, int startIndex = 0)
        {
            return ParseIntFast(numericString, startIndex, numericString.Length - startIndex);
        }

        /// <summary>
        /// Parses a substring into an integer, very quickly. Returns 0 for non-numeric strings.
        /// </summary>
        public static int? ParseIntFast(string numericString, int startIndex, int length)
        {
            int iterIndex = startIndex;
            int endIndex = iterIndex + length;

            // Needed later negative check
            if (iterIndex == endIndex)
            {
                return null;
            }

            int value = 0;
            for (; iterIndex < endIndex; iterIndex = NextCodePoint(numericString, iterIndex, endIndex))
            {
                // Ignore other chars, so we can have commas, leading +/-, etc
                char c = numericString[iterIndex];
                if (c >= '0' && c <= '9') {
                    value = value * 10 + c - '0';
                }
                else {
                    return null;
                }
            }

            if (numericString[startIndex] == '-')
            {
                value = -value;
            }

            return value;
        }

        /// <summary>
        /// Parses a string or substring into an integer, very quickly. Returns 0 for non-numeric strings.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static int ParseIntFastUnchecked(string numericString, int startIndex, int length)
        {
            return ParseIntFast(numericString, startIndex, length) ?? 0;
        }

        /// <summary>
        /// Guids are usually represented big endian, and the Guid struct is stored weird.
        /// If we serialize Guids as the individual fields, we can ensure the byte order is correct.
        /// </summary>
        public static Guid ComposeGuid(DecomposedGuid input)
        {
            GuidConvert convert;
            convert.normal = new Guid();
            convert.decomposed = input;
            return convert.normal;
        }

        /// <summary>
        /// Guids are usually represented big endian, and the Guid struct is stored weird.
        /// If we serialize Guids as the individual fields, we can ensure the byte order is correct.
        /// </summary>
        public static Guid ComposeGuidFromLittleEndian(LittleEndianGuid input)
        {
            if (BitConverter.IsLittleEndian) {
                GuidConvert convert;
                convert.normal = new Guid();
                convert.littleEndian = input;
                return convert.normal;
            }
            else {
                GuidConvert original;
                original.byteGuid = new ByteGuid();
                original.littleEndian = input;

                ByteGuid little = original.byteGuid;
                ByteGuid native = new ByteGuid();

                // Each component byte swapped, then whole thing byte swapped
                native.c0 = little.a0;
                native.c1 = little.a1;
                native.b0 = little.a2;
                native.b1 = little.a3;
                native.a0 = little.b0;
                native.a1 = little.b1;
                native.a2 = little.c0;
                native.a3 = little.c1;

                // Just byte swapped
                native.k = little.d;
                native.j = little.e;
                native.i = little.f;
                native.h = little.g;
                native.g = little.h;
                native.f = little.i;
                native.e = little.j;
                native.d = little.k;

                GuidConvert convert;
                convert.normal = new Guid();
                convert.byteGuid = native;
                return convert.normal;
            }
        }

        /// <summary>
        /// Guids are usually represented big endian, and the Guid struct is stored weird.
        /// If we serialize Guids as the individual fields, we can ensure the byte order is correct.
        /// </summary>
        public static DecomposedGuid DecomposeGuid(Guid input)
        {
            GuidConvert convert;
            convert.decomposed = new DecomposedGuid();
            convert.normal = input;
            return convert.decomposed;
        }

        /// <summary>
        /// Guids are usually represented big endian, and the Guid struct is stored weird.
        /// If we serialize Guids as the individual fields, we can ensure the byte order is correct.
        /// </summary>
        public static LittleEndianGuid DecomposeGuidToLittleEndian(Guid input)
        {
            if (BitConverter.IsLittleEndian) {
                GuidConvert convert;
                convert.littleEndian = new LittleEndianGuid();
                convert.normal = input;
                return convert.littleEndian;
            }
            else {
                GuidConvert original;
                original.byteGuid = new ByteGuid();
                original.normal = input;

                ByteGuid native = original.byteGuid;
                ByteGuid little = new ByteGuid();
                // Each component byte swapped, then whole thing byte swapped
                little.a0 = native.c0;
                little.a1 = native.c1;
                little.a2 = native.b0;
                little.a3 = native.b1;
                little.b0 = native.a0;
                little.b1 = native.a1;
                little.c0 = native.a2;
                little.c1 = native.a3;

                // Just byte swapped
                little.d = native.k;
                little.e = native.j;
                little.f = native.i;
                little.g = native.h;
                little.h = native.g;
                little.i = native.f;
                little.j = native.e;
                little.k = native.d;

                GuidConvert convert;
                convert.littleEndian = new LittleEndianGuid();
                convert.byteGuid = little;
                return convert.littleEndian;
            }
        }
        
        public static Guid ParseUuid(string input)
        {
            try {
                return new Guid(input ?? "");
            }
            catch (SystemException e) {
                if (!(e is FormatException || e is OverflowException)) { throw; }

#if !MAX_COMPATIBILTY
                //throw new NetworkSerializationException("GUID could not be parsed.", e);
                throw new Exception("GUID could not be parsed.", e);
#else
                throw new Exception("GUID could not be parsed.", e);
#endif
            }
        }

        public static bool TryParseUuid(string input, out Guid uuid)
        {
            try {
                uuid = new Guid(input ?? "");
                return true;
            }
            catch (SystemException e) {
                if (!(e is FormatException || e is OverflowException)) { throw; }

                uuid = new Guid();
                return false;
            }
        }

        public static string SerializeUuid(Guid guid)
        {
            return guid.ToString("N");
        }

        public static bool IsValidUuid(string uuid)
        {
            return (uuid != null && uuid.Length == 32 && !Statics.InvalidUuidRegex.IsMatch(uuid));
        }

        /// <summary>
        /// Like char.IsDigit, but only for ascii, so no worrying about surrogate pairs.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static bool IsAsciiDigit(string s, int index)
        {
            char c = s[index];
            return (c >= '0' && c <= '9');
        }

        /// <summary>
        /// Like char.IsDigit, but only for ascii, so no worrying about surrogate pairs.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static bool IsDigitFast(char c)
        {
            return (c >= '0' && c <= '9');
        }

        /// <summary>
        /// Returns true and increments the index if the current value is a digit.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static int NextCodePoint(string s, int index)
        {
            return NextCodePoint(s, index, s.Length);
        }

        /// <summary>
        /// Returns true and increments the index if the current value is a digit.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static int NextCodePoint(string s, int index, int maxIndex)
        {
            if (char.IsSurrogatePair(s, index) && index + 2 <= maxIndex) return index + 2;
            return index + 1;
        }

#if !MAX_COMPATIBILTY
        /// <summary>
        /// Spawns a new GZip stream.
        /// </summary>
        /// <param name="useGzip">true to use a gzip stream, false to just use the base stream.</param>
        /// <param name="baseStream">The stream to read or write from. NOTE: This stream will not be closed.</param>
        /// <param name="isDecompressing">true to decompress from gzip, false to compress to gzip.</param>
//        public static Pooled<Stream> SpawnGZipStream(bool useGzip, Stream baseStream, bool isDecompressing)
//        {
//            Action<object> destructor;

//            Stream stream;
//            if (useGzip)
//            {
//#if GAME_SERVER
//                if (isDecompressing)
//                {
//                    stream = new System.IO.Compression.GZipStream(baseStream, System.IO.Compression.CompressionMode.Decompress, false);
//                }
//                else
//                {
//                    stream = new System.IO.Compression.GZipStream(baseStream, System.IO.Compression.CompressionLevel.Optimal, false);
//                }
//#else
//                // These are not actually pooled right now; not exactly designed to be resettable
//                // Standard GZipStream is pretty bad until 4.0; mono Unity one is unimplemented
//                stream = new BestHTTP.Decompression.Zlib.GZipStream(baseStream, isDecompressing ? BestHTTP.Decompression.Zlib.CompressionMode.Decompress : BestHTTP.Decompression.Zlib.CompressionMode.Compress, BestHTTP.Decompression.Zlib.CompressionLevel.BestCompression, false);
//#endif
//                destructor = Statics.CloseStream;
//            }
//            else
//            {
//                stream = baseStream;
//                destructor = null;
//            }
//            return new Pooled<Stream>(stream, destructor);
//        }
#else
        /// <summary>
        /// Spawns a new GZip stream.
        /// </summary>
        /// <param name="useGzip">true to use a gzip stream, false to just use the base stream.</param>
        /// <param name="baseStream">The stream to read or write from. NOTE: This stream will not be closed.</param>
        /// <param name="isDecompressing">true to decompress from gzip, false to compress to gzip.</param>
        public static Stream SpawnGZipStream(bool useGzip, Stream baseStream, bool isDecompressing)
        {
            Stream stream;
            if (useGzip)
            {
#if GAME_SERVER
                if (isDecompressing)
                {
                    stream = new System.IO.Compression.GZipStream(baseStream, System.IO.Compression.CompressionMode.Decompress, false);
                }
                else
                {
                    stream = new System.IO.Compression.GZipStream(baseStream, System.IO.Compression.CompressionLevel.Optimal, false);
                }
#else
                // These are not actually pooled right now; not exactly designed to be resettable
                // Standard GZipStream is pretty bad until 4.0; mono Unity one is unimplemented
                stream = new BestHTTP.Decompression.Zlib.GZipStream(baseStream, isDecompressing ? BestHTTP.Decompression.Zlib.CompressionMode.Decompress : BestHTTP.Decompression.Zlib.CompressionMode.Compress, BestHTTP.Decompression.Zlib.CompressionLevel.BestCompression, false);
#endif
            }
            else
            {
                stream = baseStream;
            }
            return stream;
        }
#endif

//        public static byte[] CompressToGZip(byte[] bytes, int offset, int length)
//        {
//            if (bytes == null) throw new ArgumentNullException("bytes");

//#if !MAX_COMPATIBILTY
//            using (Pooled<MemoryStream> memoryStream = RecyclingPool.SpawnMemoryStream())
//            {
//                using (Pooled<Stream> gzipStream = SpawnGZipStream(true, memoryStream.Value, false))
//                {
//                    gzipStream.Value.Write(bytes, offset, length);
//                }
//                // Doesn't flush until close. (Flush call doesn't help.)
//                return memoryStream.Value.ToArray();
//            }
//#else
//            using (MemoryStream memoryStream = new MemoryStream())
//            {
//                using (Stream gzipStream = SpawnGZipStream(true, memoryStream, false))
//                {
//                    gzipStream.Write(bytes, 0, bytes.Length);
//                }
//                // Doesn't flush until close. (Flush call doesn't help.)
//                return memoryStream.ToArray();
//            }
//#endif
//        }

//        public static byte[] DecompressFromGZip(byte[] bytes)
//        {
//            if (bytes == null) throw new ArgumentNullException("bytes");
//#if !MAX_COMPATIBILTY
//            using (Pooled<MemoryStream> memoryStreamOutput = RecyclingPool.SpawnMemoryStream())
//            using (Pooled<MemoryStream> memoryStream = RecyclingPool.SpawnMemoryStream(bytes))
//            using (Pooled<Stream> gzipStream = SpawnGZipStream(true, memoryStream.Value, true))
//            {
//                byte[] buffer = new byte[1000];
//                int n;
//                while ((n = gzipStream.Value.Read(buffer, 0, buffer.Length)) != 0)
//                {
//                    memoryStreamOutput.Value.Write(buffer, 0, n);
//                }

//                return memoryStreamOutput.Value.ToArray();
//            }
//#else
//            using (MemoryStream memoryStreamOutput = new MemoryStream())
//            using (MemoryStream memoryStream = new MemoryStream(bytes))
//            using (Stream gzipStream = SpawnGZipStream(true, memoryStream, true))
//            {
//                byte[] buffer = new byte[1000];
//                int n;
//                while ((n = gzipStream.Read(buffer, 0, buffer.Length)) != 0)
//                {
//                    memoryStreamOutput.Write(buffer, 0, n);
//                }

//                return memoryStreamOutput.ToArray();
//            }
//#endif
//        }

//        public static string CompressToGZipBase64(string rawText)
//        {
//            const int ExtraBuffer = 64;
//#if !MAX_COMPATIBILTY
//            using (Pooled<MemoryStream> memoryStream = RecyclingPool.SpawnMemoryStream(Encoding.UTF8.GetByteCount(rawText) + ExtraBuffer))
//            {
//                using (Pooled<Stream> gzipStream = SpawnGZipStream(true, memoryStream.Value, false))
//                using (Pooled<StreamWriter> writer = RecyclingPool.SpawnStreamWriter(gzipStream.Value))
//                {
//                    writer.Value.Write(rawText);
//                }
//                // Doesn't flush until close. (Flush call doesn't help.)

//                return Convert.ToBase64String(memoryStream.Value.ToArray());
//            }
//#else
//            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetByteCount(rawText) + ExtraBuffer))
//            {
//                using (Stream gzipStream = SpawnGZipStream(true, memoryStream, false))
//                using (StreamWriter writer = new StreamWriter(gzipStream))
//                {
//                    writer.Write(rawText);
//                }
//                // Doesn't flush until close. (Flush call doesn't help.)

//                return Convert.ToBase64String(memoryStream.ToArray());
//            }
//#endif
//        }

//        public static string DecompressStringFromGZipBase64(string encodedGZipBase64)
//        {
//            byte[] gzippedData = Convert.FromBase64String(encodedGZipBase64);
//#if !MAX_COMPATIBILTY
//            using (Pooled<MemoryStream> memoryStream = RecyclingPool.SpawnMemoryStream(gzippedData))
//            {
//                using (Pooled<Stream> gzipStream = SpawnGZipStream(true, memoryStream.Value, true))
//                using (Pooled<StreamReader> reader = RecyclingPool.SpawnStreamReader(gzipStream.Value))
//                {
//                    return reader.Value.ReadToEnd();
//                }
//            }
//#else
//            using (MemoryStream memoryStream = new MemoryStream(gzippedData))
//            {
//                using (Stream gzipStream = SpawnGZipStream(true, memoryStream, true))
//                using (StreamReader reader = new StreamReader(gzipStream))
//                {
//                    return reader.ReadToEnd();
//                }
//            }
//#endif
//        }

        //public static string OptionallyCompressToGZipBase64WithPrefix(string rawText, string encodedPrefix)
        //{
        //    encodedPrefix = encodedPrefix ?? "";

        //    string compressed = CompressToGZipBase64(rawText);
        //    if (Encoding.UTF8.GetByteCount(encodedPrefix) + Encoding.UTF8.GetByteCount(compressed) > Encoding.UTF8.GetByteCount(rawText))
        //    {
        //        return rawText;
        //    }
        //    else
        //    {
        //        return encodedPrefix + compressed;
        //    }
        //}

        //public static string OptionallyCompressToBase64(byte[] rawData, int offset, int length, string encodedPrefix)
        //{
        //    encodedPrefix = encodedPrefix ?? "";

        //    byte[] compressed = CompressToGZip(rawData, offset, length);
        //    string base64Compressed = Convert.ToBase64String(compressed);
        //    string base64Raw = Convert.ToBase64String(rawData, offset, length);
        //    if (Encoding.UTF8.GetByteCount(encodedPrefix) + Encoding.UTF8.GetByteCount(base64Compressed) > Encoding.UTF8.GetByteCount(base64Raw))
        //    {
        //        return base64Raw;
        //    }
        //    else
        //    {
        //        return encodedPrefix + base64Compressed;
        //    }
        //}

        //public static string OptionallyDecompressFromGZipBase64WithPrefix(string prefixedEncodedText, string encodedPrefix)
        //{
        //    encodedPrefix = encodedPrefix ?? "";

        //    if (prefixedEncodedText.StartsWith(encodedPrefix, StringComparison.Ordinal))
        //    {
        //        // Only works like this with ordinal comparison
        //        string encodedText = prefixedEncodedText.Substring(encodedPrefix.Length);

        //        return DecompressStringFromGZipBase64(encodedText);
        //    }
        //    else
        //    {
        //        return prefixedEncodedText;
        //    }
        //}

        //public static byte[] OptionallyDecompressToBytes(string prefixedEncodedText, string encodedPrefix)
        //{
        //    encodedPrefix = encodedPrefix ?? "";

        //    if (prefixedEncodedText.StartsWith(encodedPrefix, StringComparison.Ordinal))
        //    {
        //        // Only works like this with ordinal comparison
        //        string encodedText = prefixedEncodedText.Substring(encodedPrefix.Length);
        //        byte[] compresedBytes = Convert.FromBase64String(encodedText);
        //        return DecompressFromGZip(compresedBytes);
        //    }
        //    else
        //    {
        //        return Convert.FromBase64String(prefixedEncodedText);
        //    }
        //}

        /// <summary>
        /// Reinterpret a float as an int, preserving endianness.
        /// </summary>
        public static int SingleToInt32Bits(float value)
        {
            //return BitConverter.SingleToInt32Bits(value);
            //return *((int*)&value);

            FloatConvert convert = new FloatConvert();
            convert.single = value;
            return convert.int32;
        }

        /// <summary>
        /// Reinterpret an int as a float, preserving endianness.
        /// </summary>
        /// <remarks>
        /// Unsafe code would be faster and allocation free, but it's less portable, and a pain to set up.
        /// Can also mess around with StructLayout(0).
        /// </remarks>
        public static float Int32BitsToSingle(int value)
        {
            //return BitConverter.Int32BitsToSingle(value);
            //return *((float*)&value);

            FloatConvert convert = new FloatConvert();
            convert.int32 = value;
            return convert.single;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DecomposedGuid
        {
            public uint a;
            public ushort b;
            public ushort c;

            public byte d;
            public byte e;
            public byte f;
            public byte g;
            public byte h;
            public byte i;
            public byte j;
            public byte k;
        }

        /// <summary>
        /// A GUID stored as two longs. Only stored correctly on little endian architecture.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        public struct LittleEndianGuid
        {
            public ulong abc;
            public ulong defghijk;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ByteGuid
        {
            public byte a0;
            public byte a1;
            public byte a2;
            public byte a3;
            public byte b0;
            public byte b1;
            public byte c0;
            public byte c1;

            public byte d;
            public byte e;
            public byte f;
            public byte g;
            public byte h;
            public byte i;
            public byte j;
            public byte k;
        }

#if UNITY_WEBGL
#warning "BitUtil.GuidConvert uses a union trick that probably does not work on WebGL."
#endif

        /// <summary>
        /// Explicit layout lets us define a true union. Don't do this with reference fields.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct GuidConvert
        {
            [FieldOffset(0)]
            public Guid normal;

            [FieldOffset(0)]
            public DecomposedGuid decomposed;

            [FieldOffset(0)]
            public LittleEndianGuid littleEndian;

            [FieldOffset(0)]
            public ByteGuid byteGuid;
        }

#if UNITY_WEBGL
#warning "BitUtil.FloatConvert uses a union trick that probably does not work on WebGL. Buffer.BlockCopy stand in, but requires arrays."
#endif

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatConvert
        {
            [FieldOffset(0)]
            public float single;

            [FieldOffset(0)]
            public int int32;
        }

        private static class Statics
        {
            const RegexOptions Options = RegexOptions.CultureInvariant
#if GAME_SERVER
                 | RegexOptions.Compiled
#endif
                ;

            public static readonly Regex InvalidDecimalRegex = new Regex(@"[^0-9]", Options);
            public static readonly Regex InvalidUuidRegex = new Regex(@"[^0-9a-f]", Options);
            public static readonly Action<object> CloseStream = stream => ((Stream)stream).Close();
        }
    }
}
