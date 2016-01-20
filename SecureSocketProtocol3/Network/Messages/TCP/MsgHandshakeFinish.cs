﻿using ProtoBuf;
using SecureSocketProtocol3.Security.Handshakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
    Secure Socket Protocol
    Copyright (C) 2016 AnguisCaptor

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace SecureSocketProtocol3.Network.Messages.TCP
{
    [ProtoContract]
    internal class MsgHandshakeFinish : IMessage
    {
        public MsgHandshakeFinish()
            : base()
        {

        }

        public override void ProcessPayload(SSPClient client, OperationalSocket OpSocket)
        {
            if (client != null && client.handshakeSystem != null)
            {
                Handshake curHandshake = client.handshakeSystem.GetCurrentHandshake();

                if (curHandshake != null)
                {
                    curHandshake.InternalFinish(false);
                }
            }
        }
    }
}