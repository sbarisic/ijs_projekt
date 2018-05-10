using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace IJSDataplot {
	static class IgorConsts {
		public const int MAXDIMS = 4;

		public const int MAX_WAVE_NAME2 = 18;
		public const int MAX_WAVE_NAME5 = 31;
		public const int MAX_UNIT_CHARS = 3;
	}

	[Flags]
	enum IgorNumberType {
		Text = 0,
		Complex = 1,
		FP32 = 2,
		FP64 = 4,
		I8 = 8,
		I16 = 0x10,
		I32 = 0x20,
		Unsigned = 0x40
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public unsafe struct BinHeader5 {
		public short version;                      // Version number for backwards compatibility.
		public short checksum;                     // Checksum over this header and the wave header.
		public int wfmSize;                       // The size of the WaveHeader5 data structure plus the wave data.
		public int formulaSize;                   // The size of the dependency formula, if any.
		public int noteSize;                      // The size of the note text.
		public int dataEUnitsSize;                // The size of optional extended data units.
		public fixed int dimEUnitsSize[IgorConsts.MAXDIMS];        // The size of optional extended dimension units.
		public fixed int dimLabelsSize[IgorConsts.MAXDIMS];        // The size of optional dimension labels.
		public int sIndicesSize;                  // The size of string indicies if this is a text wave.
		public int optionsSize1;                  // Reserved. Write zero. Ignore on read.
		public int optionsSize2;                  // Reserved. Write zero. Ignore on read.
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public unsafe struct WaveHeader5 {
		public uint next;            // link to next wave in linked list.

		public uint creationDate;         // DateTime of creation.
		public uint modDate;              // DateTime of last modification.

		public int npnts;                         // Total number of points (multiply dimensions up to first zero).
		public short type;                         // See types (e.g. NT_FP64) above. Zero for text waves.
		public short dLock;                        // Reserved. Write zero. Ignore on read.

		//public fixed char whpad1[6];                     // Reserved. Write zero. Ignore on read.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] char[] whpad1;

		//int whpad1_0;
		//short whpad1_1;

		public short whVersion;                    // Write 1. Ignore on read.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IgorConsts.MAX_WAVE_NAME5 + 1)] public char[] bname;     // Name of wave plus trailing null.
		public int whpad2;                        // Reserved. Write zero. Ignore on read.
		public uint dFolder;     // Used in memory only. Write zero. Ignore on read.

		// Dimensioning info. [0] == rows, [1] == cols etc
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IgorConsts.MAXDIMS)] public int[] nDim;                 // Number of of items in a dimension -- 0 means no data.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IgorConsts.MAXDIMS)] public double[] sfA;                // Index value for element e of dimension d = sfA[d]*e + sfB[d].
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IgorConsts.MAXDIMS)] public double[] sfB;

		// SI units
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IgorConsts.MAX_UNIT_CHARS + 1)] public char[] dataUnits;         // Natural data units go here - null if none.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IgorConsts.MAXDIMS * (IgorConsts.MAX_UNIT_CHARS + 1))] public char[] dimUnits;  // Natural dimension units go here - null if none.

		public short fsValid;                      // TRUE if full scale values have meaning.
		public short whpad3;                       // Reserved. Write zero. Ignore on read.
		public double topFullScale, botFullScale;  // The max and max full scale value for wave.

		public uint dataEUnits;                  // Used in memory only. Write zero. Ignore on read.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IgorConsts.MAXDIMS)] public uint[] dimEUnits;          // Used in memory only. Write zero. Ignore on read.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = IgorConsts.MAXDIMS)] public uint[] dimLabels;          // Used in memory only. Write zero. Ignore on read.

		public uint waveNoteH;                   // Used in memory only. Write zero. Ignore on read.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public int[] whUnused;                  // Reserved. Write zero. Ignore on read.

		// The following stuff is considered private to Igor.

		public short aModified;                    // Used in memory only. Write zero. Ignore on read.
		public short wModified;                    // Used in memory only. Write zero. Ignore on read.
		public short swModified;                   // Used in memory only. Write zero. Ignore on read.

		public char useBits;                       // Used in memory only. Write zero. Ignore on read.
		public char kindBits;                      // Reserved. Write zero. Ignore on read.
		public uint formula;                     // Used in memory only. Write zero. Ignore on read.
		public int depID;                         // Used in memory only. Write zero. Ignore on read.

		public short whpad4;                       // Reserved. Write zero. Ignore on read.
		public short srcFldr;                      // Used in memory only. Write zero. Ignore on read.
		public uint fileName;                    // Used in memory only. Write zero. Ignore on read.

		public uint sIndices;                    // Used in memory only. Write zero. Ignore on read.

		//public float wData;                     // The start of the array of data. Must be 64 bit aligned.
	}

	public unsafe static class IBWLoader {
		public static IBWFile Load(string FilePath) {
			using (BinaryReader Reader = new BinaryReader(File.OpenRead(FilePath))) {
				return Load(Reader);
			}
		}

		public static IBWFile Load(BinaryReader Reader) {
			IBWFile IBWFile = new IBWFile();

			short Version = Reader.ReadInt16();
			bool ReorderBytes = (Version & 0xFF) == 0;

			if (ReorderBytes)
				throw new Exception("Endianness not supported");

			if (Version != 5)
				throw new Exception("Version " + Version + " not supported");

			Reader.BaseStream.Seek(0, SeekOrigin.Begin);

			int WaveHeader5Size = Marshal.SizeOf<WaveHeader5>();
			int BinHeader5Size = sizeof(BinHeader5);


			// Read headers
			BinHeader5 Header = Reader.ReadStruct<BinHeader5>();
			WaveHeader5 WaveHeader5 = Reader.ReadStruct<WaveHeader5>();

			IBWFile.CreationDate = WaveHeader5.creationDate.MacTimestampToDateTime();
			IBWFile.ModifyDate = WaveHeader5.modDate.MacTimestampToDateTime();

			IBWFile.WaveName = new string(WaveHeader5.bname);
			IgorNumberType Type = (IgorNumberType)WaveHeader5.type;

			// Read the wave data
			Reader.BaseStream.Seek(BinHeader5Size + WaveHeader5Size, SeekOrigin.Begin);
			int NumOfPoints = WaveHeader5.npnts;
			int WaveDataSize = Header.wfmSize - WaveHeader5Size;
			int NumOfDimensions = WaveHeader5.nDim.Where((N) => N > 0).Count();

			if (NumOfDimensions != 3)
				throw new Exception("Unexpected number of dimensions " + NumOfDimensions);


			byte[] WaveData = Reader.ReadBytes(WaveDataSize);

			IBWFile.Width = WaveHeader5.nDim[0];
			IBWFile.Height = WaveHeader5.nDim[1];
			IBWFile.Depth = WaveHeader5.nDim[2];

			object[] Data = null;

			if (Type == IgorNumberType.FP32) {
				Data = new object[WaveData.Length / 4];

				fixed (byte* WaveDataPtr = WaveData) {
					for (int i = 0; i < Data.Length; i++) {
						Data[i] = *(float*)(&WaveDataPtr[i * 4]);
					}

				}
			} else
				throw new NotImplementedException("Unknown igor type " + Type);

			// Read notes
			IBWFile.NoteData = Encoding.UTF7.GetString(Reader.ReadBytes(Header.noteSize));

			IBWFile.Data = Data;
			IBWFile.DataBytes = WaveData;
			return IBWFile;
		}
	}

	public class IBWFile {
		public DateTime CreationDate;
		public DateTime ModifyDate;

		public string WaveName;
		public string NoteData;

		public byte[] DataBytes;
		public object[] Data;

		public int Width, Height, Depth;

		public object GetData(int X, int Y, int Z) {
			return Data[(Z * Width * Height) + (Y * Width + X)];
		}
	}
}
