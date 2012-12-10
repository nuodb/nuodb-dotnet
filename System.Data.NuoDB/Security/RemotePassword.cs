/****************************************************************************
* Copyright (c) 2012, NuoDB, Inc.
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


	/*
	 * Order of battle for SRP handshake:
	 * 
	 * 													0.  At account creation, the server generates
	 * 														a random salt and computes a password 
	 * 														verifier from the account name, password,
	 * 														and salt.
	 * 
	 * 		1. Client generates random number
	 * 		   as private key, computes public
	 * 		   key.
	 * 
	 * 		2. Client sends server the account 
	 * 		   name and its public key.
	 * 													3.  Server receives account name, looks up
	 * 														salt and password verifier.  Server
	 * 														generates random number as private key.
	 * 														Server computes public key from private
	 * 														key, account name, verifier, and salt.
	 * 
	 * 													4.  Server sends client public key and salt
	 * 
	 * 		3. Client receives server public
	 * 		   key and computes session key
	 * 		   from server key, salt, account
	 * 		   name, and password.
	 * 												5.  Server computes session key from client
	 * 														public key, client name, and verifier
	 * 
	 * 		For full details, see http://www.ietf.org/rfc/rfc5054.txt
	 * 
	 */

	public class RemotePassword
	{
		internal static byte[] hexDigits;
		internal SHA1Managed sha1;
		internal BigInteger prime;
		internal BigInteger generator;
		internal BigInteger k;
		internal BigInteger clientPrivateKey;
		internal BigInteger clientPublicKey;
		internal BigInteger serverPrivateKey;
		internal BigInteger serverPublicKey;
		internal BigInteger scramble;
		internal Random random;

		static RemotePassword()
		{
			hexDigits = new byte[256];

			for (int n = 0; n < 10; ++n)
			{
				hexDigits['0' + n] = (byte) n;
			}

			for (sbyte n = 0; n < 6; ++n)
			{
				hexDigits['a' + n] = (byte)(10 + n);
				hexDigits['A' + n] = (byte)(10 + n);
			}
		}

		public RemotePassword()
		{
			RemoteGroup group = RemoteGroup.getGroup(1024);
			prime = group.prime;
			generator = group.generator;
			k = group.k;
			random = RemoteGroup.random;
            sha1 = new SHA1Managed();
		}

		/// <param name="args"> </param>
		public static void Main(string[] args)
		{
			string saltString = "BEB25379D1A8581EB5A727673A2441EE";
			string a = "60975527035CF2AD1989806F0407210BC81EDC04E2762A56AFD529DDDA2D4393";
			string b = "E487CB59D31AC550471E81F00F6928E01DDA08E974A004F49E61F5D105284D20";
			RemotePassword server = new RemotePassword();
			RemotePassword client = new RemotePassword();
			string verifier = server.computeVerifier("alice", "password123", saltString);
			string clientKey = client.setClientPrivateKey(a);
			string serverKey = server.setServerPrivateKey(b, verifier);
			byte[] key1 = client.computeSessionKey("alice", "password123", saltString, serverKey);
			byte[] key2 = server.computeSessionKey(clientKey, verifier);

			Console.WriteLine();
		}

		public static BigInteger getBigInteger(string hex)
		{
			return new BigInteger(1, getBytes(hex));
		}

		public static byte[] getBytes(string hex)
		{
			int length = hex.Length / 2;
			byte[] bytes = new byte[length];

			for (int n = 0, c = 0; n < length; ++n, c += 2)
			{
				bytes[n] = (byte)((hexDigits[hex[c]] << 4) | hexDigits[hex[c + 1]]);
			}

			return bytes;
		}

		public static string getHex(BigInteger number)
		{
			return getHex(number.ToByteArray());
		}

		public static string getHex(byte[] rep)
		{
			int n = 0;
			int length = rep.Length;

			if (rep[0] == 0)
			{
				++n;
				--length;
			}

			char[] hex = new char[length * 2];

			for (int c = 0; n < rep.Length; ++n)
			{
				int b = rep[n] & 0xff;
				int high = b >> 4;
				int low = b & 0xf;
				hex[c++] = (char)((high < 10) ? '0' + high : 'A' + high - 10);
				hex[c++] = (char)((low < 10) ? '0' + low : 'A' + low - 10);
			}

			return new string(hex);
		}

		public virtual BigInteger getUserHash(string account, string password, string salt)
		{
            char[] accountBuff = account.ToCharArray();
            char[] passwordBuff = password.ToCharArray();
            byte[] buffer = new byte[accountBuff.Length + 1 + passwordBuff.Length];
            int i = 0;
            foreach (char c in accountBuff)
                buffer[i++] = (byte)c;
            buffer[i++] = (byte)':';
            foreach (char c in passwordBuff)
                buffer[i++] = (byte)c;

            byte[] hash1 = sha1.ComputeHash(buffer);
			byte[] saltBytes = getBytes(salt);

            byte[] buffer2 = new byte[saltBytes.Length + hash1.Length];
            Array.Copy(saltBytes, 0, buffer2, 0, saltBytes.Length);
            Array.Copy(hash1, 0, buffer2, saltBytes.Length, hash1.Length);

            byte[] hash2 = sha1.ComputeHash(buffer2);
			return new BigInteger(1, hash2);
		}

		public virtual string computeVerifier(string account, string password, string salt)
		{
			BigInteger x = getUserHash(account, password, salt);
			BigInteger verifier = generator.modPow(x, prime);
			byte[] result = verifier.ToByteArray();

			return getHex(result);
		}

		public virtual string setClientPrivateKey(string key)
		{
			clientPrivateKey = new BigInteger(1, getBytes(key));
			clientPublicKey = generator.modPow(clientPrivateKey, prime);

			return getHex(clientPublicKey);
		}

		public virtual string genClientKey()
		{
			clientPrivateKey = new BigInteger(256, random);
			clientPublicKey = generator.modPow(clientPrivateKey, prime);

			return getHex(clientPublicKey);
		}

		public virtual string setServerPrivateKey(string key, string verifier)
		{
			return genServerKey(new BigInteger(1, getBytes(key)), verifier);
		}

		public virtual string genServerKey(BigInteger privateKey, string verifier)
		{
			serverPrivateKey = privateKey; // b
			BigInteger gb = generator.modPow(serverPrivateKey, prime); // g^b
			BigInteger v = new BigInteger(1, getBytes(verifier)); // v
			BigInteger kv = k * v;
			kv = kv % prime;
			serverPublicKey = kv + gb;
			serverPublicKey = serverPublicKey % prime;

			return getHex(serverPublicKey);
		}

		public virtual string genServerKey(string verifier)
		{
			return genServerKey(new BigInteger(256, random), verifier);
		}

		public virtual void computeScramble()
		{
            byte[] client = clientPublicKey.ToByteArray();
            byte[] server = serverPublicKey.ToByteArray();

			int n1 = (client[0] == 0) ? 1 : 0;
			int n2 = (server[0] == 0) ? 1 : 0;
            byte[] buffer = new byte[client.Length - n1 + server.Length - n2];
            Array.Copy(client, n1, buffer, 0, client.Length - n1);
            Array.Copy(server, n2, buffer, client.Length - n1, server.Length - n2);
            byte[] hash = sha1.ComputeHash(buffer);
            scramble = new BigInteger(1, hash);
        }

		public virtual byte[] computeSessionKey(string account, string password, string salt, string serverPubKey)
		{
			serverPublicKey = getBigInteger(serverPubKey);
            computeScramble();
            BigInteger x = getUserHash(account, password, salt); // x
            BigInteger gx = generator.modPow(x, prime); // g^x
            BigInteger kgx = (k * gx) % prime; // kg^x
            BigInteger diff = (serverPublicKey - kgx) % prime; // B - kg^x
            BigInteger ux = (scramble * x) % prime; // ux
            BigInteger aux = (clientPrivateKey + ux) % prime; // A + ux
            BigInteger sessionSecret = diff.modPow(aux, prime); // (B - kg^x) ^ (a + ux)
            byte[] secret = sessionSecret.ToByteArray();
			int n = (secret[0] == 0) ? 1 : 0;

            return sha1.ComputeHash(secret, n, secret.Length - n);
		}

		// Server session key

		public virtual byte[] computeSessionKey(string clientPubKey, string verifier)
		{
			clientPublicKey = getBigInteger(clientPubKey);
			computeScramble();
			BigInteger v = getBigInteger(verifier);
			BigInteger vu = v.modPow(scramble, prime); // v^u
			BigInteger Avu = (clientPublicKey * vu) % prime; // Av^u
			BigInteger sessionSecret = Avu.modPow(serverPrivateKey, prime); // (Av^u) ^ b
			byte[] secret = sessionSecret.ToByteArray();
			int n = (secret[0] == 0) ? 1 : 0;

            return sha1.ComputeHash(secret, n, secret.Length - n);
		}

		public virtual string genSalt()
		{
			BigInteger n = new BigInteger(256, random);

			return getHex(n);
		}

	}

}