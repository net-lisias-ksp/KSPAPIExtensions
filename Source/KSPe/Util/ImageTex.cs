﻿/*
	This file is part of KSPe, a component for KSP API Extensions/L
	unless when specified otherwise below this code is:

	(C) 2018-19 Lisias T : http://lisias.net <support@lisias.net>

	KSPe API Extensions/L is double licensed, as follows:

	* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
	* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	KSPe API Extensions/L is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
    along with KSPe API Extensions/L. If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
	along with KSPe API Extensions/L. If not, see <https://www.gnu.org/licenses/>.

*/
using System;
using SIO = System.IO;
using TextureFormat = UnityEngine.TextureFormat;
using UTexture2D = UnityEngine.Texture2D;
using DDSHeaders;
using System.Diagnostics;

namespace KSPe.Util.Image {
	public static class Texture2D {
	// This class is derivative work from:
		// Copyright (c) 2013-2016, Maik Schreiber
		// All rights reserved.
		//
		// Redistribution and use in source and binary forms, with or without modification,
		// are permitted provided that the following conditions are met:
		//
		// 1. Redistributions of source code must retain the above copyright notice, this
		//   list of conditions and the following disclaimer.
		//
		// 2. Redistributions in binary form must reproduce the above copyright notice, this
		//    list of conditions and the following disclaimer in the documentation and/or
		//    other materials provided with the distribution.
		//
		// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
		// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
		// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
		// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
		// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
		// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
		// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
		// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
		// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
		// POSSIBILITY OF SUCH DAMAGE.

		// This function will attempt to load either a PNG or a JPG from the specified path.
		// It first checks to see if the actual file is there, if not, it then looks for either a PNG or a JPG
		//
		// easier to specify different cases than to change case to lower.	This will fail on MacOS and Linux
		// if a suffix has mixed case
		private static string[] imgSuffixes = new string[] { ".png", ".jpg", ".gif", ".dds", ".PNG", ".JPG", ".GIF", ".DDS" };
		public static UTexture2D LoadFromFile(String fileNamePath)
		{
			return LoadFromFile(fileNamePath, false);
		}
		public static UTexture2D LoadFromFile(String fileNamePath, bool mipmap)
		{
			UTexture2D tex = null;
			bool validReturn = false;
			bool dds = false;
			try
			{
				string path = fileNamePath;
				if (!SIO.File.Exists(fileNamePath))
				{
					// Look for the file with an appended suffix.
					for (int i = 0; i < imgSuffixes.Length; i++)

						if (SIO.File.Exists(fileNamePath + imgSuffixes[i]))
						{
							path = fileNamePath + imgSuffixes[i];
							dds = imgSuffixes[i] == ".dds" || imgSuffixes[i] == ".DDS";
							break;
						}
				}

				//File Exists check
				if (SIO.File.Exists(path))
				{
					try
					{
						if (dds)
						{
							byte[] bytes = SIO.File.ReadAllBytes(path);

							SIO.BinaryReader binaryReader = new SIO.BinaryReader(new SIO.MemoryStream(bytes));
							uint num = binaryReader.ReadUInt32();

							if (num != DDSValues.uintMagic)
								throw new Error("DDS: File {0} is not a DDS format file!", fileNamePath);

							DDSHeader ddSHeader = new DDSHeader(binaryReader);

							TextureFormat tf = TextureFormat.Alpha8;
							if (ddSHeader.ddspf.dwFourCC == DDSValues.uintDXT1)
								tf = TextureFormat.DXT1;
							if (ddSHeader.ddspf.dwFourCC == DDSValues.uintDXT5)
								tf = TextureFormat.DXT5;
							if (tf == TextureFormat.Alpha8)
								throw new Error("DDS: TextureFormat {0} File {1} is not supported!", tf, fileNamePath);
							tex = LoadDXT(bytes, tf, mipmap);
							validReturn = true;
						}
						else
						{
							validReturn = File.Load(out tex, SIO.File.ReadAllBytes(path));
						}
					}
					catch (Exception ex)
					{
						dbg(ex);
						throw new Error(ex, "Failed to load the texture: {0}", path);
					}
				}
				else
				{
					throw new Error("Cannot find texture to load: {0}", fileNamePath);
				}
			}
			catch (Error ex)
			{
				dbg(ex);
				throw ex;
			}
			catch (Exception ex)
			{
				dbg(ex);
				throw new Error(ex, "Failed to load (are you missing a file?): {0}", fileNamePath);
			}

			// Preventing a memory leak
			if (!validReturn && null != tex) UnityEngine.Object.Destroy(tex);

			return tex;
		}

		private static UTexture2D LoadDXT(byte[] ddsBytes, TextureFormat textureFormat, bool mipmap)
		{
			if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
				throw new Error("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

			byte ddsSizeCheck = ddsBytes[4];
			if (ddsSizeCheck != 124)
				throw new Error("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

			int height = ddsBytes[13] * 256 + ddsBytes[12];
			int width = ddsBytes[17] * 256 + ddsBytes[16];

			int DDS_HEADER_SIZE = 128;
			byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
			Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

			UTexture2D texture = new UTexture2D(width, height, textureFormat, mipmap);
			texture.LoadRawTextureData(dxtBytes);
			texture.Apply();

			return (texture);
		}

		public static bool Exists(string texturePath)
		{
			if (GameDatabase.Instance.ExistsTexture(texturePath))
				return true;
			string fileNamePath = TexPathname(texturePath);
			for (int i = 0; i < imgSuffixes.Length; ++i)
				if (SIO.File.Exists(fileNamePath + imgSuffixes[i]))
					return true;
			return false;
		}

		private static string TexPathname(string path)
		{
			string s =	SIO.Path.Combine(KSPUtil.ApplicationRootPath, "GameData");
			s = SIO.Path.Combine(s,  path);
			return s;
		}

		public static UTexture2D Get(string path, bool mipmap)
		{
			if (!Exists(TexPathname(path)))
				return GameDatabase.Instance.GetTexture(path, false);

			try
			{ 
				return LoadFromFile(TexPathname(path), mipmap);
			}
			catch (Error ex)
			{
				dbg(ex);
				return null;
			}
		}

		[ConditionalAttribute("DEBUG")]
		private static void dbg(string msg, params object[] p)
		{
			UnityEngine.Debug.LogFormat("KSPe.Util.Image.Texture2D: " + msg, p);
		}

		[ConditionalAttribute("DEBUG")]
		private static void dbg(Exception ex)
		{
			UnityEngine.Debug.LogError("KSPe.Util.Image.Texture2D: " + ex.ToString());
			UnityEngine.Debug.LogException(ex);
		}
	}
}
