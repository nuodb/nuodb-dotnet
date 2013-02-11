/****************************************************************************
* Copyright (c) 2012-2013, NuoDB, Inc.
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*   * Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*   * Redistributions in binary form must reproduce the above copyright
*     notice, this list of conditions and the following disclaimer in the
*     documentation and/or other materials provided with the distribution.
*   * Neither the name of NuoDB, Inc. nor the names of its contributors may
*     be used to endorse or promote products derived from this software
*     without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL NUODB, INC. BE LIABLE FOR ANY DIRECT, INDIRECT,
* INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
* LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
* OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
* LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
* OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
* ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
****************************************************************************/

using System;
using System.Security.Cryptography;

namespace System.Data.NuoDB.Security
{

    class RemoteGroup
	{
		internal BigInteger prime;
		internal BigInteger generator;
		internal BigInteger k;
		internal static RemoteGroup group;
		internal static Random random;

		static RemoteGroup()
		{
			random = new Random((int)DateTime.Now.Ticks);
		}

		public RemoteGroup(string primeString, string generatorString)
		{
            SHA1Managed sha1 = new SHA1Managed();

			generator = new BigInteger(2);
			byte[] primeBytes = RemotePassword.getBytes(primeString);
			prime = new BigInteger(1, primeBytes);

			byte[] generatorBytes = generator.ToByteArray();
			int pad = primeBytes.Length - generatorBytes.Length;
            byte[] buffer = new byte[primeBytes.Length + Math.Max(0, pad) + generatorBytes.Length];
            Array.Copy(primeBytes, 0, buffer, 0, primeBytes.Length);
            Array.Copy(generatorBytes, 0, buffer, primeBytes.Length + Math.Max(0, pad), generatorBytes.Length);
            byte[] kBytes = sha1.ComputeHash(buffer);
			k = new BigInteger(1, kBytes);
		}

		public static RemoteGroup getGroup(int groupSize)
		{
			if (group == null)
			{
				string prime = "EEAF0AB9ADB38DD69C33F80AFA8FC5E86072618775FF3C0B9EA2314C" + "9C256576D674DF7496EA81D3383B4813D692C6E0E0D5D8E250B98BE4" + "8E495C1D6089DAD15DC7D7B46154D6B6CE8EF4AD69B15D4982559B29" + "7BCF1885C529F566660E57EC68EDBC3C05726CC02FD4CBF4976EAA9A" + "FD5138FE8376435B9FC61D2FC0EB06E3";

				group = new RemoteGroup(prime, "02");
			}

			return group;
		}
	}

}