﻿/*
    Copyright (C) 2011-2013 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using dnlib.DotNet;
using de4dot.blocks;

namespace de4dot.code.deobfuscators.Rummage {
	public class DeobfuscatorInfo : DeobfuscatorInfoBase {
		public const string THE_NAME = "Rummage";
		public const string THE_TYPE = "rm";
		const string DEFAULT_REGEX = @"!.";
		public DeobfuscatorInfo()
			: base(DEFAULT_REGEX) {
		}

		public override string Name {
			get { return THE_NAME; }
		}

		public override string Type {
			get { return THE_TYPE; }
		}

		public override IDeobfuscator createDeobfuscator() {
			return new Deobfuscator(new Deobfuscator.Options {
				ValidNameRegex = validNameRegex.get(),
			});
		}
	}

	class Deobfuscator : DeobfuscatorBase {
		string obfuscatorName = DeobfuscatorInfo.THE_NAME;
		StringDecrypter stringDecrypter;

		internal class Options : OptionsBase {
		}

		public override string Type {
			get { return DeobfuscatorInfo.THE_TYPE; }
		}

		public override string TypeLong {
			get { return DeobfuscatorInfo.THE_NAME; }
		}

		public override string Name {
			get { return obfuscatorName; }
		}

		public Deobfuscator(Options options)
			: base(options) {
		}

		protected override int detectInternal() {
			int val = 0;

			int sum = toInt32(stringDecrypter.Detected);
			if (sum > 0)
				val += 100 + 10 * (sum - 1);

			return val;
		}

		protected override void scanForObfuscator() {
			stringDecrypter = new StringDecrypter(module);
			stringDecrypter.find();
			detectVersion();
		}

		void detectVersion() {
			string version;
			switch (stringDecrypter.Version) {
			case RummageVersion.V1_1_445: version = "v1.1 - v2.0"; break;
			case RummageVersion.V2_1_729: version = "v2.1 - v2.2"; break;
			default: version = null; break;
			}
			if (version != null)
				obfuscatorName += " " + version;
		}

		public override void deobfuscateBegin() {
			base.deobfuscateBegin();

			stringDecrypter.initialize();
		}


		public override void deobfuscateMethodEnd(Blocks blocks) {
			if (CanRemoveStringDecrypterType)
				stringDecrypter.deobfuscate(blocks);
			base.deobfuscateMethodEnd(blocks);
		}

		public override void deobfuscateEnd() {
			if (CanRemoveStringDecrypterType) {
				addTypeToBeRemoved(stringDecrypter.Type, "String decrypter type");
				addTypesToBeRemoved(stringDecrypter.OtherTypes, "Decrypted string type");
			}
			base.deobfuscateEnd();
		}

		public override IEnumerable<int> getStringDecrypterMethods() {
			return new List<int>();
		}
	}
}
