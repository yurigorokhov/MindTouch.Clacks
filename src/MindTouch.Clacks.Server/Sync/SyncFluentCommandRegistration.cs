﻿/*
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

namespace MindTouch.Clacks.Server.Sync {
    public class SyncFluentCommandRegistration : ISyncFluentCommandRegistration {
        private readonly ServerBuilder _serverBuilder;
        private readonly SyncCommandRepository _repository;
        private readonly string _command;
        private DataExpectation _dataExpectation = DataExpectation.Auto;

        public SyncFluentCommandRegistration(ServerBuilder serverBuilder, SyncCommandRepository repository, string command) {
            _serverBuilder = serverBuilder;
            _repository = repository;
            _command = command;
        }

        public ISyncServerBuilder HandledBy(Func<IRequest, IResponse> handler) {
            _repository.AddCommand(_command, handler, _dataExpectation);
            return _serverBuilder;
        }

        public ISyncServerBuilder HandledBy(Func<IRequest, IEnumerable<IResponse>> handler) {
            _repository.AddCommand(_command, handler, _dataExpectation);
            return _serverBuilder;
        }

        public ISyncFluentCommandRegistration ExpectsData() {
            _dataExpectation = DataExpectation.Always;
            return this;
        }

        public ISyncFluentCommandRegistration ExpectsNoData() {
            _dataExpectation = DataExpectation.Never;
            return this;
        }
    }
}