using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XMLReaderToolTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ReadBinaryFile()
        {
            //using(var strRdr = new BinaryReader(new FileStream(@"G:\kartenausschnitt.dat", FileMode.Open)))
            using (var strRdr = new BinaryReader(new FileStream(@"..\..\..\XMLReaderTool\bin\Debug\export.dat", FileMode.Open)))
            {
                //using (var strWrtr = new StreamWriter(new FileStream(@"G:\kartenausschnitt.txt", FileMode.Create)))
                using (var strWrtr = new StreamWriter(new FileStream(@".\export.osm", FileMode.Create)))
                {
                    byte[] buffer, buffer2;
                    int positionFix, separatorCount;
                    List<byte> record;

                    while ((buffer = strRdr.ReadBytes(1024)).Length > 0)
                    {
                        buffer2 = buffer.Reverse().ToArray();

                        // Find last record separator
                        separatorCount = 0;
                        for (positionFix = 0; positionFix < buffer2.Length; positionFix++)
                        {
                            if (buffer2[positionFix] != 170)
                            {
                                separatorCount = 0;
                                continue;
                            }
                            separatorCount++;
                            if (separatorCount == 5)
                                break;
                        }

                        // Fix position backward to latest record separator
                        strRdr.BaseStream.Position = strRdr.BaseStream.Position - (positionFix - 4);
                        buffer = buffer.Take(buffer.Length - (positionFix - 4)).ToArray();

                        record = new List<byte>();
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            if (i + 4 < buffer.Length && buffer[i] == 170 && buffer[i + 1] == 170 && buffer[i + 2] == 170 && buffer[i + 3] == 170 && buffer[i + 4] == 170)
                            {
                                strWrtr.WriteLine(ProcessRecord(record.ToArray()));
                                record = new List<byte>();
                                i += 4;
                                continue;
                            }

                            record.Add(buffer[i]);
                        }
                    }
                }
            }
        }

        private string ProcessRecord(byte[] rec)
        {
            List<byte> value;
            string result;

            result = "";
            value = new List<byte>();
            for (int i = 0; i < rec.Length; i++)
            {
                if (i + 3 < rec.Length && rec[i] == 170 && rec[i + 1] == 170 && rec[i + 2] == 170 && rec[i + 3] == 170)
                {
                    result = $"{result}{(value.Count == 8 ? BitConverter.ToInt64(value.ToArray(), 0) : BitConverter.ToUInt32(value.ToArray(), 0))} ";
                    value = new List<byte>();
                    i += 3;
                    continue;
                }

                value.Add(rec[i]);
            }

            result = $"{result}{(value.Count == 8 ? BitConverter.ToInt64(value.ToArray(), 0) : BitConverter.ToUInt32(value.ToArray(), 0))} ";

            return result;
        }
    }
}
