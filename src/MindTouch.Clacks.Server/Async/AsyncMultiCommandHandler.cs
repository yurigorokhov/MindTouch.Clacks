/*
 * MindTouch.Clacks
 * 
 * Copyright (C) 2011-2013 Arne F. Claassen
 * geekblog [at] claassen [dot] net
 * http://github.com/sdether/MindTouch.Clacks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Net;

namespace MindTouch.Clacks.Server.Async {
    public class AsyncMultiCommandHandler : IAsyncCommandHandler {
        private readonly IPEndPoint _client;
        private readonly string _command;
        private readonly string[] _arguments;
        private readonly Action<IRequest, Action<IResponse, Action>> _handler;
        private readonly Action<IRequest, Exception, Action<IResponse>> _errorHandler;
        private readonly int _dataLength;
        private int _received;
        private List<byte[]> _dataChunks;

        public AsyncMultiCommandHandler(IPEndPoint client, string command, string[] arguments, int dataLength, Action<IRequest, Action<IResponse, Action>> handler, Action<IRequest, Exception, Action<IResponse>> errorHandler) {
            _client = client;
            _command = command;
            _arguments = arguments;
            _dataLength = dataLength;
            _handler = handler;
            _errorHandler = errorHandler;
        }

        public void Dispose() { }

        public bool ExpectsData { get { return _dataLength > 0; } }
        public bool DisconnectOnCompletion { get { return false; } }
        public int OutstandingBytes { get { return _dataLength - _received; } }
        public string Command { get { return _command; } }

        public void AcceptData(byte[] chunk) {
            if(_dataChunks == null) {
                _dataChunks = new List<byte[]>();
            }
            _dataChunks.Add(chunk);
            _received += chunk.Length;
            if(_received > _dataLength) {
                throw new DataExpectationException(_dataLength, _received);
            }
        }

        public void GetResponse(Action<IResponse, Action> responseCallback) {
            if(_received < _dataLength) {
                var badRequest = new Request(_client, _command, _arguments, 0, new List<byte[]>());
                _errorHandler(badRequest, new DataExpectationException(_dataLength, _received), response => responseCallback(response, null));
                return; 
            }
            var request = new Request(_client, _command, _arguments, _dataLength, _dataChunks);
            try {
                _handler(request, responseCallback);
            } catch(Exception e) {
                _errorHandler(request, e, response => responseCallback(response, null));
            }
        }
    }
}