/*
Copyright © 2007 Nik Martin, K4ANM <><

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.IO;
using System.Collections;

namespace WaveLib
{
	public class FifoStream : Stream
	{
		private const int BlockSize = 65536;
		private const int MaxBlocksInCache = (3 * 1024 * 1024) / BlockSize;

		private int m_Size;
		private int m_RPos;
		private int m_WPos;
		private Stack m_UsedBlocks = new Stack();
		private ArrayList m_Blocks = new ArrayList(); 

		private byte[] AllocBlock()
		{
			byte[] Result = null;
			Result = m_UsedBlocks.Count > 0 ? (byte[])m_UsedBlocks.Pop() : new byte[BlockSize];
			return Result;
		}
		private void FreeBlock(byte[] block)
		{
			if (m_UsedBlocks.Count < MaxBlocksInCache)
				m_UsedBlocks.Push(block);
		}
		private byte[] GetWBlock()
		{
			byte[] Result = null;
			if (m_WPos < BlockSize && m_Blocks.Count > 0)
				Result = (byte[])m_Blocks[m_Blocks.Count - 1];
			else
			{
				Result = AllocBlock();
				m_Blocks.Add(Result);
				m_WPos = 0;
			}
			return Result;
		}

		// Stream members
		public override bool CanRead
		{
			get { return true; }
		}
		public override bool CanSeek
		{
			get { return false; }
		}
		public override bool CanWrite
		{
			get { return true; }
		}
		public override long Length
		{
			get 
			{ 
				lock(this)
					return m_Size;
			}
		}
		public override long Position
		{
			get { throw new InvalidOperationException(); }
			set { throw new InvalidOperationException(); }
		}
		public override void Close()
		{
			Flush();
		}
		public override void Flush()
		{
			lock(this)
			{
				foreach (byte[] block in m_Blocks)
					FreeBlock(block);
				m_Blocks.Clear();
				m_RPos = 0;
				m_WPos = 0;
				m_Size = 0;
			}
		}
		public override void SetLength(long len)
		{
			throw new InvalidOperationException();
		}
		public override long Seek(long pos, SeekOrigin o)
		{
			throw new InvalidOperationException();
		}
		public override int Read(byte[] buf, int ofs, int count)
		{
			lock(this)
			{
				int Result = Peek(buf, ofs, count);
				Advance(Result);
				return Result;
			}
		}
		public override void Write(byte[] buf, int ofs, int count)
		{
			lock(this)
			{
				int Left = count;
				while (Left > 0)
				{
					int ToWrite = Math.Min(BlockSize - m_WPos, Left);
					Array.Copy(buf, ofs + count - Left, GetWBlock(), m_WPos, ToWrite);
					m_WPos += ToWrite;
					Left -= ToWrite;
				}
				m_Size += count;
			}
		}

		// extra stuff
		public int Advance(int count)
		{
			lock(this)
			{
				int SizeLeft = count;
				while (SizeLeft > 0 && m_Size > 0)
				{
					if (m_RPos == BlockSize)
					{
						m_RPos = 0;
						FreeBlock((byte[])m_Blocks[0]);
						m_Blocks.RemoveAt(0);
					}
					int ToFeed = m_Blocks.Count == 1 ? Math.Min(m_WPos - m_RPos, SizeLeft) : Math.Min(BlockSize - m_RPos, SizeLeft);
					m_RPos += ToFeed;
					SizeLeft -= ToFeed;
					m_Size -= ToFeed;
				}
				return count - SizeLeft;
			}
		}
		public int Peek(byte[] buf, int ofs, int count)
		{
			lock(this)
			{
				int SizeLeft = count;
				int TempBlockPos = m_RPos;
				int TempSize = m_Size;

				int CurrentBlock = 0;
				while (SizeLeft > 0 && TempSize > 0)
				{
					if (TempBlockPos == BlockSize)
					{
						TempBlockPos = 0;
						CurrentBlock++;
					}
					int Upper = CurrentBlock < m_Blocks.Count - 1 ? BlockSize : m_WPos;
					int ToFeed = Math.Min(Upper - TempBlockPos, SizeLeft);
					Array.Copy((byte[])m_Blocks[CurrentBlock], TempBlockPos, buf, ofs + count - SizeLeft, ToFeed);
					SizeLeft -= ToFeed;
					TempBlockPos += ToFeed;
					TempSize -= ToFeed;
				}
				return count - SizeLeft;
			}
		}
	}
}
