using Snowberry.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Snowberry {

	using Element = Celeste.BinaryPacker.Element;

	class BinaryExporter {

		public static void ExportMap(Map map) {
			Export(map.Export(), "snowberry_map");
		}

		public static void Export(Element e, string filename) {
			string output = Path.Combine(Celeste.Mod.Everest.Loader.PathMods, filename + ".bin");

			var values = new Dictionary<string, short>();
			CreateLookupTable(e, values);
			if(!values.ContainsKey("innerText"))
				values.Add("innerText", (short)values.Count);
			if(!values.ContainsKey("unnamed"))
				values.Add("unnamed", (short)values.Count);

			using var file = File.OpenWrite(output); var writer = new BinaryWriter(file);

			writer.Write("CELESTE MAP");
			writer.Write(filename);
			writer.Write((short)values.Count);

			foreach(var item in values)
				writer.Write(item.Key);

			WriteElement(writer, e, values);

			writer.Flush();
		}

		public static void CreateLookupTable(Element element, Dictionary<string, short> table) {
			void AddValue(string val) { if(val != null && !val.Equals("_eid") && !table.ContainsKey(val)) table.Add(val, (short)table.Count); };
			AddValue(element.Name);
			if(element.Attributes != null)
				foreach(var item in element.Attributes) {
					AddValue(item.Key);
					if(item.Value is string)
						AddValue((string)item.Value);
				}
			if(element.Children != null)
				foreach(var item in element.Children)
					CreateLookupTable(item, table);
		}

		public static void WriteElement(BinaryWriter writer, Element e, Dictionary<string, short> lookup) {
			int attrs = e.Attributes != null ? e.Attributes.Where(k => k.Key != "_eid").Count() : 0;
			int children = e.Children != null ? e.Children.Count : 0;
			writer.Write(lookup[e.Name ?? "unnamed"]);
			writer.Write((byte)attrs);
			if(e.Attributes != null)
				foreach(var attr in e.Attributes)
					if(!attr.Key.Equals("_eid")) {
						ParseValue(attr.Value.ToString(), out byte type, out object result);
						writer.Write(lookup[attr.Key]);
						writer.Write(type);
						switch(type) {
							case 0:
								writer.Write((bool)result);
								continue;
							case 1:
								writer.Write((byte)result);
								continue;
							case 2:
								writer.Write((short)result);
								continue;
							case 3:
								writer.Write((int)result);
								continue;
							case 4:
								writer.Write((float)result);
								continue;
							case 5:
								writer.Write(lookup[(string)result]);
								continue;
							default:
								continue;
						}
					}

			writer.Write((short)children);
			if(e.Children != null)
				foreach(var child in e.Children)
					WriteElement(writer, child, lookup);
		}

		// thanks binary packer
		// try to use the smallest amount of space required
        public static bool ParseValue(string value, out byte type, out object result) {
			if(bool.TryParse(value, out bool asBool)) {
				type = 0;
				result = asBool;
			} else {
				if(byte.TryParse(value, out byte asByte)) {
					type = 1;
					result = asByte;
				} else {
					if(short.TryParse(value, out short asShort)) {
						type = 2;
						result = asShort;
					} else {
						if(int.TryParse(value, out int asInt)) {
							type = 3;
							result = asInt;
						} else {
							if(float.TryParse(value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float result5)) {
								type = 4;
								result = result5;
							} else {
								type = 5;
								result = value;
							}
						}
					}
				}
			}
			return true;
        }
    }
}
