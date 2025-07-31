//	Copyright (c) 2015, Warren Marshall <warren@warrenmarshall.biz>
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

namespace OBJ2MAP
{
    /// <summary>
    /// Compatibility class for MainForm enums and constants
    /// This maintains compatibility with the original MAPCreation classes
    /// </summary>
    public static class MainForm
    {
        public static bool bAxisAligned = false;

        public enum EGRP
        {
            Undefined,
            Grouped,
            Ungrouped,
        }

        public enum EConvOption
        {
            Standard,
            Extrusion,
            Spikes,
        }

        public enum MapVersion
        {
            Classic,
            Valve,
        }

        public enum WADOption
        {
            Auto,
            Path,
            Size,
        }
    }

    /// <summary>
    /// Interface for progress tracking during conversion
    /// This can be implemented by console, WPF, or any other UI
    /// </summary>
    public interface IProgressTracker
    {
        void UpdateProgress(string progressText = "", int value = 0);
        bool IsWadSearchSizeSelected();
        bool IsWadSearchPathSelected();
        Tuple<int, int> GetWadSearchSize();
        string GetWadSearchPath();
        string GetVisibleTextureName();
    }

    /// <summary>
    /// Console implementation of progress tracker for testing
    /// </summary>
    public class ConsoleProgressTracker : IProgressTracker
    {
        public void UpdateProgress(string progressText = "", int value = 0)
        {
            if (!string.IsNullOrEmpty(progressText))
            {
                Console.WriteLine($"Progress: {progressText} ({value}%)");
            }
        }

        public bool IsWadSearchSizeSelected() => false;
        public bool IsWadSearchPathSelected() => false;
        public Tuple<int, int> GetWadSearchSize() => Tuple.Create(64, 64);
        public string GetWadSearchPath() => "";
        public string GetVisibleTextureName() => "DEFAULT";
    }
}