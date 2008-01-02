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
using System.ServiceProcess;
using System.Diagnostics;
using System.IO.Ports;
using System.Timers;
using System.Threading;
using DSP;

public class RepeaterService :ServiceBase
	{
	private System.Timers.Timer timer;
	private SerialPort PTTPort = new SerialPort(Repeater.Properties.Settings.Default.PTTCommPort);
	
	private WaveLib.WaveOutPlayer m_Player;
	private WaveLib.WaveInRecorder m_Recorder;
	private WaveLib.FifoStream m_Fifo = new WaveLib.FifoStream();
	private byte[] m_PlayBuffer;
	private byte[] m_RecBuffer;
	private SpeechLib.SpVoiceClass sp;
	private int TimeSincePTT = 0;
	private int IDTimeout = Repeater.Properties.Settings.Default.IDTimeout;
	private int IDTimer = 0;
	private Goertzel gt = new Goertzel();

	public static void Main()
		{
		ServiceBase.Run(new RepeaterService());
		}

	public RepeaterService()
		{
		CanPauseAndContinue = true;
		ServiceName = "K4ANM Repeater Service";
		PTTPort.Handshake = Handshake.None;

		sp = new SpeechLib.SpVoiceClass();

		//the housekeeping timer
		timer = new System.Timers.Timer();
		timer.Interval = 1000; 
		timer.Elapsed += new ElapsedEventHandler(OnTimer);

		
		
				
		}
	//play the samples in the  FIFO buffer 
	private void Filler(IntPtr data, int size)
		{
		if (m_PlayBuffer == null || m_PlayBuffer.Length < size)
			m_PlayBuffer = new byte[size];
		if (m_Fifo.Length >= size)
			m_Fifo.Read(m_PlayBuffer, 0, size);
		else
			for (int i = 0; i < m_PlayBuffer.Length; i++)
				m_PlayBuffer[i] = 0;
		System.Runtime.InteropServices.Marshal.Copy(m_PlayBuffer, 0, data, size);
		}

	private void DataArrived(IntPtr data, int size)
		{
		if (m_RecBuffer == null || m_RecBuffer.Length < size)
			m_RecBuffer = new byte[size];
		System.Runtime.InteropServices.Marshal.Copy(data, m_RecBuffer, 0, size);
		m_Fifo.Write(m_RecBuffer, 0, m_RecBuffer.Length);
		}

	private void Stop()
		{
		if (m_Player != null)
			try
				{
				m_Player.Dispose();
				}
			finally
				{
				m_Player = null;
				}
		if (m_Recorder != null)
			try
				{
				m_Recorder.Dispose();
				}
			finally
				{
				m_Recorder = null;
				}
		m_Fifo.Flush(); // clear all pending data
		}

	private void Start()
		{
		Stop();
		try
			{
			//start the record and playback 
			WaveLib.WaveFormat fmt = new WaveLib.WaveFormat(11025, 16, 1);//11025 b/s, 16 bits 1 channel
			m_Player = new WaveLib.WaveOutPlayer(-1, fmt, 8192, 3, new WaveLib.BufferFillEventHandler(Filler));
			m_Recorder = new WaveLib.WaveInRecorder(-1, fmt, 8192, 3, new WaveLib.BufferDoneEventHandler(DataArrived));
			}
		catch
			{
			Stop();
			throw;
			}
		}
			

	protected override void OnStart(string[] args)
		{
		EventLog.WriteEntry("K4ANM Repeater Service started");
		timer.Enabled = true;
		PTTPort.Open();
		Start();
		//startup announcement
		PTT(true);
		sp.Speak(Repeater.Properties.Settings.Default.IDString, SpeechLib.SpeechVoiceSpeakFlags.SVSFDefault);
		PTT(false);
		}

	protected override void OnStop()
		{
		EventLog.WriteEntry("K4ANM Repeater Service stopped");
		PTTPort.Close();
		Stop();
		timer.Enabled = false;
		}

	protected override void OnPause()
		{
		EventLog.WriteEntry("K4ANM Repeater Service paused");
		Stop();
		timer.Enabled = false;
		}

	protected override void OnContinue()
		{
		EventLog.WriteEntry("K4ANM Repeater Service continued");
		timer.Enabled = true;
		Start();
		}

	protected void OnTimer(Object source, ElapsedEventArgs e)
		{
		//EventLog.WriteEntry("K4ANM Repeater Service Timed Out!");
		//TODO: Add housekeeping code here:
		//Send ID if no traffic, etc/.
		//Hang Timer
		//
		//Do Station ID
			if (IDTimer >= IDTimeout)
			{
				IDTimer = 0;
				DoID();
			}

		IDTimer++;
		TimeSincePTT++;

		}
	protected void PTT(bool state)
		{
		switch (state)
			{
			case (true):
							
				PTTPort.DtrEnable = true;
				PTTPort.RtsEnable = true;
				Thread.Sleep(100);
				break;
			case (false):
				Thread.Sleep(100);
				PTTPort.DtrEnable = false;
				PTTPort.RtsEnable = false;
				//send roger beep here
				TimeSincePTT = 0;
				break;
			default:
				break;
			}

		}
	void DoID()
		{
		PTT(true);
		sp.Speak(Repeater.Properties.Settings.Default.IDString, SpeechLib.SpeechVoiceSpeakFlags.SVSFDefault);
		PTT(false);
		
		}


	}
