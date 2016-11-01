// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.Text
{
    /// <summary>
	///     Base 32 Encoding.
	/// </summary>
    public static class Base32Encoding
    {
        /// <summary>
        /// The base 32 alphabet.
        /// </summary>
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        /// <summary>
        /// The number of bits in a byte.
        /// </summary>
        private const int BitsPerByte = 8;

        /// <summary>
        /// Bits per encoded block.
        /// </summary>
        private const int BitsPerBase32EncodedBlock = 5;


        /// <summary>
        //  Returns an array of bytes from a Base32 encoded string.
        /// </summary>
        /// <param name="value">A base32 encoded string.</param>
        /// <returns>The decoded byte array.</returns>
        public static byte[] FromBase32String(string value)
        {                        
            if (value == null)
            {
                return null;
            }            

            if (value == string.Empty)
            {
                return new byte[0];
            }                        

            int bitCount = (value.Length * BitsPerBase32EncodedBlock / BitsPerByte) * BitsPerByte;

            if (bitCount == 0)
            {                
                throw new FormatException("The input is not a valid Base32 string.");
            }

            // Create a bit array of the appropriate size
            var outputBitArray = new BitArray(bitCount);

            // Start at the end of the array
            int currentBitIndex = bitCount - 1;            

            foreach (char c in value.ToUpperInvariant())
            {                
                int index = Base32Alphabet.IndexOf(c);
             
                if (index < 0)
                {
                    throw new FormatException(string.Format("The input is not a valid Base32 string. The character '{0}' does not exist in the Base32 alphabet.", c));
                }

                var outputByte = (byte)index;

                // Get the bits in each block and add them to the bitarray
                for (var i = BitsPerBase32EncodedBlock - 1; i >= 0; i--)
                {
                    unchecked
                    {
                        var mask = (byte)(1 << i);

                        // Determine if the bit is set and set the appropriate value in the bitarray
                        bool isSet = ((byte)(outputByte & mask) == mask);    
                                            
                        outputBitArray.Set(currentBitIndex, isSet);
                    }

                    currentBitIndex--;

                    if (currentBitIndex < 0)
                    {
                        break;
                    }
                }

                if (currentBitIndex < 0)
                {
                    break;
                }
            }

            // Encode the bit array as 8-bit bytes and return
            return EncodeBitArray(outputBitArray, BitsPerByte).ToArray();
        }

        /// <summary>
        /// Encodes a bit array using to a list of bytes using the specified output bit count.
        /// </summary>
        /// <param name="bitArray"></param>
        /// <param name="outputBitCount"></param>
        /// <returns></returns>
        private static List<byte> EncodeBitArray(BitArray bitArray, int outputBitCount)
        {
            var outputBytes = new List<byte>();

            byte outputValue = 0;
            var outputIndex = -1;

            for (var i = bitArray.Length - 1; i >= 0; i--)
            {
                if (outputIndex < 0)
                {
                    outputValue = 0;
                    outputIndex = outputBitCount - 1;
                }

                var value = bitArray.Get(i);

                unchecked
                {
                    var mask = (byte)(1 << outputIndex);

                    outputValue = (byte)(value ? (outputValue | mask) : (outputValue & ~mask));
                }

                outputIndex--;

                if (outputIndex < 0)
                {
                    outputBytes.Add(outputValue);
                }
            }

            if (outputIndex >= 0)
            {
                outputBytes.Add(outputValue);
            }

            return outputBytes;
        }

        /// <summary>
        //  Converts an array of bytes to its equivalent Base32 string representation.        
        /// </summary>
        /// <param name="bytes">The bytes to encode.</param>
        /// <returns>A Base32 encoded string.</returns>
        public static string ToBase32String(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            // Get all the input bytes as a bit array
            var inputBitArray = new BitArray(bytes.Reverse().ToArray());            

            // Encode all the bytes as a list of 5-bit blocks
            var outputBytes = EncodeBitArray(inputBitArray, BitsPerBase32EncodedBlock);
            
            var outputStringBuilder = new StringBuilder();

            // Use each of the 5-bit blocks as an index into the alphabet and return
            foreach (byte b in outputBytes)
            {
                outputStringBuilder.Append(Base32Alphabet[b]);
            }            

            return outputStringBuilder.ToString();
        }
    }
}