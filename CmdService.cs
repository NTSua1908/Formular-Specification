﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Formular_Specification
{
    public class CmdService : IDisposable
    {
        private Process _cmdProcess;
        private StreamWriter _streamWriter;
        private AutoResetEvent _outputWaitHandle;
        private string _cmdOutput;

        public CmdService(string cmdPath)
        {
            _cmdProcess = new Process();
            _outputWaitHandle = new AutoResetEvent(false);
            _cmdOutput = String.Empty;

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = cmdPath;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.CreateNoWindow = true;

            _cmdProcess.OutputDataReceived += _cmdProcess_OutputDataReceived;

            _cmdProcess.StartInfo = processStartInfo;
            _cmdProcess.Start();

            _streamWriter = _cmdProcess.StandardInput;
            //_streamReader = _cmdProcess.StandardOutput;
            _cmdProcess.BeginOutputReadLine();
            //_streamReader.ReadLine();
        }

        public string ExecuteCommand(string command)
        {
            _cmdOutput = String.Empty;

            _streamWriter.WriteLine(command);
            _streamWriter.WriteLine("echo end");
            _outputWaitHandle.WaitOne();
            //_streamReader.ReadLine();
            return _cmdOutput;
        }

        private void _cmdProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null || e.Data == "end")
                _outputWaitHandle.Set();
            else
                _cmdOutput += e.Data + Environment.NewLine;
        }

        public void Dispose()
        {
            _cmdProcess.Close();
            _cmdProcess.Dispose();
            _streamWriter.Close();
            _streamWriter.Dispose();
        }
    }

}
