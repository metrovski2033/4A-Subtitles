using _4A_Subtitles.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace _4A_Subtitles
{
  [Serializable]
  public class MetroLocalization
  {
    public char[] MetroTable { get; set; } = new char[0];

    // Payload of the leading version chunk (4 bytes after "0, 4"). It is 0 in the
    // English localization and 1 in the Japanese one, so it must be preserved on
    // save instead of being hard-coded to 0 (otherwise an edited stable_jp.lng
    // would no longer match what the game expects).
    public int HeaderFlag { get; set; } = 0;

    public ObservableCollection<MetroPair> MetroPairs { get; set; } = new ObservableCollection<MetroPair>();

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      int num2 = reader.ReadInt32();
      if (num1 != 0 || num2 != 4)
        throw new Exception(string.Format("Chunk Ver Incorrect: '{0} {1}'", (object) num1, (object) num2));
      this.HeaderFlag = reader.ReadInt32();
      int num3 = reader.ReadInt32();
      int num4 = reader.ReadInt32();
      if (num3 != 1)
        throw new Exception(string.Format("Chunk Table Incorrect: '{0} {1}'", (object) num3, (object) num4));
      this.MetroTable = new char[num4 / 2];
      for (int index = 0; index < this.MetroTable.Length; ++index)
        this.MetroTable[index] = (char) reader.ReadInt16();
      int num5 = reader.ReadInt32();
      int num6 = reader.ReadInt32();
      if (num5 != 2)
        throw new Exception(string.Format("Chunk Pairs Incorrect: '{0} {1}'", (object) num5, (object) num6));
      this.MetroPairs = new ObservableCollection<MetroPair>();
      while (reader.PeekChar() != -1)
      {
        MetroPair metroPair = new MetroPair();
        metroPair.Read(reader, this.MetroTable);
        this.MetroPairs.Add(metroPair);
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(0);
      writer.Write(4);
      writer.Write(this.HeaderFlag);
      this.BuildMetroTable();
      writer.Write(1);
      writer.Write(this.MetroTable.Length * 2);
      foreach (char ch in this.MetroTable)
        writer.Write((short) ch);
      int offset = (int) (writer.BaseStream.Position + 4L);
      writer.Write(2);
      writer.Write(0);
      foreach (MetroPair metroPair in (Collection<MetroPair>) this.MetroPairs)
        metroPair.Write(writer, this.MetroTable);
      int num = (int) (writer.BaseStream.Position - (long) offset);
      num -= 4; // because 4 is chunk size length
      writer.Seek(offset, SeekOrigin.Begin);
      writer.Write(num);
      writer.Seek(0, SeekOrigin.End);
    }

    public void BuildMetroTable()
    {
      List<char> charList = new List<char>((IEnumerable<char>) this.MetroTable);
      foreach (MetroPair metroPair in (Collection<MetroPair>) this.MetroPairs)
      {
        foreach (char hash in metroPair.Description.ToHashSet<char>())
        {
          if (!charList.Contains(hash))
            charList.Add(hash);
        }
      }
      this.MetroTable = charList.ToArray();
      charList.Clear();
    }
  }
}
