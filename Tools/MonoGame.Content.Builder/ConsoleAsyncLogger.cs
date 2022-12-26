// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    internal class ConsoleAsyncLogger : ConsoleLogger
    {
        private ConsoleLogger _logger;
        Queue<ICmd> _commandQueue = new Queue<ICmd>();
        private bool _immediate = false;

        public bool Immediate
        {
            get { return _immediate; }
            set
            {
                if (value != _immediate)
                {
                    if (_immediate) Flush();
                    _immediate = value;
                }
            }
        }

        public ConsoleAsyncLogger(ConsoleLogger logger)
        {
            this._logger = logger;
        }

        //public override string LoggerRootDirectory
        //{
        //    get { return _logger.LoggerRootDirectory; }
        //    set { throw new InvalidOperationException(); }
        //}
        
        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            Enqueue(new LogImportantMessageCmd(message, messageArgs));
        }

        public override void LogMessage(string message, params object[] messageArgs)
        {
            Enqueue(new LogMessageCmd(message, messageArgs));
        }

        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
            Enqueue(new LogWarningCmd(helpLink, contentIdentity, message, messageArgs));
        }
        
        public override void PushFile(string filename)
        {
            Enqueue(new PushFileCmd(filename));
        }

        public override void PopFile()
        {
            Enqueue(new PopFileCmd());
        }

        public override void Indent()
        {
            Enqueue(new IndentCmd());
        }

        public override void Unindent()
        {
            Enqueue(new UnindentCmd());
        }

        private void Enqueue(ICmd cmd)
        {
            _commandQueue.Enqueue(cmd);
            if (Immediate) Flush();
        }

        internal void Flush()
        {
            while(_commandQueue.Count>0)
            {
                var cmd = _commandQueue.Dequeue();
                cmd.Execute(_logger);
            }
        }

        #region command queue
        interface ICmd
        {
            void Execute(ConsoleLogger reciever);
        }
        
        class LogImportantMessageCmd : ICmd
        {
            private string message;
            private object[] messageArgs;
            public LogImportantMessageCmd(string message, object[] messageArgs)
            {
                this.message = message;
                this.messageArgs = messageArgs;
            }
            public void Execute(ConsoleLogger reciever) { reciever.LogImportantMessage(message, messageArgs); }
        }

        class LogMessageCmd : ICmd
        {
            private string message;
            private object[] messageArgs;
            public LogMessageCmd(string message, object[] messageArgs)
            {
                this.message = message;
                this.messageArgs = messageArgs;
            }
            public void Execute(ConsoleLogger reciever) { reciever.LogMessage(message, messageArgs); }
        }
        
        class LogWarningCmd : ICmd
        {
            private string helpLink;
            private ContentIdentity contentIdentity;
            private string message;
            private object[] messageArgs;
            public LogWarningCmd(string helpLink, ContentIdentity contentIdentity, string message, object[] messageArgs)
            {
                this.helpLink = helpLink;
                this.contentIdentity = contentIdentity;
                this.message = message;
                this.messageArgs = messageArgs;
            }
            public void Execute(ConsoleLogger reciever) { reciever.LogWarning(helpLink, contentIdentity, message, messageArgs); }
        }

        class PushFileCmd: ICmd
        {
            private string filename;
            public PushFileCmd(string filename) { this.filename = filename; }
            public void Execute(ConsoleLogger reciever) { reciever.PushFile(filename); }
        }

        class PopFileCmd : ICmd
        {
            public PopFileCmd() { }
            public void Execute(ConsoleLogger reciever) { reciever.PopFile(); }
        }

        class IndentCmd : ICmd
        {
            public IndentCmd() { }
            public void Execute(ConsoleLogger reciever) { reciever.Indent(); }
        }

        class UnindentCmd : ICmd
        {
            public UnindentCmd() { }
            public void Execute(ConsoleLogger reciever) { reciever.Unindent(); }
        }
        

        #endregion
    }
}
