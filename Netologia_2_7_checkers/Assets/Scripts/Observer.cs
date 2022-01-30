using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class Observer : MonoBehaviour, IObserver
    {
        [SerializeField]
        private bool _read;
        [SerializeField, Range(20f, 70f)]
        private int _delay = 20;
        
        public bool Read { get; private set; }

        private readonly string _r = Environment.CurrentDirectory + "\\ObserverLog.txt";
        private IForwardObserver _forwardObserver;

        private void Start()
        {
            Read = _read;
            _forwardObserver = gameObject.GetComponent<GameManager>();
            if (_read)
                gameObject.GetComponent<PhysicsRaycaster>().enabled = false;
            ReadLog();
        }


        public void WriteInLog(string message)
        {

            if (Read)
                return;

            if (!File.Exists(_r)) 
            {
                var logFile = File.Create(_r);
                logFile.Close();
            }
            using (var stream = new FileStream(_r, FileMode.Append))
            {
                using (var writer = new BinaryWriter(stream, System.Text.Encoding.Default, false))
                {
                    writer.Write(message + "\n");
                }
            }
        }

        public async void ReadLog()
        {
            if (!Read)
                return;
            await Task.Run(() => Thread.Sleep(1000));
            if (File.Exists(_r))
            {
                using (var readerStream = File.OpenRead(_r))
                {
                    using (var binaryReader = new BinaryReader(readerStream))
                    {
                        string name = string.Empty;
                        string secondString;
                        while (binaryReader.PeekChar() != -1)
                        {
                            await Task.Run(() => Thread.Sleep(_delay * 100));
                            secondString = string.Empty;
                            secondString += binaryReader.ReadString();
                            if (secondString.Contains("Player 1"))
                                _forwardObserver.Reproduce("Player 1", null);
                            if (secondString.Contains("Player 2"))
                                _forwardObserver.Reproduce("Player 2", null);
                            if (secondString.Contains("Selected Chip"))
                            {
                                name = secondString.Substring(secondString.Length - 3, 2);
                                _forwardObserver.Reproduce("Chip", name);
                            }
                            if (secondString.Contains("Move to"))
                            {
                                name = secondString.Substring(secondString.Length - 3, 2);
                                _forwardObserver.Reproduce("Move", name);
                            }
                        }
                        Debug.LogWarning("Воспроизведение ходов завершено");
                        UnityEditor.EditorApplication.isPaused = true;
                    }
                }
            }
            else
                Debug.LogWarning("Не найден файл с записью ходов");
        }

    }

    public interface IForwardObserver
    {
        void Write(string b, string name);
        void Reproduce(string a, string name);
    }

    public interface IObserver
    {
        void WriteInLog(string masseg);
        void ReadLog();
        bool Read { get; }
    }
}