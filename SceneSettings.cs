//	Copyright (c) 2015, Warren Marshall <warren@warrenmarshall.biz>
//	Copyright (c) 2015, Aleksander Marhall
//	
//	Permission to use, copy, modify, and/or distribute this software for any
//	purpose with or without fee is hereby granted, provided that the above
//	copyright notice and this permission notice appear in all copies.
//
//	THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
//	WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
//	MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
//	ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
//	WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
//	ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
//	OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.IO;

namespace OBJ2MAP
{
    class SceneSettings
    {
        MainForm progForm;

		public enum EFieldNames {
                MapOutput,
                BrushMethod,
                CopyToClipboard,
                Depth,
                Scale,
                DecimalPlaces,
                Class,
                VisibleTexture,
                HiddenTexture,
                AxisAligned,
                MapVersion,
                WADOption,
                WADPath,
                wadTextureSizeX,
                wadTextureSizeY
        };

        public SceneSettings(MainForm form)
        {
            progForm = form;
        }

        public static bool SettingsSave(
            string output,
            MainForm.EConvOption econvOption,
            bool checked3,
            double _Scalar,
            float num2,
            int num1,
            string text3,
            string str1,
            string str2,
            string text1,
            bool axis,
            MainForm.MapVersion mapVersion,
            MainForm.WADOption wadOption,
            string WADPath,
            string wadTextureSizeX,
            string wadTextureSizeY
            )
        {
            XElement settings = new XElement("OBJ2MAPSettings",
                new XElement("MAPOutput", @output),
                new XElement("BrushMethod", @econvOption.ToString()),
                new XElement("CopyToClipboard", @checked3.ToString()),
                new XElement("Depth", @_Scalar),
                new XElement("Scale", @num2),
                new XElement("DecimalPlaces", @num1),
                new XElement("Class", @text3),
                new XElement("VisibleTexture", @str1),
                new XElement("HiddenTexture", @str2),
                new XElement("AxisAligned", @axis.ToString()),
                new XElement("MapVersion", @mapVersion.ToString()),
                new XElement("WADOption", @wadOption.ToString()),
                new XElement("WADPath", @WADPath),
                new XElement("wadTextureSizeX", @wadTextureSizeX),
                new XElement("wadTextureSizeY", @wadTextureSizeY)
            );

            settings.Save(Path.Combine(Path.GetDirectoryName(text1), Path.GetFileNameWithoutExtension(text1) + ".xml"));

            string test = text3.ToString();
            return true;
        }

        static XElement loadFile;

        public static void SetPathForLoading(string text1)
        {
            loadFile = XElement.Load(Path.Combine(Path.GetDirectoryName(text1), Path.GetFileNameWithoutExtension(text1) + ".xml"));
        }

		public static string SettingsLoad(EFieldNames mode)
        {
			if (loadFile != null)
			{
                XElement element = new XElement("null");
				switch (mode)
				{
					case EFieldNames.MapOutput:
                        element = loadFile.Element("MAPOutput");
                        break;
                    case EFieldNames.BrushMethod:
                        element = loadFile.Element("BrushMethod");
                        break;
                    case EFieldNames.CopyToClipboard:
                        element = loadFile.Element("CopyToClipboard");
                        break;
                    case EFieldNames.Depth:
                        element = loadFile.Element("Depth");
                        break;
                    case EFieldNames.Scale:
                        element = loadFile.Element("Scale");
                        break;
                    case EFieldNames.DecimalPlaces:
                        element = loadFile.Element("DecimalPlaces");
                        break;
                    case EFieldNames.Class:
                        element = loadFile.Element("Class");
                        break;
                    case EFieldNames.VisibleTexture:
                        element = loadFile.Element("VisibleTexture");
                        break;
                    case EFieldNames.HiddenTexture:
                        element = loadFile.Element("HiddenTexture");
                        break;
                    case EFieldNames.AxisAligned:
                        element = loadFile.Element("AxisAligned");
                        break;
                    case EFieldNames.MapVersion:
                        element = loadFile.Element("MapVersion");
                        break;
                    case EFieldNames.WADOption:
                        element = loadFile.Element("WADOption");
                        break;
                    case EFieldNames.WADPath:
                        element = loadFile.Element("WADPath");
                        break;
                    case EFieldNames.wadTextureSizeX:
                        element = loadFile.Element("wadTextureSizeX");
                        break;
                    case EFieldNames.wadTextureSizeY:
                        element = loadFile.Element("wadTextureSizeY");
                        break;
                }

                if (element != null)
                {
                    return element.Value;
                }
            }

			return "";
        }
    }
}
