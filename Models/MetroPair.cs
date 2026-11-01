using System;
using System.IO;

namespace _4A_Subtitles.Models
{
  public class MetroPair
  {
    public string Name { get; set; }

    public string Description { get; set; }

    public void Read(BinaryReader reader, char[] table)
    {
      this.Name = "";
      this.Description = "";
      byte num = 1;
      while (num > (byte) 0)
      {
        num = reader.ReadByte();
        if (num > (byte) 0)
          this.Name += ((char) num).ToString();
      }
      // Description is stored as indices into 'table'. The encoding is:
      //   * a byte below 0xE0 (1..223) is the index itself (one byte);
      //   * a byte from 0xE0 up is a "page" prefix followed by a second byte
      //     (1..255), giving: index = 223 + (b - 0xE0) * 255 + nextByte.
      // Each page therefore holds 255 entries (the second byte is never 0,
      // since 0 terminates the string). This lets the table hold far more than
      // 256 characters, which Japanese (and other multibyte) localizations
      // require - stable_jp.lng has ~2376 table entries. For tables that fit in
      // a single byte (e.g. English, ~200 entries) every byte stays below 0xE0,
      // so the behaviour is identical to before.
      while (true)
      {
        byte b = reader.ReadByte();
        if (b == (byte) 0)
          break;
        int index = b < (byte) 0xE0 ? (int) b : 223 + ((int) b - 0xE0) * 255 + (int) reader.ReadByte();
        this.Description += table[index].ToString();
      }
    }

    public void Write(BinaryWriter writer, char[] table)
    {
      byte num = 0;
      foreach (char ch in this.Name)
        writer.Write((byte) ch);
      writer.Write(num);
      foreach (char ch in this.Description)
      {
        char c = ch;
        int index = Array.FindIndex<char>(table, (Predicate<char>) (x => (int) x == (int) c));
        if (index == -1)
          throw new ArgumentOutOfRangeException("c");
        // Mirror of Read: indices 1..223 fit in one byte; larger indices are
        // written as a 0xE0-based page prefix plus a second byte in 1..255.
        if (index < 224)
          writer.Write((byte) index);
        else
        {
          int v = index - 224;
          writer.Write((byte) (0xE0 + v / 255));
          writer.Write((byte) (v % 255 + 1));
        }
      }
      writer.Write(num);
    }
  }
}
