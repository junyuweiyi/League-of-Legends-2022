/********************************************************************
	created:	2020/10/30
	author:		ZYL
	
	purpose:	CTR模式对称加密
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace iFramework
{
    public class AESCryptor
    {
        private AesManaged _aes;
        private ICryptoTransform _cryptor;
        public AESCryptor(byte[] key)
        {
            _aes = new AesManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None
            };

            _cryptor = new CounterModeCryptoTransform(_aes, key, key);
        }

        public byte[] Crypt(byte[] inputbuffer)
        {
            return _cryptor.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
        }

        public byte[] Crypt(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            return _cryptor.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
        }

        public int Crypt(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _cryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }
    }

    public class CounterModeCryptoTransform : ICryptoTransform
    {
        private readonly byte[] _counter;
        private readonly ICryptoTransform _counterEncryptor;
        private readonly Queue<byte> _xorMask = new Queue<byte>();
        private readonly SymmetricAlgorithm _symmetricAlgorithm;
        private byte[] _counterModeBlock;

        public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[] counter)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            _symmetricAlgorithm = symmetricAlgorithm ?? throw new ArgumentNullException(nameof(symmetricAlgorithm));

            _counter = new byte[counter.Length];
            Buffer.BlockCopy(counter, 0, _counter, 0, counter.Length);
            _counterModeBlock = new byte[_symmetricAlgorithm.BlockSize / 8];

            var zeroIv = new byte[_symmetricAlgorithm.BlockSize / 8];
            _counterEncryptor = symmetricAlgorithm.CreateEncryptor(key, zeroIv);
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var output = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
            return output;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            for (var i = 0; i < inputCount; i++)
            {
                if (NeedMoreXorMaskBytes())
                {
                    EncryptCounterThenIncrement();
                }

                var mask = _xorMask.Dequeue();
                outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ mask);
            }

            return inputCount;
        }

        private bool NeedMoreXorMaskBytes()
        {
            return _xorMask.Count == 0;
        }

        private void EncryptCounterThenIncrement()
        {
            _counterEncryptor.TransformBlock(_counter, 0, _counter.Length, _counterModeBlock, 0);
            IncrementCounter();

            foreach (var b in _counterModeBlock)
            {
                _xorMask.Enqueue(b);
            }
        }

        private void IncrementCounter()
        {
            for (var i = _counter.Length - 1; i >= 0; i--)
            {
                if (++_counter[i] != 0)
                    break;
            }
        }

        public int InputBlockSize => _symmetricAlgorithm.BlockSize / 8;
        public int OutputBlockSize => _symmetricAlgorithm.BlockSize / 8;
        public bool CanTransformMultipleBlocks => true;
        public bool CanReuseTransform => false;

        public void Dispose()
        {
            _counterEncryptor.Dispose();
        }
    }
}